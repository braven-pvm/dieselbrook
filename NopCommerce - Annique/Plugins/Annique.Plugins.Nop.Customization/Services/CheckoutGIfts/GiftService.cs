using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using DocumentFormat.OpenXml.Office2010.Excel;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.CheckoutGifts
{
    /// <summary>
    /// GiftService class
    /// </summary>
    public class GiftService : IGiftService
    {
        #region Fields 

        private readonly IRepository<Gift> _giftRepository;
        private readonly IRepository<GiftsTaken> _giftTakenRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly IAwardService _awardService;
        private readonly IRepository<Event> _eventRepository;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly TaxSettings _taxSettings;
        private readonly IWorkContext _workContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IEventService _eventService;
        private readonly IExclusiveItemsService _exclusiveItemsService;
        private readonly ISpecialOffersService _specialOffersService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public GiftService(IRepository<Gift> giftRepository,
            IRepository<GiftsTaken> giftTakenRepository,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
             IShoppingCartService shoppingCartService,
            IProductService productService,
            IRepository<Order> orderRepository,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            IAwardService awardService,
            IRepository<Event> eventRepository,
            IOrderTotalCalculationService orderTotalCalculationService,
            TaxSettings taxSettings,
            IWorkContext workContext,
            IPriceFormatter priceFormatter,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IEventService eventService,
            IExclusiveItemsService exclusiveItemsService,
            ISpecialOffersService specialOffersService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IOrderService orderService)
        {
            _giftRepository = giftRepository;
            _giftTakenRepository = giftTakenRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _orderRepository = orderRepository;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _awardService = awardService;
            _eventRepository = eventRepository;
            _orderTotalCalculationService = orderTotalCalculationService;
            _taxSettings = taxSettings;
            _workContext = workContext;
            _priceFormatter = priceFormatter;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _eventService = eventService;
            _exclusiveItemsService = exclusiveItemsService;
            _specialOffersService = specialOffersService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _orderService = orderService;
        }

        #endregion

        #region Gifts Methods

        /// <summary>
        /// Gets all Gifts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gifts
        /// </returns>
        public async Task<IList<Gift>> GetAllGiftsAsync()
        {
            var gifts = await _giftRepository.GetAllAsync(query => query, _ => default);
            return gifts.Any() ? gifts : new List<Gift>();
        }

        /// <summary>
        /// Gets all Force Gifttype gifts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Force giftType gifts
        /// </returns>
        public async Task<IList<Gift>> GetAllForceGiftsAsync()
        {
            var query = _giftRepository.Table;

            query = query.Where(g => g.cGiftType.Trim() == AnniqueCustomizationDefaults.GiftTypeForce);

            query = query.Where(g => g.StartDateUtc <= DateTime.UtcNow && g.EndDateUtc >= DateTime.UtcNow);

            return await query.OrderBy(t => t.Id).ToListAsync();
        }

        /// <summary>
        /// Gets all Blank giftType Gifts
        /// </summary>
        /// <param name="orderTotal">Order Total</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Blank gift type gifts
        /// </returns>
        public async Task<IList<Gift>> GetAllBlankGiftsAsync(decimal orderTotal)
        {
            var query = _giftRepository.Table;
            query = query.Where(g => string.IsNullOrWhiteSpace(g.cGiftType) || g.cGiftType == "DONATION");
            query = query.Where(g => g.StartDateUtc <= DateTime.UtcNow && g.EndDateUtc >= DateTime.UtcNow);
            return await query.Where(g => g.nMinSales <= orderTotal).OrderBy(g => g.Id).ToListAsync();
            //return await query.Where(g => g.nMinSales <= orderTotal && g.nMinSales == query.Where(g2 => g2.nMinSales <= orderTotal).Max(g2 => g2.nMinSales)).OrderBy(g => g.Id).ToListAsync();
        }

        /// <summary>
        /// Gets a gifts by product Id
        /// </summary>
        /// <param name="productId">ProductId Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gifts 
        /// </returns>
        public async Task<List<Gift>> GetGiftsByProductIdsAsync(int[] productIds)
        {
            #region Bug 614 Gift Items not in Gifts Taken table
            //added time and date filter for acturate gifts
            // and retrieve gifts based on the provided product ids
            var gifts = await _giftRepository.Table
                .Where(g => productIds.Contains(g.ProductId) && g.StartDateUtc <= DateTime.UtcNow && g.EndDateUtc >= DateTime.UtcNow)
                .ToListAsync();
            #endregion
            return gifts;
        }

        /// <summary>
        /// Gets a gift by product Id
        /// </summary>
        /// <param name="productId">ProductId Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        public async Task<Gift> GetGiftByProductIdAsync(int productId)
        {
            var query = from g in _giftRepository.Table
                        where g.ProductId == productId
                        && g.StartDateUtc <= DateTime.UtcNow
                        && g.EndDateUtc >= DateTime.UtcNow
                        select g;

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a Starter Gift
        /// </summary>
        /// <param name="orderTotal">Order Total</param>
        /// <returns>
        /// The task result contains the gift
        /// </returns>
        public async Task<Gift> GetStarterGiftByOrderTotalAsync(decimal orderTotal)
        {
            var query = from g in _giftRepository.Table
                        where g.cGiftType.Trim() == AnniqueCustomizationDefaults.GiftTypeStarter
                        && g.StartDateUtc <= DateTime.UtcNow
                        && g.EndDateUtc >= DateTime.UtcNow
                        && g.nMinSales < orderTotal
                        orderby g.nMinSales descending
                        select g;

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns gift id and shopping cart item id
        /// </summary>
        /// <param name="customerId">customer Identifier</param>
        /// <param name="storeId">Store Identifier</param>
        /// The task result returns gift id and shopping cart item id
        public async Task<List<(int giftId, int sciId)>> GetExistGiftItemInCartAsync(int customerId, string giftType, IList<ShoppingCartItem> cart)
        {
            var giftItems = new List<(int giftId, int sciId)>();

            var query = from gift in _giftRepository.Table
                        join cartItem in cart
                        on gift.ProductId equals cartItem.ProductId
                        where cartItem.CustomerId == customerId
                            && gift.cGiftType.Trim() == giftType
                            && gift.StartDateUtc <= DateTime.UtcNow
                            && gift.EndDateUtc >= DateTime.UtcNow
                        select new
                        {
                            GiftId = gift.Id,
                            ShoppingCartItemId = cartItem.Id,
                            ProductId = cartItem.ProductId,
                            CustomerId = cartItem.CustomerId
                        };

            if (query.Any())
            {
                var result = await query.ToListAsync();
                foreach (var item in result)
                {
                    var isForceExclusiveProduct = await _exclusiveItemsService.IsForceExclusiveProductAsync(item.ProductId, item.CustomerId);
                    if (!isForceExclusiveProduct)
                    {
                        giftItems.Add((item.GiftId, item.ShoppingCartItemId));
                    }
                }
            }

            if (!giftItems.Any())
            {
                giftItems.Add((0, 0));
            }

            return giftItems;
        }

        /// <summary>
        /// Returns Wheather customer is eligible to get Starter gifts or not
        /// <param name="customerId">Customer Identifier</param>
        /// </summary>
        /// The task result returns true if customer is new or first sale duration is on for customer
        public async Task<bool> IsEligibleForStarterGiftAsync(int customerId)
        {
            //Get first order
            var firstOrder = _orderRepository.Table.Any(o => o.CustomerId == customerId && o.PaymentStatusId == (int)PaymentStatus.Paid && (o.OrderStatusId == (int)OrderStatus.Processing || o.OrderStatusId == (int)OrderStatus.Complete));

            //Get User profile additional info
            var userProfileInfo = await _userProfileAdditionalInfoService.GetUserProfileAdditionalInfoByCustomerIdAsync(customerId);

            //if customer has not placed any order and activation date is also not set means customer is new and can get gift
            if (!firstOrder && !userProfileInfo.ActivationDate.HasValue)
                return true;
            else if (firstOrder && !userProfileInfo.ActivationDate.HasValue)
                return false;
            else
            {
                //Current date
                DateTime now = DateTime.Now;

                //get Start month for duration from activation date
                var startDate = userProfileInfo.ActivationDate.Value;

                //Calculate end date from 3 month from activation date
                DateTime endDate = new(startDate.AddMonths(3).Year, startDate.AddMonths(3).Month, DateTime.DaysInMonth(startDate.AddMonths(3).Year, startDate.AddMonths(3).Month));

                //Check current date falls between 3 months from customer profile's activation date
                if (now > startDate && now < endDate)
                    return true;
            }
    
            return false;
        }

        /// <summary>
        /// Add gift product to cart
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="productId">ProductId</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store Identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task AddGiftProductInShoppingCartAsync(IList<ShoppingCartItem> cart, int productId, int quantity, Customer customer, int storeId)
        {
            //Get product by product id
            var product = await _productService.GetProductByIdAsync(productId);

            //first, try to find product in existing shopping cart 
            var itemInCart = cart.Where(ci => ci.ProductId == product.Id).FirstOrDefault();
            
            //If item already not exist in cart
            if (itemInCart == null)
            {
                //product.Published = true;

                //Add exclusive item to cart with available limit quantity
                await _shoppingCartService.AddToCartAsync(customer, product, ShoppingCartType.ShoppingCart, storeId, null, 0, null, null, quantity, true);
            }
        }

        #endregion

        #region Gift Taken Methods

        /// <summary>
        /// Inserts a gift taken
        /// </summary>
        /// <param name="giftsTaken">Gifts Taken</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertGiftsTakenAsync(GiftsTaken giftsTaken)
        {
            await _giftTakenRepository.InsertAsync(giftsTaken);
        }

        /// <summary>
        /// Get a total of qty of gift taken
        /// </summary>
        /// <param name="giftId">Gifts Id</param>
        /// <param name="customerId">Customer Id</param>
        public async Task<int> GetGiftTakenQtyTotalAsync(int giftId, int customerId)
        {
            int total = 0;
            total = await _giftTakenRepository.Table.Where(gt => gt.GiftId == giftId && gt.CustomerId == customerId).SumAsync(g => g.Qty);
            return total;
        }

        /// <summary>
        /// Returns Wheather gift taken record already exit for order item
        /// </summary>
        /// <param name="orderItemId">Order Item Identifier</param>
        /// The task result returns true if record with orderItem already exist else false
        public async Task<bool> IsGiftTakenAlreadyExistAsync(int orderItemId)
        {
            #region Bug 614 Gift Items not in Gifts Taken table
            // Asynchronous query to improve scalability and responsiveness
            return await _giftTakenRepository.Table
                .AnyAsync(gt => gt.OrderItemId == orderItemId);
            #endregion
        }

        #endregion

        #region Cart total & Cart page related Method

        public async Task<List<int>> GetNonEditableCartProductsAsync(Customer customer, Store store)
        {
            var nonEditableCartItemIds = new List<int>();

            //Customer current cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            if (!cart.Any())
                return nonEditableCartItemIds;

            var isConsultantRole = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();

            //check customer contains Consultant role 
            if (isConsultantRole)
            {
                //get gift item id from cart for current user
                var giftProductIds = await GetGiftCartItemIds(cart);
                if (giftProductIds.Any())
                    nonEditableCartItemIds.AddRange(giftProductIds);

                //get event item id from cart for current user
                var eventProductIds = await GetEventCartItemIds(cart);
                if (eventProductIds.Any())
                    nonEditableCartItemIds.AddRange(eventProductIds);

                //get award item id from cart for current user
                var awardProductIds = GetAwardCartItemIds(cart);
                if (awardProductIds.Any())
                    nonEditableCartItemIds.AddRange(awardProductIds);
            }

            var specialProductIds = GetSpecialCartItemIds(cart);
            if (specialProductIds.Any())
                nonEditableCartItemIds.AddRange(specialProductIds);

            if (!nonEditableCartItemIds.Any())
                nonEditableCartItemIds.Add(0);

            return nonEditableCartItemIds;
        }

        //get gift cart item id from cart for current user
        private async Task<IEnumerable<int>> GetGiftCartItemIds(IList<ShoppingCartItem> cart)
        {
            // Get all gifts asynchronously
            var allGifts = await GetAllGiftsAsync();

            if (!allGifts.Any())
                return Enumerable.Empty<int>();

            // Extract all unique ProductIds from the cart
            var productIds = cart.Select(sci => sci.ProductId).Distinct().ToList();

            // Filter the gifts based on ProductIds from the cart
            var relevantGifts = allGifts.Where(g => productIds.Contains(g.ProductId)).ToList();

            var currentTimeUtc = DateTime.UtcNow;

            // Now perform the join with the filtered gifts (relevantGifts)
            return cart
                .Join(relevantGifts, // Join with the pre-filtered relevant gifts
                    sci => sci.ProductId,
                    g => g.ProductId,
                    (sci, g) => new { sci, g })
                .Where(joined => joined.g.StartDateUtc <= currentTimeUtc
                                 && joined.g.EndDateUtc >= currentTimeUtc)
                .Select(joined => joined.sci.Id);
        }

        //get event cart item id from cart for current user
        private async Task<IEnumerable<int>> GetEventCartItemIds(IList<ShoppingCartItem> cart)
        {
            var events = await _eventService.GetAllEventsAsync(true, true);
            if(events == null || !events.Any())
                return Enumerable.Empty<int>();

            // Extract unique ProductIds from the cart using a HashSet for efficient lookup
            var productIds = cart.Select(sci => sci.ProductId).ToHashSet();

            // Filter events that match ProductIds in the cart (no need to convert to a list)
            var relevantEvents = events.Where(e => productIds.Contains(e.ProductID));

            // Return the cart item IDs for the matching events
            return cart
                .Where(sci => relevantEvents.Any(e => e.ProductID == sci.ProductId)) // Only include cart items with matching events
                .Select(sci => sci.Id);  // Return the ShoppingCartItem IDs
        }

        //get award cart item id from cart for current user
        private IList<int> GetAwardCartItemIds(IList<ShoppingCartItem> cart)
        {
            IList<int> cartItemIds = new List<int>();

            //get cart items where Attribute xml is not null empty
            var cartItems = cart.Where(item => !string.IsNullOrEmpty(item.AttributesXml))
                .Select(item => item);

            foreach (var cartItem in cartItems)
            {
                //check from attribute xml cart product is award item or not
                if (_awardService.ContainsAwardProductAttribute(cartItem.AttributesXml))
                {
                    cartItemIds.Add(cartItem.Id);
                }
            }
            return cartItemIds;
        }

        //get award cart item id from cart for current user
        private IList<int> GetSpecialCartItemIds(IList<ShoppingCartItem> cart)
        {
            IList<int> cartItemIds = new List<int>();

            //get cart items where Attribute xml is not null empty
            var cartItems = cart.Where(item => !string.IsNullOrEmpty(item.AttributesXml))
                .Select(item => item);

            foreach (var cartItem in cartItems)
            {
                //check from attribute xml cart product is award item or not
                if (_specialOffersService.ContainsSpecialOfferAttribute(cartItem.AttributesXml))
                {
                    cartItemIds.Add(cartItem.Id);
                }
            }
            return cartItemIds;
        }

        /// <summary>
        /// Returns shopping cart total discounts
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartTotalBeforeDiscount">Shopping cart total before discount</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetShoppingCartTotalsDiscountAsync(IList<ShoppingCartItem> cart, decimal cartTotalBeforeDiscount)
        {
            var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;

            var (_, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);

            var discount = cartTotalBeforeDiscount - subTotalWithoutDiscountBase;
            var discountValue = await _priceFormatter.FormatPriceAsync(discount, false, false);

            return discountValue;
        }

        /// <summary>
        /// Returns Wheather gifts should be procesed or not
        /// <param name="cart">Shopping cart</param>
        /// </summary>
        /// The task result returns true if cart has other products besides events 
        public async Task<bool> CanProcessGiftsAsync(IList<ShoppingCartItem> cart,Customer customer)
        {
            // Get product ids of items in the cart
            var productIdsInCart = cart.Select(item => item.ProductId).ToArray();

            //event tickets product id
            var eventProductIds = (await _eventService.GetAllEventsAsync()).Select(e => e.ProductID);

            // Get gifts for the product ids in the cart
            var gifts = await GetGiftsByProductIdsAsync(productIdsInCart);

            // Filter out cart items with gifts
            var cartWithoutGifts = FilterItemsWithoutGifts(cart, gifts);

            //check cart has any other items besides event products, if yes then gifts can be processed so return true
            if (cartWithoutGifts.Any(item => !eventProductIds.Contains(item.ProductId)))
                return true;

            //if cart has no other products besides event tickets then remove gifts from cart
            //filter out cart item with gifts
            var cartItemsWithGifts = FilterItemsWithGifts(cart, gifts);

            //if any gift items 
            if (cartItemsWithGifts.Any())
            {
                foreach (var item in cartItemsWithGifts)
                {
                    //Remove gift product from cart
                    await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                        item.Id, string.Empty, decimal.Zero,
                        null, null, 0, true);
                }
            }

            return false;
        }

        public IList<ShoppingCartItem> FilterItemsWithoutGifts(IList<ShoppingCartItem> cart, List<Gift> gifts)
        {
            return cart.Where(item =>
            {
                var gift = gifts.FirstOrDefault(g => g.ProductId == item.ProductId);
                return gift == null;
            }).ToList();
        }

        public IList<ShoppingCartItem> FilterItemsWithGifts(IList<ShoppingCartItem> cart, List<Gift> gifts)
        {
            return cart.Where(item =>
            {
                var gift = gifts.FirstOrDefault(g => g.ProductId == item.ProductId);
                return gift != null;
            }).ToList();
        }

        /// <summary>
        /// Process gifts on checkout 
        /// <param name="cart">Shopping cart</param>
        /// <param name="customer"/>
        /// </summary>
        /// The task process gifts on checkout
        public async Task ProcessGiftsAsync(IList<ShoppingCartItem> cart, Customer customer)
        {
            var canProcessGifts = await CanProcessGiftsAsync(cart, customer);
            if (!canProcessGifts) return;

            var productIdsInCart = cart.Select(item => item.ProductId).ToArray();
            var gifts = await GetGiftsByProductIdsAsync(productIdsInCart);

            var cartWithoutGifts = (from cartItem in cart
                                    join gift in gifts on cartItem.ProductId equals gift.ProductId into matchingGifts
                                    from matchedGift in matchingGifts.DefaultIfEmpty()
                                    where matchedGift == null ||
                                          string.IsNullOrWhiteSpace(matchedGift.cGiftType) ||
                                          matchedGift.cGiftType == AnniqueCustomizationDefaults.GiftTypeDonation
                                    select cartItem).ToList();

            var (subTotalWithoutDiscount, _) = await _anniqueCustomizationConfigurationService.GetShoppingCartTotalsBeforeDiscountAsync(cartWithoutGifts);

            await HandleForceGiftsAsync(gifts, cart, subTotalWithoutDiscount, customer);
            await HandleStarterGiftsAsync(cart, cartWithoutGifts, customer, subTotalWithoutDiscount);
        }

        private async Task HandleForceGiftsAsync(IList<Gift> gifts, IList<ShoppingCartItem> cart, decimal subTotalWithoutDiscount, Customer customer)
        {
            var forceGiftItems = gifts.Where(g => g.cGiftType == AnniqueCustomizationDefaults.GiftTypeForce).ToList();
            foreach (var gift in forceGiftItems)
            {
                if (subTotalWithoutDiscount == 0 || subTotalWithoutDiscount < gift.nMinSales)
                {
                    var forceCartItem = cart.FirstOrDefault(ci => ci.ProductId == gift.ProductId);
                    if (forceCartItem != null)
                    {
                        await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                            forceCartItem.Id, string.Empty, decimal.Zero,
                            null, null, 0, true);
                    }
                }
            }
        }

        private async Task HandleStarterGiftsAsync(IList<ShoppingCartItem> cart, IList<ShoppingCartItem> cartWithoutGifts, Customer customer, decimal subTotalWithoutDiscount)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var isEligibleStarterGift = await IsEligibleForStarterGiftAsync(customer.Id);
            var isStarterKitInCart = _exclusiveItemsService.IsStarterKitExistInCart(cartWithoutGifts);
          
            //Get gift id and shopping cart item Id for already exist starter gifttype product in cart
            var starterGiftItems = await GetExistGiftItemInCartAsync(customer.Id, AnniqueCustomizationDefaults.GiftTypeStarter, cart);

            //if customer is not eligible for gift but in customer's cart has old Starter gift then remove that gift from cart
            if (!isEligibleStarterGift && starterGiftItems[0].sciId != 0)
            {
                //Remove old starer gift product from cart
                await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                    starterGiftItems[0].sciId, string.Empty, decimal.Zero,
                    null, null, 0, true);
            }

            if (isEligibleStarterGift && !isStarterKitInCart)
            {
                //get new starter gift product 
                var gift = await GetStarterGiftByOrderTotalAsync(subTotalWithoutDiscount);

                if (gift != null)
                {
                    if (starterGiftItems[0].giftId != 0 && starterGiftItems[0].giftId != gift.Id)
                    {
                        //Remove old starer gift product from cart
                        await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                            starterGiftItems[0].sciId, string.Empty, decimal.Zero,
                            null, null, 0, true);
                    }

                    var takenGiftsQtyTotal = await GetGiftTakenQtyTotalAsync(gift.Id, customer.Id);
                    var availableQty = gift.nQtyLimit;

                    if (takenGiftsQtyTotal != 0)
                        availableQty = gift.nQtyLimit - takenGiftsQtyTotal;

                    if (takenGiftsQtyTotal == 0 || availableQty > 0 && starterGiftItems[0].giftId != gift.Id)
                    {
                        //Add new gift product to cart
                        await AddGiftProductInShoppingCartAsync(cart, gift.ProductId, 1, customer, store.Id);
                    }
                }
                else
                {
                    if (starterGiftItems[0].giftId != 0)
                    {
                        //Remove old starer gift product from cart
                        await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                            starterGiftItems[0].sciId, string.Empty, decimal.Zero,
                            null, null, 0, true);
                    }
                }
            }
        }

        /// <summary>
        /// Process gifts on order paid  
        /// <param name="customerId"/>Customer Id</param>
        /// <param name="orderItems">Order Items</param>
        /// </summary>
        /// The task process gifts taken on order paid
        public async Task ProcessGiftsTakenAsync(int customerId, IList<OrderItem> orderItems)
        {
            //Get order id
            var orderId = orderItems.Select(oi => oi.OrderId).FirstOrDefault();

            // Get distinct ProductIds from orderItems
            var productIds = orderItems.Select(oi => oi.ProductId).Distinct();

            #region Checkout Gift

            //get gifts by productIds
            var gifts = await GetGiftsByProductIdsAsync(productIds.ToArray());
            if (gifts.Any())
            {
                //associated order items with gift
                var orderItemsWithGifts = from orderItem in orderItems
                                          join gift in gifts on orderItem.ProductId equals gift.ProductId
                                          select new { OrderItem = orderItem, Gift = gift };

                #region Bug 614 Gift Items not in Gifts Taken table

                List<string> orderNotes = new List<string>(); // List to hold multiple order notes for all gifts

                #endregion

                foreach (var pair in orderItemsWithGifts)
                {
                    // Check for order item already exists in gift taken table
                    var alreadyEntryInGiftTaken = await IsGiftTakenAlreadyExistAsync(pair.OrderItem.Id);

                    #region Bug 614 Gift Items not in Gifts Taken table

                    // Prepare base order note with order item and gift info
                    var orderNote = $"OrderItemId: {pair.OrderItem.Id} , Gift Id :{pair.Gift.Id} , Gift Type : {pair.Gift.cGiftType}";

                    #endregion

                    if (!alreadyEntryInGiftTaken)
                    {
                        var giftsTaken = new GiftsTaken
                        {
                            GiftId = pair.Gift.Id,
                            CustomerId = customerId,
                            OrderItemId = pair.OrderItem.Id,
                            Qty = pair.OrderItem.Quantity
                        };
                        await InsertGiftsTakenAsync(giftsTaken);

                        #region  #region Bug 614 Gift Items not in Gifts Taken table

                        //tracing issue with order notes for All gift type
                        orderNote += $" GiftTakenId: {giftsTaken.Id}";
                        orderNotes.Add(orderNote);

                        #endregion

                        if (pair.Gift.cGiftType == AnniqueCustomizationDefaults.GiftTypeDonation)
                            //update order item attribute description to 'Donation'
                            pair.OrderItem.AttributeDescription += await _localizationService.GetResourceAsync("CheckoutGifts.OrderItem.Donation.AttributeDescription");
                        else
                            //update order item attribute description to 'Gift'
                            pair.OrderItem.AttributeDescription += await _localizationService.GetResourceAsync("CheckoutGifts.OrderItem.AttributeDescription");

                        await _orderService.UpdateOrderItemAsync(pair.OrderItem);
                    }
                }

                #region Bug 614 Gift Items not in Gifts Taken table

                // If there are multiple Force gifts, concatenate them into one order note and insert it
                if (orderNotes.Any())
                {
                    // Concatenate all force gift order notes
                    var allGiftNotes = string.Join(Environment.NewLine, orderNotes);
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = orderId,
                        Note = allGiftNotes,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
                #endregion
               
            }

            #endregion
        }

        #endregion
    }
}
