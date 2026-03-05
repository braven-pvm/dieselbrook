using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Events;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// Customer Loggedout event
    /// </summary>
    public class StoreCustomerLoggedOutEvent : IConsumer<CustomerLoggedOutEvent>
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IGenericAttributeService _genericAttributeService;
        #endregion

        #region Ctor

        public StoreCustomerLoggedOutEvent(IStoreContext storeContext,
            ISettingService settingService, 
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Represents an event that occurs after customer logged out
        /// </summary>
        /// <typeparam name="eventMessage">eventMessage</typeparam>
        public async Task HandleEventAsync(CustomerLoggedOutEvent eventMessage)
        {
            //get active store
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //If plugin is not enable return
            if (!settings.IsEnablePlugin)
                return;

            // Get the customer from the event message
            var customer = eventMessage.Customer;

            // Check if the attribute exists for the customer
            var notifiedAboutPrivateMessages = await _genericAttributeService.GetAttributeAsync<bool>(
                customer, NopCustomerDefaults.NotifiedAboutNewPrivateMessagesAttribute, store.Id);

            if (notifiedAboutPrivateMessages)
            {
                // Remove the attribute
                await _genericAttributeService.SaveAttributeAsync(
                    customer, NopCustomerDefaults.NotifiedAboutNewPrivateMessagesAttribute, false, store.Id);
            }
            
        }

        #endregion
    }
}
