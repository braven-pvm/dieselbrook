using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Annique.Plugins.Nop.Customization.Services.NewActivityLogs;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Orders;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// Custom Order Placed Event
    /// </summary>
    public class CustomOrderPlacedEvent : IConsumer<OrderPlacedEvent>
    {
        #region Fields

        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IEventService _eventService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IDiscountCustomerMappingService _discountCustomerMappingService;
        private readonly ISpecialOffersService _specialOffersService;
        private readonly IAdditionalActivityLogService _additionalActivityLogService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CustomOrderPlacedEvent(IOrderProcessingService orderProcessingService,
            IStoreContext storeContext,
            ISettingService settingService,
            IOrderService orderService,
            IEventService eventService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IDiscountCustomerMappingService discountCustomerMappingService,
            ISpecialOffersService specialOffersService,
            IAdditionalActivityLogService additionalActivityLogService,
            IWorkContext workContext)
        {
            _orderProcessingService = orderProcessingService;
            _storeContext = storeContext;
            _settingService = settingService;
            _orderService = orderService;
            _eventService = eventService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _discountCustomerMappingService = discountCustomerMappingService;
            _specialOffersService = specialOffersService;
            _additionalActivityLogService = additionalActivityLogService;
            _workContext = workContext;
        }

        #endregion

        #region Method

        /// <summary>
        /// Represents an event that occurs after Order placed
        /// </summary>
        /// <typeparam name="orderPlacedEvent">orderPlacedEvent</typeparam>
        public async Task HandleEventAsync(OrderPlacedEvent orderPlacedEvent)
        {
            //get active store
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope);

            var isConsultant = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();

            //If plugin is enable
            if (settings.IsEnablePlugin)
            {
                var order = orderPlacedEvent.Order;

                //Get order items
                var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

                // Get distinct ProductIds from orderItems
                var productIds = orderItems.Select(oi => oi.ProductId).Distinct();

                if (isConsultant)
                {
                    #region Annique Events

                    await _eventService.ProcessEventsOnOrderPlacedAsync(order, orderItems);

                    #endregion
                }

                #region Mapped/Unmapped discount usage entry

                //handle discount usage entries 
                await _discountCustomerMappingService.HandleOrderDiscountsAsync(order, orderItems);

                #endregion

                #region special offer discount usage entry

                //handle discount usage entries 
                await _specialOffersService.SaveSpecialOfferDiscountUsageHistoryAsync(order, orderItems);

                #endregion

                #region Exclusive items (code enhancement with fixing of bug #584)

                // Do not inject IExclusiveItemsService via constructor because it'll cause circular references
                var _exclusiveItemsService = EngineContext.Current.Resolve<IExclusiveItemsService>();
                await _exclusiveItemsService.HandleExclusiveItemsOnOrderPlaceAsync(order,orderItems);

                #endregion

                #region Annique Staff Customer

                //Check if payment method is COD
                if (order.PaymentMethodSystemName == "Payments.CashOnDelivery")
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
                }

                #endregion

                #region task 590 Customer Role - Update when registered role orders

                await _anniqueCustomizationConfigurationService.SetCustomerRoleToRegisteredUserAsync(order.CustomerId);

                #endregion

                #region Task 644 new Activity logs

                await _additionalActivityLogService.InsertActivityTrackingLogAsync("PublicStore.OrderPlaceEvent", "Order Place", await _workContext.GetCurrentCustomerAsync());

                #endregion
            }
        }

        #endregion
    }
}
