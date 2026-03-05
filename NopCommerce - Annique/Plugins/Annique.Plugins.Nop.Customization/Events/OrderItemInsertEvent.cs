using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    public class OrderItemInsertEvent : IConsumer<EntityInsertedEvent<OrderItem>>
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly ISpecialOffersService _specialOffersService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public OrderItemInsertEvent(
            IStoreContext storeContext,
            ISettingService settingService,
            IOrderService orderService,
            ISpecialOffersService specialOffersService,
            ILocalizationService localizationService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _orderService = orderService;
            _specialOffersService = specialOffersService;
            _localizationService = localizationService;
        }

		#endregion

		#region Method

		/// <summary>
		/// Represents an event that occurs after Order placed
		/// </summary>
		/// <typeparam name="eventMessage">eventMessage</typeparam>
		public async Task HandleEventAsync(EntityInsertedEvent<OrderItem> eventMessage)
        {
            //get active store
            var storeScope = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

            //If plugin is enable
            if (settings.IsEnablePlugin)
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(eventMessage.Entity.Id);

                #region #579 special Offers

                if (!string.IsNullOrEmpty(orderItem.AttributesXml))
                {
                    //if order item attribute means product is from award basket so make prices 0
                    if (_specialOffersService.ContainsSpecialOfferAttribute(orderItem.AttributesXml))
                    {
						//update order item attribute description to 'Special Offer Item'
						orderItem.AttributeDescription += await _localizationService.GetResourceAsync("SpecialOffer.OrderItem.AttributeDescription");

                        await _orderService.UpdateOrderItemAsync(orderItem);
                    }
                }
               
                #endregion
            }
        }

        #endregion
    }
}
