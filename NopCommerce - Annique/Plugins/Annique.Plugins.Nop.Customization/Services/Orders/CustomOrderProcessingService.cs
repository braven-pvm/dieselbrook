using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Services.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.Orders
{
    public class CustomOrderProcessingService : ICustomOrderProcessingService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftService _giftService;
        private readonly IEventService _eventService;
        private readonly IExclusiveItemsService _exclusiveItemsService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ISpecialOffersService _specialOffersService;
        
        #endregion

        #region Ctor

        public CustomOrderProcessingService(ICustomerService customerService,
            IOrderService orderService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            IGiftService giftService,
            IEventService eventService,
            IExclusiveItemsService exclusiveItemsService,
            IStoreContext storeContext,
            ISettingService settingService,
            ISpecialOffersService specialOffersService)
        {
            _customerService = customerService;
            _orderService = orderService;
            _productService = productService;
            _genericAttributeService = genericAttributeService;
            _shoppingCartService = shoppingCartService;
            _giftService = giftService;
            _eventService = eventService;
            _exclusiveItemsService = exclusiveItemsService;
            _storeContext = storeContext;
            _settingService = settingService;
            _specialOffersService = specialOffersService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Place order items in current user shopping cart.
        /// </summary>
        /// <param name="order">The order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ReOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeContext.GetCurrentStoreAsync();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
           
            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
           
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            #region #588 Special Reorder

            // Filter order items based on (attribute is not null/empty and does not contain special offer attribute)
            var specialOrderItemIds = orderItems.Where(oi =>
               !string.IsNullOrEmpty(oi.AttributesXml) &&
               _specialOffersService.ContainsSpecialOfferAttribute(oi.AttributesXml)
            ).Select(oi => oi.Id).ToArray();

            if (specialOrderItemIds.Any())
                orderItems = orderItems.Where(oi => !specialOrderItemIds.Contains(oi.Id)).ToList();

            #endregion

            // Convert orderItems to a List so that we can use RemoveAll
            var orderItemsList = orderItems.ToList();

            if (customerRoleIds.Contains(settings.ConsultantRoleId))
            {
                // Get distinct ProductIds from orderItems
                var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToArray();

                //gifts by product id
                var giftsTask = await _giftService.GetGiftsByProductIdsAsync(productIds);
                var gifts = giftsTask ?? Enumerable.Empty<Gift>(); // Null coalescing to an empty enumerable if null

                //events by product id
                var anniqueEventTask = await _eventService.GetEventsByProductIdsAsync(productIds);
                var anniqueEvents = anniqueEventTask ?? Enumerable.Empty<Event>(); // Null coalescing to an empty enumerable if null

                //exclusive products 
                var exclusiveProductTask = await _exclusiveItemsService.GetExclusiveItemsByProductIdsAsync(productIds);
                var exclusiveProducts = exclusiveProductTask ?? Enumerable.Empty<ExclusiveItems>(); // Null coalescing to an empty enumerable if null

                // Get distinct ProductIds from gifts, anniqueEvent, and exclusiveProducts for excluding for reorder
                var excludeProductIds = gifts.Select(g => g.ProductId)
                    .Concat(anniqueEvents.Select(e => e.ProductID))
                    .Concat(exclusiveProducts.Select(ep => (int)ep.ProductID))
                    .Distinct();

                if (excludeProductIds.Any())
                {
                    // Remove order items where ProductId matches any of the excludeProductIds
                    orderItemsList.RemoveAll(oi => excludeProductIds.Contains(oi.ProductId));
                }
            }

            //move shopping cart items (if possible)
            foreach (var item in orderItemsList)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);

                await _shoppingCartService.AddToCartAsync(customer, product,
                    ShoppingCartType.ShoppingCart, order.StoreId,
                    string.Empty, item.UnitPriceExclTax,
                    item.RentalStartDateUtc, item.RentalEndDateUtc,
                    item.Quantity, false);
            }

            //set checkout attributes
            //comment the code below if you want to disable this functionality
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CheckoutAttributes, order.CheckoutAttributesXml, order.StoreId);
        }

        #endregion
    }
}
