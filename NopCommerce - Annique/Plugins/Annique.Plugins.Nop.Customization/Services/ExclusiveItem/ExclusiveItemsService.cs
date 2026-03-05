using Annique.Plugins.Nop.Customization.Domain;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ExclusiveItem
{
    /// <summary>
    /// ExclusiveItemsService service
    /// </summary>
    public class ExclusiveItemsService : IExclusiveItemsService
    {
        #region Fields

        protected readonly IRepository<ExclusiveItems> _exclusiveItemsRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<Category> _categoryRepository;
        protected readonly IRepository<Product> _productRepository;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public ExclusiveItemsService(IRepository<ExclusiveItems> exclusiveItemsRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<Category> categoryRepository,
            IRepository<Product> productRepository,
             IWorkContext workContext,
            ILogger logger,
            IStaticCacheManager staticCacheManager,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _exclusiveItemsRepository = exclusiveItemsRepository;
            _productCategoryRepository = productCategoryRepository;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _workContext = workContext;
            _logger = logger;
            _staticCacheManager = staticCacheManager;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns Wheather customer can access exclusive category of not
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if customer is exist in exclusive table otherwise returns false
        public bool CanAccessExclusiveCategory(int customerId)
        {
            var query = from e in _exclusiveItemsRepository.Table
                        where e.CustomerID == customerId && e.IActive == true
                        select e;

            if (query.Any())
                return true;

            return false;
        }

        /// <summary>
        /// Search Exclusive products
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Exclusive products
        /// </returns>
        public async Task<IEnumerable<Product>> SearchExclusiveProductsAsync(int customerId)
        {
            // Step 1: Start by filtering the exclusive items by customer ID
            var exclusiveItemRecords = _exclusiveItemsRepository.Table
                                       .Where(e => e.CustomerID == customerId);

            // Step 2: Apply the other filters one by one
            exclusiveItemRecords = exclusiveItemRecords
                                   .Where(e => e.IActive == true)
                                   .Where(e => e.nQtyLimit != e.nQtyPurchased)
                                   .Where(e => DateTime.UtcNow >= (e.dFrom ?? DateTime.MinValue))
                                   .Where(e => DateTime.UtcNow <= (e.dTo ?? DateTime.MaxValue))
                                   .Where(e => e.IForce == false);

            // Step 3: Check if any records match the criteria before proceeding
            if (!await exclusiveItemRecords.AnyAsync())
                return Enumerable.Empty<Product>();

            // Step 4: Join the filtered exclusive items with products
            var exclusiveProductsQuery = from exclusiveItem in exclusiveItemRecords
                                         join product in _productRepository.Table
                                         on exclusiveItem.ProductID equals product.Id
                                         select product;

            // Execute query and return result as IEnumerable<Product>
            return await exclusiveProductsQuery.ToListAsync();
        }

        /// <summary>
        /// Search Force Add to Cart Exclusive products
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the Forced Add to cart Exclusive products
        /// </returns>
        public IList<ExclusiveItems> SearchForceAddToCartExclusiveItems(int customerId)
        {
            var exclusiveItems = from e in _exclusiveItemsRepository.Table
                                 where e.CustomerID == customerId
                                       && e.IActive == true
                                       && e.nQtyLimit != e.nQtyPurchased
                                       && DateTime.UtcNow >= (e.dFrom ?? DateTime.MinValue)
                                       && DateTime.UtcNow <= (e.dTo ?? DateTime.MaxValue)
                                       && e.IForce == true
                                 select e;

            return exclusiveItems.ToList();
        }

        /// <summary>
        /// Get Exclusive Item
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the Exclusive Item 
        /// </returns>
        public async Task<ExclusiveItems> GetExclusiveItemAsync(int productId, int customerId)
        {
            return await _exclusiveItemsRepository.Table
                .Where(ei => ei.ProductID == productId &&
                             ei.CustomerID == customerId &&
                             ei.IActive == true &&
                             DateTime.UtcNow >= (ei.dFrom ?? DateTime.MinValue) &&
                             DateTime.UtcNow <= (ei.dTo ?? DateTime.MaxValue))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Exclusive Item allocated
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the true if item allocated else returns false
        /// </returns>
        public async Task<bool> IsExclusiveItemAllocatedAsync(int productId, int customerId)
        {
            // Prepare a cache key using productId and customerId
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                AnniqueCustomizationDefaults.IsAllocatedExclusiveItemCacheKey, productId, customerId);

            var now = DateTime.Now;

            // Try to get the result from the cache
            var cachedResult = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                // If not found in cache, execute the query to check if the exclusive item exists
                return await _exclusiveItemsRepository.Table
                    .AnyAsync(ei => ei.ProductID == productId &&
                                    ei.CustomerID == customerId &&
                                    ei.IActive == true);
            });

            return cachedResult;
        }

        /// <summary>
        /// Get Allocated Exclusive Item
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains exclusive item
        /// </returns>
        public async Task<ExclusiveItems> GetAllocatedExclusiveItemAsync(int productId, int customerId)
        {
            var now = DateTime.Now;

            var exclusiveItem = await _exclusiveItemsRepository.GetAllAsync(query =>
            {
                return query.Where(ei => ei.ProductID == productId &&
                             ei.CustomerID == customerId &&
                             ei.IActive == true &&
                             ei.nQtyLimit != ei.nQtyPurchased &&
                             now >= (ei.dFrom ?? DateTime.MinValue) &&
                             now <= (ei.dTo ?? DateTime.MaxValue));

            }, cache => cache.PrepareKeyForShortTermCache(AnniqueCustomizationDefaults.GetAllocatedExclusiveItemsCacheKey, productId, customerId));

            return exclusiveItem?.FirstOrDefault();
        }

        /// <summary>
        /// Gets a product category mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="customer">Customer</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product category mapping collection
        /// </returns>
        public async Task<IList<ProductCategory>> GetProductCategoriesByProductIdAsync(int productId, int storeId,
            Customer customer ,bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductCategory>();

            return await _productCategoryRepository.GetAllAsync(query =>
            {
                if (!showHidden)
                {
                    var categoriesQuery = _categoryRepository.Table.Where(c => c.Published);

                    query = query.Where(pc => categoriesQuery.Any(c => !c.Deleted && c.Id == pc.CategoryId));
                }

                return query
                    .Where(pc => pc.ProductId == productId)
                    .OrderBy(pc => pc.DisplayOrder)
                    .ThenBy(pc => pc.Id);

            }, cache => _staticCacheManager.PrepareKeyForDefaultCache(AnniqueCustomizationDefaults.ExclusiveProductCategoriesByProductCacheKey,
                productId, showHidden, customer, storeId));
        }

        /// <summary>
        /// Update Exclusive Item
        /// </summary>
        /// <param name="exclusiveItems">Exclusive Item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateExclusiveItemAsync(ExclusiveItems exclusiveItems)
        {
            await _exclusiveItemsRepository.UpdateAsync(exclusiveItems);
        }

        /// <summary>
        /// Search Exclusive Items
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Exclusive Items
        /// </returns>
        public async Task<IList<ExclusiveItems>> SearchStarterKitExclusiveItemsAsync(int customerId)
        {
            var exclusiveItems = from e in _exclusiveItemsRepository.Table
                                 where e.CustomerID == customerId
                                    && e.IActive == true
                                    && e.nQtyLimit != e.nQtyPurchased
                                    && DateTime.UtcNow >= (e.dFrom ?? DateTime.MinValue)
                                    && DateTime.UtcNow <= (e.dTo ?? DateTime.MaxValue)
                                    && e.IForce == false
                                    && e.IStarter == true
                                 select e;

            return await exclusiveItems.ToListAsync();
        }

        /// <summary>
        /// Returns Wheather product is Force exclusive product or not
        /// </summary>
        /// <param name="productId">product Identifier</param>
        /// <param name="customerId">customer Identifier</param>
        /// The task result returns true if product is Force exclusive product else return false
        public async Task<bool> IsForceExclusiveProductAsync(int productId, int customerId)
        {
            var exclusiveItemsForCustomer = await _exclusiveItemsRepository.Table
                                            .Where(e => e.CustomerID == customerId)
                                            .ToListAsync();

            // If there are records for the customer, check for productId
            if (exclusiveItemsForCustomer.Any(e => e.ProductID == productId && e.IActive == true && e.IForce == true && e.IStarter == false))
                return true;

            return false;
        }

        /// <summary>
        /// Returns Wheather product is exclusive product or not
        /// </summary>
        /// <param name="productId">product Identifier</param>
        /// <param name="customerId">customer Identifier</param>
        /// The task result returns true if product is exclusive product else return false
        public bool IsStarterExclusiveProduct(int productId, int customerId)
        {
            var exclusiveItemsForCustomer = _exclusiveItemsRepository.Table
                                            .Where(e => e.CustomerID == customerId)
                                            .ToList();

            // If there are records for the customer, check for productId
            if (exclusiveItemsForCustomer.Any(e => e.ProductID == productId && e.IActive == true && e.IStarter == true))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns exclusive items 
        /// </summary>
        /// <param name="productIds">product Identifier</param>
        /// The task result returns exclusive items
        public async Task<IList<ExclusiveItems>> GetExclusiveItemsByProductIdsAsync(IEnumerable<int> productIds)
        {
            #region 610 Issue: Product removed from cart once order placed

            // Updated query to include a search within a specified date range, 
            // ensuring expired exclusive products are excluded from the results.
            var productIdSet = new HashSet<int>(productIds);
            return await _exclusiveItemsRepository.Table
                         .Where(e => productIdSet.Contains((int)e.ProductID)
                         && DateTime.UtcNow >= (e.dFrom ?? DateTime.MinValue)
                         && DateTime.UtcNow <= (e.dTo ?? DateTime.MaxValue))
                        .Distinct()
                        .ToListAsync();
            #endregion
        }

        /// <summary>
        /// Returns Wheather starter product exist in cart or not
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// The task result returns true if product is exist in cart else return false
        public bool IsStarterKitExistInCart(IList<ShoppingCartItem> cart)
        {
            var query = from c in cart
                        join e in _exclusiveItemsRepository.Table on c.CustomerId equals e.CustomerID
                        where c.ProductId == e.ProductID
                        && e.IActive == true
                        && e.IStarter == true
                        select e;

            if (query.Any())
                return true;

            return false;
        }

        /// <summary>
        /// Get Exclusive Item by Id
        /// </summary>
        /// <param name="Id">Product identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the Exclusive Item 
        /// </returns>
        public async Task<ExclusiveItems> GetExclusiveItemByIdAsync(int id)
        {
            return await _exclusiveItemsRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Restore ExclusiveItems On Order Cancellation
        /// </summary>
        /// <param name="order">Order</param>
        ///<param name="orderNotes">Order notes</param>
        /// </param>
        /// <returns>
        /// </returns>
        public async Task RestoreExclusiveItemsOnOrderCancellationAsync(Order order, IList<OrderNote> orderNotes)
        {
            // Do not inject IDiscountService via constructor because it'll cause circular references
            var _orderService = EngineContext.Current.Resolve<IOrderService>();

            //get exclusive note 
            var exclusiveItemNote = orderNotes.FirstOrDefault(n => n.Note.StartsWith("Exclusive Item Note:"))?.Note;
            if (string.IsNullOrEmpty(exclusiveItemNote))
                return;

            //get details from exclusive note
            var orderNoteDetails = GetOrderNoteDetails(exclusiveItemNote);
            if (orderNoteDetails == null || !orderNoteDetails.Any())
                return;

            //list of updated exclusive item
            var updatedOrderNoteDetails = new List<JToken>();

            foreach (var noteDetail in orderNoteDetails)
            {
                var exclusiveItemId = (int)noteDetail["ExclusiveItemId"];
                var orderItemQuantity = (int)noteDetail["OrderItemQuantity"];

                //get exclusive item by id
                var exclusiveItem = await GetExclusiveItemByIdAsync(exclusiveItemId);
                if (exclusiveItem != null)
                {
                    exclusiveItem.nQtyPurchased -= orderItemQuantity;

                    if (exclusiveItem.nQtyPurchased < exclusiveItem.nQtyLimit)
                        exclusiveItem.IActive = true;

                    //update exclusive item
                    await UpdateExclusiveItemAsync(exclusiveItem);

                    updatedOrderNoteDetails.Add(noteDetail);
                }
            }

            //after updation prepare and insert note with restored exclusive items
            if (updatedOrderNoteDetails.Any())
            {
                var combinedNote = new JArray(updatedOrderNoteDetails).ToString();

                combinedNote = "Exclusive items re-instated: " + combinedNote;

                await AddOrderNoteAsync(order,combinedNote);
            }
        }

        private static JArray GetOrderNoteDetails(string exclusiveItemNote)
        {
            var json = exclusiveItemNote.Replace("Exclusive Item Note: ", "");
            return JArray.Parse(json);
        }

        /// <summary>
        /// Handle exclusive items on order place event
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItems">Order items</param>
        /// </param>
        public async Task HandleExclusiveItemsOnOrderPlaceAsync(Order order, IEnumerable<OrderItem> orderItems)
        {
            try
            {
                // Do not inject IOrderService via constructor because it'll cause circular references
                var _orderService = EngineContext.Current.Resolve<IOrderService>();
                var _productService = EngineContext.Current.Resolve<IProductService>();
                var _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                var _orderProcessingService = EngineContext.Current.Resolve<IOrderProcessingService>();

                // Get distinct ProductIds from orderItems
                var productIds = orderItems.Select(oi => oi.ProductId).Distinct();

                // Initialize a list to store order note details
                List<string> orderNoteDetails = new List<string>();

                var exclusiveProducts = await GetExclusiveItemsByProductIdsAsync(productIds);
                if (exclusiveProducts.Any())
                {
                    // Create a set of product IDs from the exclusive products for quick lookup
                    var exclusiveProductIds = new HashSet<int>(exclusiveProducts.Select(ep => ep.ProductID.Value));

                    // Filter order items where the ProductId matches any of the exclusive product IDs
                    var exclusiveOrderItems = orderItems.Where(o => exclusiveProductIds.Contains(o.ProductId)).ToList();

                    foreach (var exclusiveOrderItem in exclusiveOrderItems)
                    {
                        var exclusiveItem = await GetExclusiveItemAsync(exclusiveOrderItem.ProductId, order.CustomerId);
                        if (exclusiveItem != null)
                        {
                            var qtyPurchased = (exclusiveItem.nQtyPurchased == null) ? 0 : exclusiveItem.nQtyPurchased;

                            //Update qtypuchased in exclusive table with order item quanity
                            qtyPurchased += exclusiveOrderItem.Quantity;

                            exclusiveItem.nQtyPurchased = qtyPurchased;

                            //Check for exclusive item limit and qty purchased
                            if (exclusiveItem.nQtyLimit == exclusiveItem.nQtyPurchased)
                                //Make exclusive product active to false so next time user can not see and order it
                                exclusiveItem.IActive = false;

                            //Update Exclsuive Item
                            await UpdateExclusiveItemAsync(exclusiveItem);

                            // Prepare order note details in JSON format
                            var orderNoteDetail = new
                            {
                                OrderItemId = exclusiveOrderItem.Id,
                                ExclusiveItemId = exclusiveItem.Id,
                                OrderItemQuantity = exclusiveOrderItem.Quantity
                            };

                            // Serialize order note detail object to JSON
                            string orderNoteDetailJson = JsonConvert.SerializeObject(orderNoteDetail);

                            // Add serialized order note detail to the list
                            orderNoteDetails.Add(orderNoteDetailJson);
                        }
                        else
                        {
                            var product = await _productService.GetProductByIdAsync(exclusiveOrderItem.ProductId);

                            //adjust inventory
                            await _productService.AdjustInventoryAsync(product, exclusiveOrderItem.Quantity, exclusiveOrderItem.AttributesXml,
                                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.DeleteOrderItem"), order.Id));

                            //delete item
                            await _orderService.DeleteOrderItemAsync(exclusiveOrderItem);

                            //update order totals
                            var updateOrderParameters = new UpdateOrderParameters(order, exclusiveOrderItem);
                            await _orderProcessingService.UpdateOrderTotalsAsync(updateOrderParameters);
                        }
                    }
                }

                if (orderNoteDetails.Count > 0)
                {
                    // Combine order note details into a single JSON array
                    string orderNote = "[" + string.Join(",", orderNoteDetails) + "]";

                    // Add a prefix to the order note
                    orderNote = "Exclusive Item Note: " + orderNote;

                    // Insert the order note into the order note table
                    await AddOrderNoteAsync(order, orderNote);
                }
            }
            catch (Exception ex)
            {
                // Log the exception 
                await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            }
        }

        protected async Task AddOrderNoteAsync(Order order, string note)
        {
            // Do not inject IOrderService via constructor because it'll cause circular references
            var _orderService = EngineContext.Current.Resolve<IOrderService>();

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Handle exclusive items on product details page
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// </param>
        /// If product is exclusive , then checks for is it allocated to user or not 
        /// If not allocated to that user then return user to home page
        public async Task<IActionResult> HandleExclusiveItemsAsync(int productId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //get product categories
            var productCategories = await GetProductCategoriesByProductIdAsync(productId, store.Id, customer);

            //if products belongs to exclusive category
            if (productCategories.Any(pc => pc.CategoryId == settings.ExclusiveItemsCategoryId))
            {
                //check for exclusive product is allocated or not
                var isAllocatedExclusiveItem = await IsExclusiveItemAllocatedAsync(productId, customer.Id);
                if (!isAllocatedExclusiveItem)
                    return new RedirectToRouteResult("Homepage", null);
            }

            return null; // No redirect, continue processing
        }

        #endregion
    }
}
