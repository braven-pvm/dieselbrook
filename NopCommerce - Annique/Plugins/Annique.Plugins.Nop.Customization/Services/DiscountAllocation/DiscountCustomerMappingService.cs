using Annique.Plugins.Nop.Customization.Domain.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Models.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Services.PrivateMessagePopUp;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.DiscountAllocation
{
    public class DiscountCustomerMappingService : IDiscountCustomerMappingService
    {
        #region Fields

        private readonly IRepository<DiscountCustomerMapping> _discountCustomerMappingRepository;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountUsage> _discountUsageRepository;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly ICustomPrivateMessageService _customPrivateMessageService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public DiscountCustomerMappingService(IRepository<DiscountCustomerMapping> discountCustomerMappingRepository,
            IProductService productService,
            CatalogSettings catalogSettings,
            ICategoryService categoryService,
            IRepository<Discount> discountRepository,
            IRepository<DiscountUsage> discountUsageRepository,
            IStoreContext storeContext,
            IWorkContext workContext,
            ILogger logger,
            IOrderService orderService,
            ICustomPrivateMessageService customPrivateMessageService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IStaticCacheManager staticCacheManager)
        {
            _discountCustomerMappingRepository = discountCustomerMappingRepository;
            _productService = productService;
            _catalogSettings = catalogSettings;
            _categoryService = categoryService;
            _discountRepository = discountRepository;
            _discountUsageRepository = discountUsageRepository;
            _storeContext = storeContext;
            _workContext = workContext;
            _logger = logger;
            _orderService = orderService;
            _customPrivateMessageService = customPrivateMessageService;
            _customerService = customerService;
            _localizationService = localizationService;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilites

        /// <summary>
        /// Gets Products ids In Discount
        /// </summary>
        /// <param name="discount">discount</param>
        /// <param name="orderItems">order items</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task returns product's Id from order items and which are connected to discounts
        /// </returns>
        private async Task<IEnumerable<int>> GetProductsInDiscountAsync(Discount discount, IList<OrderItem> orderItems)
        {
            if (discount.DiscountType.Equals(DiscountType.AssignedToSkus))
            {
                //get discounted product's Id
                var discountedProduct = (await _productService.GetProductsWithAppliedDiscountAsync(discount.Id)).Select(x => x.Id);
                return discountedProduct.ToList();
            }

            var result = new List<int>();
            if (orderItems.Count > 0)
            {
                var productIds = orderItems.Select(x => x.ProductId).ToArray();
                //get product categy ids
                var productCategoriesIds = await _categoryService.GetProductCategoryIdsAsync(productIds);

                var allcategories = new List<int>();
                //get categories where discount is applied
                var categories = (await _categoryService.GetCategoriesByAppliedDiscountAsync(discount.Id)).Select(x => x.Id);
                allcategories.AddRange(categories);
                foreach (var item in categories)
                {
                    //get child category and add into category list
                    allcategories.AddRange(await _categoryService.GetChildCategoryIdsAsync(item));
                }

                //check order item's any product's categoryId match with discounted category Id
                //If matched then take product id from cart and make a list 
                result = await productCategoriesIds
                            ?.Where(kv => kv.Value.Intersect(allcategories).Any())
                            .Select(kv => kv.Key)
                            .ToListAsync();
            }

            return result.ToHashSet();
        }

        /// <summary>
        /// Gets Cheapest product from order items
        /// </summary>
        /// <param name="discountAppliedProductIds">discount Applied ProductIds</param>
        /// <param name="orderItems">order items</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task returns cheapest product's Id from order items
        /// </returns>
        private async Task<int> GetCheapeastProductAsync(IEnumerable<int> discountAppliedProductIds, IList<OrderItem> orderItems)
        {
            //get products by ids
            var products = await _productService.GetProductsByIdsAsync(discountAppliedProductIds.ToArray());

            // Check if productsdiscount is null or empty
            if (products == null || !products.Any())
            {
                // no products are found return 0
                return 0;
            }

            // if the order item is null or empty
            if (orderItems == null || !orderItems.Any())
            {
                // the order item is empty by returning 0
                return 0;
            }

            var minPrice = products.Max(p => p.Price);
            var result = 0;
            foreach (var item in orderItems)
            {
                if (products.Any(p => p.Id.Equals(item.ProductId)))
                {
                    (minPrice, result) = products
                        .Where(p => p.Id.Equals(item.ProductId))
                        .FirstOrDefault()
                        .Price <= minPrice ?
                            (products
                                .Where(p => p.Id.Equals(item.ProductId))
                                .FirstOrDefault()
                                .Price, item.ProductId) : (minPrice, result);
                }
            }
            return result;
        }

        /// <summary>
        /// Get Get OrderItem DiscountDetailsAsync
        /// </summary>
        /// <param name="orderItemsWithProducts">Order items with product pair</param>
        ///<param name="discountCustomerMappings">Discount customer mappings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// returns orderItemId , discount amount , discount Id applied on order items
        /// </returns>
        public async Task<List<(int OrderItemId, decimal DiscountAmount, int discountId)>> GetOrderItemDiscountDetailsAsync(IEnumerable<(OrderItem OrderItem, Product Product)> orderItemsWithProducts, IList<DiscountCustomerMapping> discountCustomerMappings)
        {
            var discountDetails = new List<(int OrderItemId, decimal DiscountAmount, int DiscountId)>();

            //do not inject IPriceCalculationService via constructor because it'll cause circular references
            var _priceCalculationService = EngineContext.Current.Resolve<IPriceCalculationService>();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //do not inject IDiscountService via constructor because it'll cause circular references
            var _discountService = EngineContext.Current.Resolve<IDiscountService>();

            var orderItems = orderItemsWithProducts.Select(oi => oi.OrderItem).ToList();
            
            // Precalculate cheapest product IDs for mapped discounts
            foreach (var mappedDiscount in discountCustomerMappings)
            {
                var discount = await _discountService.GetDiscountByIdAsync(mappedDiscount.DiscountId);
                if (discount.DiscountType == DiscountType.AssignedToSkus || discount.DiscountType == DiscountType.AssignedToCategories || discount.DiscountType == DiscountType.AssignedToManufacturers)
                {
                    //get discount applied product ids
                    var discountAppliedProductIds = await GetProductsInDiscountAsync(discount, orderItems);
                    //get cheapest product id
                    var cheapestProduct = await GetCheapeastProductAsync(discountAppliedProductIds, orderItems);

                    if (cheapestProduct != 0)
                    {
                        // Add order item discount details if the order item matches the cheapest product
                        var orderItem = orderItems.FirstOrDefault(oi => oi.ProductId == cheapestProduct);
                        discountDetails.Add((orderItem.Id, discount.DiscountAmount, discount.Id));
                    }
                }
            }

            foreach (var (orderItem, product) in orderItemsWithProducts)
            {
                // Get applied discounts from product final price
                var (_, _, _, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, decimal.Zero, true, 1);

                foreach (var appliedDiscount in appliedDiscounts)
                {
                    //get special mapped discount
                    var mappedDiscount = discountCustomerMappings?.FirstOrDefault(dc => dc.DiscountId == appliedDiscount.Id);

                    if (mappedDiscount == null)
                    {
                        var discountAmount = _discountService.GetDiscountAmount(appliedDiscount, product.Price);
                        discountDetails.Add((orderItem.Id, discountAmount * orderItem.Quantity, appliedDiscount.Id));
                    }
                }
            }

            return discountDetails;
        }

        //manage updation of discount customer mapping no of time usage 
        protected async Task ManageDiscountCustomerMappingUsageAsync(DiscountCustomerMapping mappingToUpdate)
        {
            if (mappingToUpdate != null)
            {
                // Update no times used
                mappingToUpdate.NoTimesUsed++;
                if (mappingToUpdate.NoTimesUsed == mappingToUpdate.LimitationTimes)
                    mappingToUpdate.IsActive = false;

                // Update discount customer mapping
                await UpdateDiscountCustomerMappingAsync(mappingToUpdate);

                #region bug 606 Voucher for 100% Discount on Product

                // Remove cache when a voucher is used to prevent users from utilizing it more than the allowed number of times.
                // This is necessary to avoid issues caused by caching, which could lead to multiple redemptions of the same voucher.
                // By clearing the cache upon voucher usage, we ensure that the user can only redeem the voucher according to the specified limitations.
                await _staticCacheManager.RemoveAsync(AnniqueCustomizationDefaults.DiscountCustomerMappingsCacheKey, mappingToUpdate.DiscountId, mappingToUpdate.CustomerId);
                await _staticCacheManager.RemoveAsync(AnniqueCustomizationDefaults.DiscountCustomerMappingAllCacheKey, mappingToUpdate.CustomerId);

                #endregion

                //handle private message
                await _customPrivateMessageService.HandlePrivateMessageAsync(discountId: mappingToUpdate.Id);
            }
        }

        //handle discount usage entry for discount type shipping
        protected async Task HandleShippingDiscountUsage(Order order, DiscountUsageHistory discountUsageHistory, DiscountCustomerMapping mappingToUpdate, decimal discountAmount)
        {
            var discountUsageToAdd = new DiscountUsage
            {
                DiscountUsageHistoryId = discountUsageHistory.Id,
                OrderId = order.Id,
                DiscountAmount = discountAmount,
                DiscountCustomerMappingId = mappingToUpdate?.Id
            };

            //insert entry in ANQ_DiscountUsage table
            await InsertDiscountUsageAsync(discountUsageToAdd);
        }

        //handle discount usage entry for discount type order total
        protected async Task HandleOrderTotalDiscountUsage(Order order, DiscountUsageHistory discountUsageHistory, DiscountCustomerMapping mappingToUpdate)
        {
            var discountUsageToAdd = new DiscountUsage
            {
                DiscountUsageHistoryId = discountUsageHistory.Id,
                OrderId = order.Id,
                DiscountAmount = order.OrderDiscount,
                DiscountCustomerMappingId = mappingToUpdate?.Id
            };
            //insert entry in ANQ_DiscountUsage table
            await InsertDiscountUsageAsync(discountUsageToAdd);
        }

        //handle discount usage entry for mapped/unmapped product level discounts applied on order item
        private async Task HandleProductDiscountUsage(Order order, DiscountUsageHistory discountUsageHistory, DiscountCustomerMapping mappingToUpdate, List<(int OrderItemId, decimal DiscountAmount, int discountId)> discountDetails)
        {
            // Retrieve the matching discount details for the given discount usage
            var matchingDiscountDetails = discountDetails.Where(dd => dd.discountId == discountUsageHistory.DiscountId);
            foreach (var matchingDiscount in matchingDiscountDetails)
            {
                var discountUsageToAdd = new DiscountUsage
                {
                    DiscountUsageHistoryId = discountUsageHistory.Id,
                    OrderId = order.Id,
                    OrderItemId = matchingDiscount.OrderItemId,
                    DiscountAmount = matchingDiscount.DiscountAmount,
                    DiscountCustomerMappingId = mappingToUpdate?.Id
                };
                //insert entry in ANQ_DiscountUsage table
                await InsertDiscountUsageAsync(discountUsageToAdd);
            }
        }

        protected static List<int> GetMappingToUpdateIdsFromNote(string note)
        {
            var json = note.Replace("Special Discount Customer Mapping Ids: ", "");
            return JsonConvert.DeserializeObject<List<int>>(json);
        }

        protected async Task AddOrderNoteAsync(Order order, string note)
        {
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        
        #endregion

        #region Method

        /// <summary>
        /// Returns Wheather discount customer mapping 
        /// </summary>
        /// <param name="discountId">Discount Identifier</param>
        ///<param name="customerId"> Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns discount customer mapping
        public async Task<DiscountCustomerMapping> GetDiscountCustomerMappingAsync(int discountId, int customerId)
        {
            // Stage 1: Filter based on DiscountId, CustomerId, and IsActive
            var initialQuery = await _discountCustomerMappingRepository.Table
                .Where(dcm => dcm.DiscountId == discountId
                              && dcm.CustomerId == customerId
                              && dcm.IsActive)
                .ToListAsync();

            // If no records found in the initial stage, return null
            if (!initialQuery.Any())
            {
                return null;
            }

            // Stage 2: Further filter the results based on the date and usage conditions
            var finalResult = initialQuery
                .FirstOrDefault(dcm => dcm.StartDateUtc <= DateTime.UtcNow
                                       && dcm.EndDateUtc >= DateTime.UtcNow
                                       && dcm.NoTimesUsed < dcm.LimitationTimes);

            return finalResult;
        }

        /// <summary>
        /// Returns all discount customer mappings
        /// </summary>
        ///<param name="customerId"> Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns discount customer mappings
        public async Task<IList<DiscountCustomerMapping>> GetAllDiscountCustomerMappingsAsync(int customerId)
        {
            var nowUtc = DateTime.UtcNow;

            var discountMappings = (await _discountCustomerMappingRepository.GetAllAsync(query =>
            {
                //filter by customer
                query = query.Where(dcm => dcm.CustomerId == customerId);

                //filter by active and start date end date
                query = query.Where(dcm => dcm.IsActive && nowUtc >= (dcm.StartDateUtc ?? DateTime.MinValue) &&
                           nowUtc <= (dcm.EndDateUtc ?? DateTime.MaxValue) && dcm.NoTimesUsed < dcm.LimitationTimes);

                return query;
            }, cache => cache.PrepareKeyForDefaultCache(AnniqueCustomizationDefaults.DiscountCustomerMappingAllCacheKey,
             customerId)))
            .AsQueryable();

            // select unique mappings based on DiscountId
            var uniqueMappings = discountMappings?.DistinctBy(dcm => dcm.DiscountId).ToList() ?? new List<DiscountCustomerMapping>();

            return uniqueMappings;
        }

        /// <summary>
        /// Get discount customer mapping by discount id
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// </param>
        /// <returns>
        /// The task result returns true if discount customer mapping exist
        /// </returns>
        public async Task<bool> IsExitDiscountCustomerMappingByDiscountIdAsync(int discountId)
        {
            return await _discountCustomerMappingRepository.Table.AnyAsync(dcm => dcm.DiscountId == discountId);
        }

        /// <summary>
        /// Gets discount names
        /// </summary>
        /// <param name="customer">customer</param>
        /// <param name="shoppingCartModel">Shopping cart model</param>
        /// <param name="discountCustomerMappings">Discount customer mapping</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the discounts names
        /// </returns>
        public async Task<(IList<AvailableDiscountModel>, IList<string>, bool HasAutoApplied)> GetDiscountNamesAsync(Customer customer, ShoppingCartModel shoppingCartModel, IList<DiscountCustomerMapping> discountCustomerMappings)
        {
            // 1. Get all discount models available to the customer
            var availableDiscountModels = await GetAvailableDiscountNamesAsync(discountCustomerMappings);

            var appliedDiscountNames = new List<string>();
            bool hasAutoAppliedDiscount = false;

            var discountService = EngineContext.Current.Resolve<IDiscountService>();

            // 2. Get applied coupon codes from the customer (safe list)
            var couponCodes = (await _customerService.ParseAppliedDiscountCouponCodesAsync(customer))
                                .Where(code => !string.IsNullOrWhiteSpace(code))
                                .Select(code => code.Trim())
                                .ToList();

            // Copy because we will mutate the original list
            var toEvaluate = availableDiscountModels.ToList();

            foreach (var model in toEvaluate)
            {
                var discount = model.Discount;

                // -------------------------------------------------------
                // A. AUTO-APPLIED DISCOUNTS (no coupon code needed)
                // -------------------------------------------------------
                if (!discount.RequiresCouponCode &&
                    (discount.DiscountType == DiscountType.AssignedToOrderTotal ||
                     discount.DiscountType == DiscountType.AssignedToOrderSubTotal ||
                     discount.DiscountType == DiscountType.AssignedToShipping))
                {
                    hasAutoAppliedDiscount = true;
                    model.ShowRadioButton = false;

                    var validation = await discountService.ValidateDiscountAsync(discount, customer);
                    if (validation.IsValid)
                    {
                        appliedDiscountNames.Add(model.Name);
                        availableDiscountModels.Remove(model); // remove auto-applied
                    }

                    continue;
                }

                // -------------------------------------------------------
                // B. COUPON-BASED DISCOUNTS (use parsed coupon codes)
                // -------------------------------------------------------
                if (discount.RequiresCouponCode)
                {
                    // SAFE check — handles empty list, trims, ignores case
                    bool couponMatched =
                        couponCodes.Any(code =>
                            code.Equals(discount.CouponCode, StringComparison.OrdinalIgnoreCase));

                    if (couponMatched)
                    {
                        var validation = await discountService.ValidateDiscountAsync(discount, customer);
                        if (validation.IsValid)
                        {
                            appliedDiscountNames.Add(model.Name);
                            availableDiscountModels.Remove(model); // remove applied discount
                        }
                    }
                }
            }

            return (availableDiscountModels, appliedDiscountNames, hasAutoAppliedDiscount);
        }

        public async Task<IList<AvailableDiscountModel>> GetAvailableDiscountNamesAsync(IList<DiscountCustomerMapping> discountCustomerMappings)
        {
            // Do not inject IDiscountService via constructor because it'll cause circular references
            var _discountService = EngineContext.Current.Resolve<IDiscountService>();

            var discounts = await _discountService.GetAllDiscountsAsync();

            var query = from dcm in discountCustomerMappings
                        join d in discounts on dcm.DiscountId equals d.Id
                        select new AvailableDiscountModel
                        {
                            Name = $"{d.Name} (Expires: {dcm.EndDateUtc:dd/MM/yyyy})",
                            CouponCode = d.CouponCode ,
                            DiscountType = d.DiscountType,
                            Discount = d
                        };

            return await query.ToListAsync();
        }

        public async Task<IList<string>> GetAppliedDiscountsAsync(Customer customer)
        {
            var appliedDiscountNames = new HashSet<string>();

            var _discountService = EngineContext.Current.Resolve<IDiscountService>();

            // 1. Get all applied coupon codes
            var couponCodes = (await _customerService.ParseAppliedDiscountCouponCodesAsync(customer))
                                .ToList();

            if (!couponCodes.Any())
                return appliedDiscountNames.ToList();

            // 2. Single DB call: load all discounts matching ANY of those coupon codes
            var allDiscounts = await _discountService.GetAllDiscountsAsync();

            // 3. Filter only discounts that match the applied coupon codes
            var discountsForCodes = allDiscounts
                .Where(d => d.RequiresCouponCode &&
                            couponCodes.Equals(d.CouponCode))
                .ToList();

            // 4. Validate each discount only once
            foreach (var discount in discountsForCodes)
            {
                var validation = await _discountService.ValidateDiscountAsync(discount, customer);
                if (validation.IsValid)
                    appliedDiscountNames.Add(discount.Name);
            }

            return appliedDiscountNames.ToList();
        }

        /// <summary>
        /// Update Discount customer mapping
        /// </summary>
        /// <param name="discountCustomerMapping">Discount customer mapping</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task UpdateDiscountCustomerMappingAsync(DiscountCustomerMapping discountCustomerMapping)
        {
            await _discountCustomerMappingRepository.UpdateAsync(discountCustomerMapping);
        }

        /// <summary>
        /// Insert Discount usage
        /// </summary>
        /// <param name="discountUsage">Discount Usage</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task InsertDiscountUsageAsync(DiscountUsage discountUsage)
        {
            await _discountUsageRepository.InsertAsync(discountUsage);
        }

        /// <summary>
        /// Get Discount customer mapping
        /// </summary>
        /// <param name="discountCustomerMappingId">Discount Customer mapping id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task<DiscountCustomerMapping> GetDiscountCustomerMappingByIdAsync(int discountCustomerMappingId)
        {
            return await _discountCustomerMappingRepository.GetByIdAsync(discountCustomerMappingId);
        }

        /// <summary>
        /// Handle order discounts and discount usage entries
        /// </summary>
        /// <param name="order">Order</param>
        ///<param name="orderItems">Order items</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task HandleOrderDiscountsAsync(Order order, IEnumerable<OrderItem> orderItems)
        {
            try
            {
                orderItems = orderItems.Where(oi => oi.DiscountAmountInclTax > decimal.Zero && string.IsNullOrEmpty(oi.AttributesXml));

                //do not inject IDiscountService via constructor because it'll cause circular references
                var _discountService = EngineContext.Current.Resolve<IDiscountService>();

                //get all duh from nopcommerce discount usage history table
                var duh = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);

                //get discount customer mappings
                var discountCustomerMappings = await GetAllDiscountCustomerMappingsAsync(order.CustomerId);

                if (duh.Any())
                {
                    //product ids 
                    var productIdsWithDiscounts = orderItems.Select(oi => oi.ProductId).Distinct().ToArray();

                    //get all products
                    var products = await _productService.GetProductsByIdsAsync(productIdsWithDiscounts);

                    //pair order item with respective products
                    var orderItemsWithProducts = from orderItem in orderItems
                                                 join product in products on orderItem.ProductId equals product.Id
                                                 select (OrderItem: orderItem, Product: product);

                    // Create a list of discount IDs from the duh list
                    var duhDiscountIds = duh.Select(d => d.DiscountId).ToList();

                    // Filter discountCustomerMappings where the DiscountId is in duhDiscountIds
                    var filteredDiscountCustomerMappings = discountCustomerMappings?.Where(dcm => duhDiscountIds.Contains(dcm.DiscountId)).ToList();

                    //get order item's discount details
                    var discountDetails = await GetOrderItemDiscountDetailsAsync(orderItemsWithProducts, filteredDiscountCustomerMappings);

                    var mappingToUpdateIds = new List<int>();

                    foreach (var d in duh)
                    {
                        //get discount 
                        var discount = await _discountService.GetDiscountByIdAsync(d.DiscountId);
                        var discountType = discount.DiscountType;

                        //get mapping for updation from discount customer mapping if discount customer mapping is not null
                        var mappingToUpdate = filteredDiscountCustomerMappings?.FirstOrDefault(dcm => dcm.DiscountId == discount.Id);
                        if (mappingToUpdate != null)
                        {
                            //handle updation of discount customer mapping no of time usage 
                            await ManageDiscountCustomerMappingUsageAsync(mappingToUpdate);
                            mappingToUpdateIds.Add(mappingToUpdate.Id);
                        }

                        if (discountType == DiscountType.AssignedToShipping)
                        {
                             // Handle discount applied to shipping
                            await HandleShippingDiscountUsage(order, d, mappingToUpdate, discount.DiscountAmount);
                        }
                        else if (discountType == DiscountType.AssignedToOrderTotal)
                        {
                            // Handle discount applied to order total
                            await HandleOrderTotalDiscountUsage(order, d, mappingToUpdate);
                        }
                        else
                        {
                            // product-level discounts entry in discount usage table
                            await HandleProductDiscountUsage(order, d, mappingToUpdate, discountDetails);
                        }
                    }

                    // Insert order note with prefix "Special Discounts" and JSON of MappingToUpdate ids
                    if (mappingToUpdateIds.Any())
                    {
                        var note = $"Special Discount Customer Mapping Ids: {JsonConvert.SerializeObject(mappingToUpdateIds)}";
                        await AddOrderNoteAsync(order, note);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception 
                await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            }
        }

        /// <summary>
        /// Restore special discount mappings On Order Cancellation
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderNotes">Order notes</param>
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task RestoreSpecialDiscountOnOrderCancellationAsync(Order order, IList<OrderNote> orderNotes)
        {
            var restoredIds = new List<int>();
            try
            {
                //get discount note
                var discountNote = orderNotes.FirstOrDefault(n => n.Note.StartsWith("Special Discount Customer Mapping Ids: "))?.Note;

                if (discountNote != null)
                {
                    //get ids from note
                    var mappingToUpdateIds = GetMappingToUpdateIdsFromNote(discountNote);

                    foreach (var mappingId in mappingToUpdateIds)
                    {
                        var mappingToUpdate = await GetDiscountCustomerMappingByIdAsync(mappingId);

                        if (mappingToUpdate != null)
                        {
                            mappingToUpdate.NoTimesUsed--;
                            mappingToUpdate.IsActive = true;

                            // Update discount customer mapping
                            await UpdateDiscountCustomerMappingAsync(mappingToUpdate);

                            restoredIds.Add(mappingId);
                        }
                    }
                }

                // Add note with restored ids
                if (restoredIds.Any())
                {
                    var note = $"Restored Special Discount Customer Mapping ids: {JsonConvert.SerializeObject(restoredIds)}";
                    await AddOrderNoteAsync(order, note);
                }
            }
            catch (Exception ex)
            {
                // Log the exception 
                await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            }
        }

        public async Task HandleSpecialDiscountCodeApplicationAsync(Customer customer, string discountCouponCode, ShoppingCartModel model)
        {
            discountCouponCode = discountCouponCode?.Trim();

            if (string.IsNullOrEmpty(discountCouponCode))
                return;

            // Check if the discount can be applied
            (var canApplyDiscountCode, var discount) = await CanApplySpecialDiscountAsync(customer, discountCouponCode);

            if (!canApplyDiscountCode)
            {
                if (model != null && model.DiscountBox.IsApplied)
                {
                    // Remove applied discount as current user has no access
                    await _customerService.RemoveDiscountCouponCodeAsync(customer, discountCouponCode);

                    // Update discount box model properties
                    model.DiscountBox.IsApplied = false;

                    // Retrieve localized messages
                    var couponAppliedMessage = await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied");
                    var enteredCouponCodeMessage = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.CurrentCode"), discountCouponCode);
                    var specialDiscountErrorMessage = await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.SpecialDiscountAccess.Error");

                    // Remove specific messages from the model
                    model.DiscountBox.Messages.RemoveAll(m =>
                        m.Contains(couponAppliedMessage) ||
                        m == enteredCouponCodeMessage
                    );

                    // Add the message indicating the special discount rule
                    model.DiscountBox.Messages.Add(specialDiscountErrorMessage);
                }

                return;
            }
            else 
            {
                if (model != null && model.DiscountBox.IsApplied && discount?.DiscountType == DiscountType.AssignedToSkus)
                {
                    bool productAdded = await HandleDiscountSkuAddToCartAsync(customer, discount);
                    if (productAdded)
                    {
                        // Only refresh cart items
                        var _shoppingCartModelFactory = EngineContext.Current.Resolve<IShoppingCartModelFactory>();
                        var _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();

                        var store = await _storeContext.GetCurrentStoreAsync();
                        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                        // Prepare only the items list — lightweight and efficient
                        var tempModel = new ShoppingCartModel();
                        tempModel = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(tempModel, cart);

                        // Just update the items collection
                        model.Items = tempModel.Items;
                    }
                }
            }
        }

        public async Task<(bool applyAccess, Discount discount)> CanApplySpecialDiscountAsync(Customer customer, string discountCouponCode)
        {
            //do not inject IDiscountService via constructor because it'll cause circular references
            var discountService = EngineContext.Current.Resolve<IDiscountService>();

            // 1. Find the valid discount for the coupon
            var discount = (await discountService.GetAllDiscountsAsync(couponCode: discountCouponCode)).FirstOrDefault();

            if (discount == null)
                return (false, null);

            // 2. Check if customer already has mapping for this discount
            var mappings = await GetAllDiscountCustomerMappingsAsync(customer.Id);

            bool customerHasDiscount = mappings.Any(cm => cm.DiscountId == discount.Id);

            if (customerHasDiscount)
            {
                // 1. Get all applied coupon codes
                var couponCodes = (await _customerService.ParseAppliedDiscountCouponCodesAsync(customer))
                                    .ToList();

                if (couponCodes.Any() && couponCodes.Count >= 2)
                { 
                    //remove old voucher code
                    var oldCoupon = couponCodes.First();
                    await _customerService.RemoveDiscountCouponCodeAsync(customer, oldCoupon);
                }
            }

            return (true, discount);
        }


        private async Task<bool> HandleDiscountSkuAddToCartAsync(Customer customer, Discount discount)
        {
            var _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();

            bool productAdded = false;

            var store = await _storeContext.GetCurrentStoreAsync();

            //1.check the discount
            if (discount == null)
                return false;

            // 2. Check if discount applies to specific products (SKUs)
            var appliedToProducts = await _productService.GetProductsWithAppliedDiscountAsync(discount.Id);
            if (!appliedToProducts?.Any() ?? true)
                return false;

            // 3. Loop through all products tied to the discount
            foreach (var product in appliedToProducts)
            {
                // Check if already in the cart
                var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                var alreadyInCart = cartItems.Any(ci => ci.ProductId == product.Id);

                if (!alreadyInCart)
                {
                    // 4. Add product automatically to cart
                    var addToCartWarnings = await _shoppingCartService.AddToCartAsync(
                        customer,
                        product,
                        ShoppingCartType.ShoppingCart,
                        store.Id,
                        quantity: 1, 
                        attributesXml: string.Empty
                    );

                    //log warnings
                    if (addToCartWarnings.Any())
                        await _logger.WarningAsync(string.Join(", ", addToCartWarnings), null, await _workContext.GetCurrentCustomerAsync());
                    else
                        productAdded = true;
                }
            }

            return productAdded;
        }

        #endregion
    }
}
