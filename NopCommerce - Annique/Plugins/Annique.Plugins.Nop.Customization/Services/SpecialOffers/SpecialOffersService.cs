using Annique.Plugins.Nop.Customization.Domain.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Domain.SpecialOffers;
using Annique.Plugins.Nop.Customization.Models.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
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
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Annique.Plugins.Nop.Customization.Services.SpecialOffers
{
    public class SpecialOffersService : ISpecialOffersService
    {
        #region Fields

        private readonly IRepository<Offers> _offersRepository;
        private readonly IDiscountService _discountService;
        private readonly IRepository<OfferList> _offerListRepository;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductService _productService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;

        #endregion

        #region Properties

        protected string RootElementName { get; set; } = "Attributes";

        protected string ChildElementName { get; set; } = "SpecialOfferAttribute";

        #endregion

        #region Ctor

        public SpecialOffersService(IRepository<Offers> offersRepository,
            IDiscountService discountService,
            IRepository<OfferList> offerListRepository,
            IRepository<Discount> discountRepository,
            IWorkContext workContext,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            IProductService productService,
            IPriceCalculationService priceCalculationService,
            ICustomerService customerService,
            IStoreContext storeContext,
            ILogger logger)
        {
            _offersRepository = offersRepository;
            _discountService = discountService;
            _offerListRepository = offerListRepository;
            _discountRepository = discountRepository;
            _workContext = workContext;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _productService = productService;
            _priceCalculationService = priceCalculationService;
            _customerService = customerService;
            _storeContext = storeContext;
            _logger = logger;
        }

        #endregion

        #region Methods

        #region Offer and offer list related methods

        /// <summary>
        /// Gets active special offers and associated discounts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer and associated discount
        public async Task<IList<(Offers, Discount)>> GetActiveSpecialOfferListAsync()
        {
            // Get the current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            var key = _staticCacheManager.PrepareKeyForShortTermCache(AnniqueCustomizationDefaults.ActiveSpecialOffersAllCacheKey, customer.Id);

            var result = await _staticCacheManager.GetAsync(key, async () =>
            {
                var list = new List<(Offers, Discount)>();

                // Query to join offers with discounts
                var query = from o in _offersRepository.Table
                            join d in _discountRepository.Table on o.DiscountId equals d.Id
                            select new { Offer = o, Discount = d };

                var queryList = await query.ToListAsync();

                if (!queryList.Any())
                    return list;

                // Validate each discount and add valid offers to the result
                foreach (var item in queryList)
                {
                    var validationResult = await _discountService.ValidateDiscountAsync(item.Discount, customer);
                    if (validationResult.IsValid)
                    {
                        list.Add((item.Offer, item.Discount));
                    }
                }

                return list;
            });

            return result;
        }

        /// <summary>
        /// Gets a offer by id
        /// </summary>
        /// <param name="offerId">offer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer 
        /// </returns>
        public async Task<Offers> GetOfferByIdAsync(int offerId)
        {
            return await _offersRepository.GetByIdAsync(offerId, cache => default);
        }

        /// <summary>
        /// Gets a offers by ids
        /// </summary>
        /// <param name="offerIds">offer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer list 
        /// </returns>
        public async Task<IList<Offers>> GetOffersByIdsAsync(int[] offerIds)
        {
            return await _offersRepository.GetByIdsAsync(offerIds, cache => default);
        }

        /// <summary>
        /// Gets a offer list
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer list 
        /// </returns>
        public async Task<IList<OfferList>> GetAllOfferListAsync()
        {
            return await _offerListRepository.GetAllAsync(query =>
            {
                return from ol in query orderby ol.Id select ol;
            }, _ => default);
        }

        /// <summary>
        /// Returns all product ids by list type and offer id
        /// </summary>
        /// <param name="offerId">offer Id</param>
        /// <param name="listType">List type of product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns product ids based on list type and offer id 
        public async Task<IEnumerable<int>> GetProductIdsByOfferTypeAsync(int offerId, string listType)
        {
            var offerLists = (await _offerListRepository.GetAllAsync(query =>
            {
                // Filter by offer id and list type
                return query.Where(ol => ol.OfferId == offerId && ol.ListType.Equals(listType));

            }, cache => cache.PrepareKeyForDefaultCache(AnniqueCustomizationDefaults.OfferListAllCacheKey,
             listType, offerId)))
           .AsQueryable();

            return offerLists?.Select(ol => ol.ProductId).Distinct() ?? Enumerable.Empty<int>();
        }

        /// <summary>
        /// Returns total of offer Type F products
        /// </summary>
        /// <param name="cart">cart items</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns total 
        public async Task<decimal> EvaluateFProductTotalsAsync(IEnumerable<ShoppingCartItem> cart)
        {
            // Get distinct product IDs from the cart items
            var distinctProductIds = cart.Select(item => item.ProductId).Distinct().ToArray();

            // Fetch the product details for these IDs to get the prices
            var products = await _productService.GetProductsByIdsAsync(distinctProductIds);

            // Calculate the total value of matching cart items
            var totalValue = cart.Sum(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                return product != null ? product.Price * item.Quantity : 0;
            });

            return totalValue;
        }

        /// <summary>
        /// Returns offer valid or not
        /// </summary>
        /// <param name="offer">Offer</param>
        /// <param name="productIds">Product ids</param>
        /// <param name="cart">Shopping cart</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if offer rule satisfied otherwise return false
        public async Task<bool> IsOfferValidForCartAsync(Offers offer, IEnumerable<int> productIds, IList<ShoppingCartItem> cart)
        {
            #region Tasl 642 Special offer - on order total

            if (offer.MinValueOnTotalRsp) 
            {
                // Filter cart items have empty AttributesXml

                var allCartItems = cart.Where(item =>
                {
                    var xml = item.AttributesXml;
                    if (string.IsNullOrEmpty(xml))
                        return true;

                    var status = ParseSpecialOfferStatus(xml);
                    return status.HasSpecialOffer && !status.IsFullyFree;
                });

                // ✅ Evaluate total of eligible items
                var totalValue = await EvaluateFProductTotalsAsync(allCartItems);

                return totalValue >= offer.MinValue;
            }

            #endregion

            // Check if productIds is not empty
            if (!productIds.Any())
                return false;

            // Filter cart items that match the product IDs and have empty AttributesXml
            var cartItems = cart.Where(item => productIds.Contains(item.ProductId) && string.IsNullOrEmpty(item.AttributesXml));
            var totalQty = cartItems.Sum(item => item.Quantity);

            if (offer.MinQty > 0)
            {
                return totalQty >= offer.MinQty;
            }

            // If the offer is validated by minimum value
            if (offer.MinValue > 0)
            {
                var totalValue = await EvaluateFProductTotalsAsync(cartItems);

                return totalValue >= offer.MinValue;
            }

            // If both MinQty and MinValue are 0, apply the third rule type
            if (offer.MinQty == 0 && offer.MinValue == 0)
            {
                // Ensure the customer has at least one of each F list product in the cart
                var uniqueProductCountInCart = cartItems.Select(item => item.ProductId).Distinct().Count();

                // Validate if all F list products are present in the cart
                if (uniqueProductCountInCart == productIds.Count())
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        #endregion

        #region Special Offer Cart methods

        /// <summary>
        /// check provided product belongs to provied offerType or not
        /// </summary>
        /// <param name="productId">Product Identifier</param>
        /// <param name="offerType">Offer type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if product belong to offer type else return false
        public async Task<bool> IsProductInOfferTypeAsync(int productId, string offerType)
        {
            var offerList = await GetAllOfferListAsync();
            if (offerList.Any())
            {
                var query = offerList.Where(ol => ol.ProductId == productId && ol.ListType == offerType);
                return query.Any();
            }

            return false;
        }

        /// <summary>
        /// Get OfferId by productId
        /// </summary>
        /// <param name="productId">Product Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns offer Id
        public async Task<int> GetOfferIdByProductAsync(int productId)
        {
            var offerList = await GetAllOfferListAsync();

            // Filter the cached records by product ID
            return offerList.Where(ol => ol.ProductId == productId)
                            .Select(ol => ol.OfferId)
                            .FirstOrDefault();
        }

        /// <summary>
        /// handle G products when F product removed from cart
        /// </summary>
        /// <param name="sci">Shopping cart item</param>
        /// <param name="cart">Shopping cart</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task removes associated G products from cart when F type product removed from cart

        public async Task HandleGProductsOnFProductRemovalAsync(ShoppingCartItem sci, IList<ShoppingCartItem> cart)
        {
            if (ContainsSpecialOfferAttribute(sci.AttributesXml))
                return;

            var _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();

            #region Task 642 special offer on total RSP

            // Iterate through all cart items to check for IsTotalRspOffer attribute in AttributesXml
            var itemsToRemove = cart.Where(c => IsTotalRspOfferInAttributesXml(c.AttributesXml)).ToList();

            foreach (var cartItem in itemsToRemove)
            {
                // Remove the item from the cart
                await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem);
                cart.Remove(cartItem);
            }

            #endregion

            // Handle removal for F products
            if (await IsProductInOfferTypeAsync(sci.ProductId, "F"))
            {
                var offerId = await GetOfferIdByProductAsync(sci.ProductId);
              
                var cartItemsToRemove = cart.Where(c => IsMatchingSpecialOfferAttribute(c.AttributesXml, offerId))
                                    .ToList();

                foreach (var cartItem in cartItemsToRemove)
                {
                    await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem);
                }
            }
        }

        //check standards warnings for G products before this product displayed in special offer product
        public async Task<bool> HasStandardWarningsAsync(Customer customer, Product product, int storeId, int quantity)
        {
            bool warnings = false;

            // Check if the product is deleted
            if (product.Deleted)
                return true;

            // Check if the product is not published
            if (!product.Published)
                return true;

            // Check store mapping authorization
            if (!await _storeMappingService.AuthorizeAsync(product, storeId))
                return true;

            // Check availability dates
            if (product.AvailableStartDateTimeUtc.HasValue && product.AvailableStartDateTimeUtc.Value > DateTime.UtcNow)
                return true;

            if (product.AvailableEndDateTimeUtc.HasValue && product.AvailableEndDateTimeUtc.Value < DateTime.UtcNow)
                return true;

            // Check maximum quantity that can be added
            var maximumQuantityCanBeAdded = await _productService.GetTotalStockQuantityAsync(product);

            //quantity is already qty into cart
            //maximum qty represent how much max stock qty can be added
            maximumQuantityCanBeAdded = maximumQuantityCanBeAdded - quantity;
            if (maximumQuantityCanBeAdded <= 0)
                return true;

            return warnings;
        }

        //calculate special discount first, then calculate other standard discounts on discounted unit price
        public async Task<(decimal finalPrice, decimal totalDiscountAmount, List<Discount> appliedDiscounts)> ApplySpecialAndStandardDiscountsAsync(Product product, string attributesXml, decimal basePrice, List<Discount> standardDiscounts)
        {
            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<Discount>();
            var finalPrice = basePrice;

            var specialDiscountIds = ParseAttributeDiscountIds(attributesXml);
            if (specialDiscountIds.Any())
            {
                var specialDiscount = await _discountService.GetDiscountByIdAsync(specialDiscountIds.FirstOrDefault());

                if (specialDiscount != null)
                {
                    var (priceAfterSpecialDiscount, specialDiscountAmount) = CalculateDiscountedPrice(specialDiscount, product.Price, product.Price);
                    finalPrice = priceAfterSpecialDiscount;
                    discountAmount += specialDiscountAmount;
                    appliedDiscounts.Add(specialDiscount);

                    if (finalPrice != decimal.Zero)
                    {
                        foreach (var discount in standardDiscounts)
                        {
                            var (priceAfterDiscount, normalDiscountAmount) = CalculateDiscountedPrice(discount, finalPrice, finalPrice);
                            finalPrice = priceAfterDiscount;
                            discountAmount += normalDiscountAmount;
                            appliedDiscounts.Add(discount);
                        }
                    }
                }
            }

            return (finalPrice, discountAmount, appliedDiscounts);
        }

        /// <summary>
        /// Calculate special discounted price and discount amout 
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="unitPriceWithDiscount">Unit price with discount</param>
        /// <param name="unitPriceWithoutDiscount">Unit price without discount</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task returns special unit price and special discount amount
        public (decimal, decimal) CalculateDiscountedPrice(Discount discount, decimal unitPriceWithDiscount, decimal unitPriceWithoutDiscount)
        {
            if (discount.UsePercentage && discount.DiscountPercentage == 100m)
            {
                return (decimal.Zero, decimal.Zero);
            }

            var appliedDiscounts = new List<Discount> { discount };
            var preferredDiscount = _discountService.GetPreferredDiscount(appliedDiscounts, unitPriceWithoutDiscount, out var discountAmount);
            if (preferredDiscount != null)
            {
                return (unitPriceWithDiscount - discountAmount, discountAmount);
            }
            return (decimal.Zero, decimal.Zero);
        }

        /// <summary>
        /// Calculate allowed selection quantity
        /// </summary>
        /// <param name="offers">Offers</param>
        /// <param name="productIds">Product id of offer Type F</param>
        /// <param name="cart">Cart</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task returns allowed selection quantities for offer items
        public async Task<int> CalculateAllowedSelectionsAsync(Offers offers, IEnumerable<int> productIds, IList<ShoppingCartItem> cart)
        {
            //task 642 Special offer - on order total
            // Helper local function to apply MaxLimit
            int CapToMaxLimit(int value)
            {
                if (value < 1)
                    return 0;

                // Only apply max limit if set (greater than 0)
                return (offers.MaxAllowedQty > 0)
                    ? Math.Min(value, offers.MaxAllowedQty)
                    : value;
            }

            // Filter cart items that match the product IDs and have empty AttributesXml
            var cartItems = cart.Where(item => productIds.Contains(item.ProductId) && string.IsNullOrEmpty(item.AttributesXml));

            int allowedSelections = 0;

            // If the offer is validated by minimum quantity
            if (offers.MinQty > 0)
            {
                // Calculate the total quantity of matching cart items
                var totalQty = cartItems.Sum(item => item.Quantity);

                allowedSelections = (int)Math.Floor((double)totalQty / offers.MinQty) * offers.MaxQty;
               
                return CapToMaxLimit(allowedSelections);
            }

            // If the offer is validated by minimum value
            if (offers.MinValue > 0 && !offers.MinValueOnTotalRsp)
            {
                var totalValue = await EvaluateFProductTotalsAsync(cartItems);

                allowedSelections = (int)Math.Floor(totalValue / offers.MinValue) * offers.MaxQty;
               
                return CapToMaxLimit(allowedSelections);
            }

            if (offers.MinQty == 0 && offers.MinValue == 0)
            {
                if (cartItems.Count() == productIds.Count())
                {
                    // Calculate the minimum quantity of any F list item
                    var minQtyOfFListItems = cartItems.Min(item => item.Quantity);

                    // The allowed selections from the G list is proportional to the smallest F list item quantity
                    allowedSelections = offers.MaxQty * minQtyOfFListItems;
                }
                return CapToMaxLimit(allowedSelections);
               
            }

            // If the offer is validated by minimum value
            if (offers.MinValue > 0 && offers.MinValueOnTotalRsp)
            {
                var cartItemsForTotalRsp = cart.Where(item =>
                {
                    var xml = item.AttributesXml;
                    if (string.IsNullOrEmpty(xml))
                        return true;

                    var status = ParseSpecialOfferStatus(xml);
                    return status.HasSpecialOffer && !status.IsFullyFree;
                });

                var totalValue = await EvaluateFProductTotalsAsync(cartItemsForTotalRsp);
                
                allowedSelections = (int)Math.Floor(totalValue / offers.MinValue) * offers.MaxQty;
             
                return CapToMaxLimit(allowedSelections);
            }

            return allowedSelections;
        }

        /// <summary>
        /// Adjust allowed selection based on G items in cart
        /// </summary>
        /// <param name="offers">Offers</param>
        /// <param name="allowedSelections">Allowed selection</param>
        /// <param name="cart">Cart</param>
        /// The task returns allowed selection quantities with adjusting already in cart quantities 
        public int AdjustAllowedSelectionsBasedOnCartGProducts(Offers offer, int allowedSelections, IList<ShoppingCartItem> cart)
        {
            // Filter cart items where AttributeXml matches the special offer attributes
            var gProductCartItems = cart
                                    .Where(c => IsMatchingSpecialOfferAttribute(c.AttributesXml, offer.Id))
                                    .ToList();

            // If no matching items found, return allowedSelections as it is
            if (!gProductCartItems.Any())
                return allowedSelections;

            // Calculate the sum of quantities of the matching items
            var selectedGProductsQty = gProductCartItems.Sum(c => c.Quantity);

            // Adjust allowedSelections by subtracting the selectedGProductsQty
            allowedSelections -= selectedGProductsQty;

            // Ensure allowedSelections is not negative
            return Math.Max(0, allowedSelections);
        }

        /// <summary>
        /// Validate and adjust G products on cart
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task validate and handles G products on cart 
        public async Task ValidateAndAdjustGProductQtyInCartAsync(IList<ShoppingCartItem> cart, Customer customer)
        {
            var activeSpecialOffers = await GetActiveSpecialOfferListAsync();
            if (activeSpecialOffers.Any())
            {
                foreach (var specialOffer in activeSpecialOffers)
                {
                    await ValidateAndAdjustGProductQtyInCartAsync(cart, specialOffer.Item1, customer);
                }
            }
        }

        /// <summary>
        /// Validate and adjust G products on cart at checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="offer">Special Offer</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task validate and handles G products on cart at checkout
        public async Task ValidateAndAdjustGProductQtyInCartAsync(IList<ShoppingCartItem> cart, Offers offer, Customer customer)
        {
            var _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();

            var productIdsF = await GetProductIdsByOfferTypeAsync(offer.Id, "F");

            // Step 1: Calculate the allowed selection quantity
            var allowedSelections = await CalculateAllowedSelectionsAsync(offer, productIdsF, cart);

            // Step 2: Get cart items where AttributeXml is not null and matches SpecialAttributeXml, and sum their quantities
            var gProductCartItems = cart
                                    .Where(c => IsMatchingSpecialOfferAttribute(c.AttributesXml, offer.Id))
                                    .ToList();

            if (!gProductCartItems.Any())
                return;

            var selectedGProductsQty = gProductCartItems.Sum(c => c.Quantity);

            // Step 3: If the total quantity exceeds the allowed selection, adjust the cart quantities
            if (selectedGProductsQty > allowedSelections)
            {
                var excessQty = selectedGProductsQty - allowedSelections;

                // Reduce quantities until excessQty is zero
                while (excessQty > 0)
                {
                    for (int i = 0; i < gProductCartItems.Count; i++)
                    {
                        if (excessQty <= 0)
                            break;

                        var cartItem = gProductCartItems[i];
                        if (cartItem.Quantity > 0)
                        {
                            cartItem.Quantity--;
                            excessQty--;
                        }
                    }
                }


                // Step 4: Update cart items with new quantities
                foreach (var cartItem in gProductCartItems)
                {
                    await _shoppingCartService.UpdateShoppingCartItemAsync(customer, cartItem.Id, cartItem.AttributesXml, cartItem.CustomerEnteredPrice, cartItem.RentalStartDateUtc, cartItem.RentalEndDateUtc, cartItem.Quantity, true);
                }
            }
        }

        #endregion

        #region Attribute Xml Methods

        /// <summary>
        /// Gets selected attribute discount ids
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Selected attribute discountid</returns>
        public virtual IList<int> ParseAttributeDiscountIds(string attributesXml)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(attributesXml))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var elements = xmlDoc.SelectNodes(@$"//{RootElementName}/{ChildElementName}");

                if (elements == null)
                    return Array.Empty<int>();

                foreach (XmlNode node in elements)
                {
                    if (node.Attributes?["DiscountId"] == null)
                        continue;

                    var attributeValue = node.Attributes["DiscountId"].InnerText.Trim();
                    if (int.TryParse(attributeValue, out var id))
                        ids.Add(id);
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return ids;
        }

        /// <summary>
        /// Gets status for special offer
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Returns if has special offer and is it fully free or not</returns>
        public SpecialOfferStatus ParseSpecialOfferStatus(string attributesXml)
        {
            if (string.IsNullOrEmpty(attributesXml))
                return new SpecialOfferStatus { HasSpecialOffer = false };

            try
            {
                var xdoc = XDocument.Parse(attributesXml);
                var offerNode = xdoc.Descendants("SpecialOfferAttribute").FirstOrDefault();

                if (offerNode == null)
                    return new SpecialOfferStatus { HasSpecialOffer = false };

                var isFullyFreeAttr = offerNode.Attribute("IsFullyFree")?.Value?.Trim();

                return new SpecialOfferStatus
                {
                    HasSpecialOffer = true,
                    IsFullyFree = bool.TryParse(isFullyFreeAttr, out var result) && result
                };
            }
            catch
            {
                return new SpecialOfferStatus { HasSpecialOffer = false };
            }
        }

        /// <summary>
        /// Check for SpecialOffer product attribute in attributeXml
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Returns true if contains special attribute else return false</returns>
        public bool ContainsSpecialOfferAttribute(string attributeXml)
        {
            if (!string.IsNullOrEmpty(attributeXml))
            {
                XDocument xdoc = XDocument.Parse(attributeXml);
                return xdoc.Descendants("SpecialOfferAttribute").Any();
            }
            return false;
        }

        /// <summary>
        /// Is total rsp offer in attribute xml
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Returns true is total rsp offer exist in cart items attribute xml</returns>
        public bool IsTotalRspOfferInAttributesXml(string attributesXml)
        {
            if (string.IsNullOrEmpty(attributesXml))
                return false;

            try
            {
                var xdoc = XDocument.Parse(attributesXml);
                var offerNode = xdoc.Descendants("SpecialOfferAttribute").FirstOrDefault();

                if (offerNode == null)
                    return false;

                var isTotalRspOffer = offerNode.Attribute("IsTotalRspOffer")?.Value?.Trim();

                return bool.TryParse(isTotalRspOffer, out var result) && result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Is matching Special attribute by offer Id
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="offerId">Offer id</param>
        /// <returns>Returns true for attribute match with offer id</returns>
        public bool IsMatchingSpecialOfferAttribute(string attributesXml, int offerId)
        {
            if (string.IsNullOrEmpty(attributesXml))
                return false;

            try
            {
                var xdoc = XDocument.Parse(attributesXml);

                var matchingNode = xdoc.Descendants("SpecialOfferAttribute")
                    .FirstOrDefault(x =>
                    {
                        var idAttr = x.Attribute("ID")?.Value;
                        return int.TryParse(idAttr, out int id) && id == offerId;
                    });

                return matchingNode != null;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Adds an special offer attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="offerId">Offer id</param>
        /// <param name="discountId">discount id</param>
        /// <returns>Updated result (XML format)</returns>
        public async Task<string> AddSpecialOfferAttributeAsync(string attributesXml, int offerId, int discountId)
        {
            var result = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributesXml))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributesXml);
                }

                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                XmlElement attributeElement = null;
               
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/SpecialOfferAttribute");

                var offer = await GetOfferByIdAsync(offerId);

                //get discount by discount id
                var discount = await _discountService.GetDiscountByIdAsync(discountId);

                var isFullyFree = discount.UsePercentage && discount.DiscountPercentage == 100m;

                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != offerId)
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                //create new one if not found
                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("SpecialOfferAttribute");
                    attributeElement.SetAttribute("ID", offerId.ToString());
                    attributeElement.SetAttribute("DiscountId", discountId.ToString());
                    attributeElement.SetAttribute("IsFullyFree", isFullyFree ? "true" : "false");
                    attributeElement.SetAttribute("IsTotalRspOffer", offer.MinValueOnTotalRsp ? "true" : "false");
                    rootElement.AppendChild(attributeElement);
                }
                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }


         #endregion

        /// <summary>
        /// save special offer inside discount usage table
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItems">order items</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task will add entry inside ANQ_Discountusage table for special offer products
        public async Task SaveSpecialOfferDiscountUsageHistoryAsync(Order order, IList<OrderItem> orderItems)
        {
            try
            {
                var _discountCustomerMappingService = EngineContext.Current.Resolve<IDiscountCustomerMappingService>();

                // Step 1: Get cart items where AttributesXml contains special offer attribute and UnitPrice is not decimal 0
                var cartItemsWithSpecialOffer = orderItems
                    .Where(item => ContainsSpecialOfferAttribute(item.AttributesXml) && item.UnitPriceInclTax > 0)
                    .ToList();

                // Step 2: If no cart items match, do nothing
                if (!cartItemsWithSpecialOffer.Any())
                    return;

                // Step 3: Get existing DiscountUsageHistory for the current order
                var discountUsageHistoryList = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                var store = await _storeContext.GetCurrentStoreAsync();

                //product ids 
                var productIds = cartItemsWithSpecialOffer.Select(oi => oi.ProductId).Distinct().ToArray();

                //get all products
                var products = await _productService.GetProductsByIdsAsync(productIds);

                // Step 4: Iterate over each matching cart item
                foreach (var orderItem in cartItemsWithSpecialOffer)
                {
                    // Parse special offer discount IDs
                    var specialDiscountIds = ParseAttributeDiscountIds(orderItem.AttributesXml);

                    var product = products.Where(p => p.Id == orderItem.ProductId).FirstOrDefault();

                    // Step 5: get standard discounts
                    var (_, _, _, standardAppliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(
                        product, customer, store, decimal.Zero, true, 1);

                    decimal finalDiscountedPrice = product.Price;

                    // Step 6: Insert special discounts into DiscountUsage table
                    foreach (var discountId in specialDiscountIds)
                    {
                        var specialDiscount = await _discountService.GetDiscountByIdAsync(discountId);
                        if (specialDiscount != null)
                        {
                            var appliedDiscounts = new List<Discount> { specialDiscount };
                            
                            // Calculate special discount amount
                            var preferredDiscount = _discountService.GetPreferredDiscount(appliedDiscounts, finalDiscountedPrice, out var discountAmount);
                            finalDiscountedPrice -= discountAmount;

                            var discountUsageHistory = discountUsageHistoryList.Where(d => d.DiscountId == specialDiscount.Id).FirstOrDefault();

                            // Add entry to DiscountUsage table for the special discount
                            var discountUsageToAdd = new DiscountUsage
                            {
                                DiscountUsageHistoryId = discountUsageHistory.Id,
                                OrderId = order.Id,
                                OrderItemId = orderItem.Id,
                                DiscountAmount = discountAmount * orderItem.Quantity,
                                DiscountCustomerMappingId = null,
                            };

                            // Insert special discount into DiscountUsage table
                            await _discountCustomerMappingService.InsertDiscountUsageAsync(discountUsageToAdd);
                        }
                    }

                    // Step 7: Insert standard discounts into DiscountUsage table
                    foreach (var discount in standardAppliedDiscounts)
                    {
                        var appliedDiscounts = new List<Discount> { discount };

                        // Calculate standard discount amount based on the remaining price
                        var preferredDiscount = _discountService.GetPreferredDiscount(appliedDiscounts, finalDiscountedPrice, out var discountAmount);
                        finalDiscountedPrice -= discountAmount;

                        var discountUsageHistory = discountUsageHistoryList.Where(d => d.DiscountId == discount.Id).FirstOrDefault();

                        // Add entry to DiscountUsage table for the standard discount
                        var discountUsageToAdd = new DiscountUsage
                        {
                            DiscountUsageHistoryId = discountUsageHistory.Id,
                            OrderId = order.Id,
                            OrderItemId = orderItem.Id,
                            DiscountAmount = discountAmount * orderItem.Quantity,
                            DiscountCustomerMappingId = null,
                        };

                        // Insert standard discount into DiscountUsage table
                        await _discountCustomerMappingService.InsertDiscountUsageAsync(discountUsageToAdd);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                await _logger.ErrorAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            }
        }


        #endregion
    }
}