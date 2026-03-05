using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// Custom Order Status Changed Event
    /// </summary>
    public class CustomOrderStatusChangedEvent : IConsumer<OrderStatusChangedEvent>
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IExclusiveItemsService _exclusiveItemsService;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IDiscountCustomerMappingService _discountCustomerMappingService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public CustomOrderStatusChangedEvent(IStoreContext storeContext,
            ISettingService settingService,
            IExclusiveItemsService exclusiveItemsService,
            ILogger logger,
            IWorkContext workContext,
            IDiscountCustomerMappingService discountCustomerMappingService,
            IOrderService orderService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _exclusiveItemsService = exclusiveItemsService;
            _logger = logger;
            _workContext = workContext;
            _discountCustomerMappingService = discountCustomerMappingService;
            _orderService = orderService;
        }

        #endregion

        #region method

        /// <summary>
        /// Represents an event that occurs after Order status is changed
        /// </summary>
        /// <typeparam name="orderStatusChangeEvent">orderStatusChangeEvent</typeparam>
        public async Task HandleEventAsync(OrderStatusChangedEvent orderStatusChangeEvent)
        {
            try
            {
                //get active store
                var storeScope = await _storeContext.GetCurrentStoreAsync();

                //get Active store Annique Settings
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

                //If plugin is enable
                if (settings.IsEnablePlugin)
                {
                    var order = orderStatusChangeEvent.Order;

                    if (order.OrderStatus != OrderStatus.Cancelled)
                        return;

                    var orderNotes = await _orderService.GetOrderNotesByOrderIdAsync(order.Id);

                    //process restoring exclusive items on order cancellation
                    await _exclusiveItemsService.RestoreExclusiveItemsOnOrderCancellationAsync(order, orderNotes);

                    //process restoring special discounts on order cancellation
                    await _discountCustomerMappingService.RestoreSpecialDiscountOnOrderCancellationAsync(order, orderNotes);
                }
            }
            catch (Exception ex)
            {
                // Log the exception 
                await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            }
        }

        #endregion
    }
}
