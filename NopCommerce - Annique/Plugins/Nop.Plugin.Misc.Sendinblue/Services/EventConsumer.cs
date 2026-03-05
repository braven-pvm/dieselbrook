using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Installation;
using Nop.Services.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Sendinblue.Services
{
    /// <summary>
    /// Represents event consumer
    /// </summary>
    public class EventConsumer :
        IConsumer<EmailUnsubscribedEvent>,
        IConsumer<EmailSubscribedEvent>,
        IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
        IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
        IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
        IConsumer<OrderPaidEvent>,
        IConsumer<OrderPlacedEvent>,
        IConsumer<EntityTokensAddedEvent<Store, Token>>,
        IConsumer<EntityTokensAddedEvent<Customer, Token>>
    {
        #region Fields

        private readonly MarketingAutomationManager _marketingAutomationManager;
        private readonly SendinblueManager _sendinblueEmailManager;
        private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public EventConsumer(MarketingAutomationManager marketingAutomationManager,
            SendinblueManager sendinblueEmailManager,
            ICountryService countryService)
        {
            _marketingAutomationManager = marketingAutomationManager;
            _sendinblueEmailManager = sendinblueEmailManager;
            _countryService = countryService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle the email unsubscribed event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EmailUnsubscribedEvent eventMessage)
        {
            //unsubscribe contact
            await _sendinblueEmailManager.UnsubscribeAsync(eventMessage.Subscription);
        }

        /// <summary>
        /// Handle the email subscribed event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EmailSubscribedEvent eventMessage)
        {
            //subscribe contact
            await _sendinblueEmailManager.SubscribeAsync(eventMessage.Subscription);
        }

        /// <summary>
        /// Handle the add shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {
            //handle event
            await _marketingAutomationManager.HandleShoppingCartChangedEventAsync(eventMessage.Entity);
        }

        /// <summary>
        /// Handle the update shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            //handle event
            await _marketingAutomationManager.HandleShoppingCartChangedEventAsync(eventMessage.Entity);
        }

        /// <summary>
        /// Handle the delete shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            //handle event
            await _marketingAutomationManager.HandleShoppingCartChangedEventAsync(eventMessage.Entity);
        }

        /// <summary>
        /// Handle the order paid event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            //handle event
            await _marketingAutomationManager.HandleOrderCompletedEventAsync(eventMessage.Order);
            await _sendinblueEmailManager.UpdateContactAfterCompletingOrderAsync(eventMessage.Order);
        }

        /// <summary>
        /// Handle the order placed event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            //handle event
            await _marketingAutomationManager.HandleOrderPlacedEventAsync(eventMessage.Order);
        }

        /// <summary>
        /// Handle the store tokens added event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public Task HandleEventAsync(EntityTokensAddedEvent<Store, Token> eventMessage)
        {
            //handle event
            eventMessage.Tokens.Add(new Token("Store.Id", eventMessage.Entity.Id));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle the customer tokens added event
        /// This method processes the customer's phone number, ensuring it is properly formatted with the appropriate 
        /// international country code based on the customer's country ID. If the phone number does not already 
        /// include the correct country code, it is prepended with the country dialing code.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityTokensAddedEvent<Customer, Token> eventMessage)
        {
            var phone = eventMessage.Entity.Phone;
            var countryId = eventMessage.Entity.CountryId;

            #region task 610 Format phone number for sendinblue plugin

            //to format phone number country id is required so setting country id for customer
            //adding static country id 207 for South africa country 
            if (countryId == 0)
                countryId = 207;

            // Only proceed if phone valid
            if (!string.IsNullOrEmpty(phone))
            {
                // Fetch country information
                var country = await _countryService.GetCountryByIdAsync(countryId);

                if (country?.NumericIsoCode != null)
                {
                    var phoneCode = ISO3166.FromISOCode(country.NumericIsoCode)
                        ?.DialCodes?.FirstOrDefault()?.Replace(" ", string.Empty);

                    if (!string.IsNullOrEmpty(phoneCode) && !phone.StartsWith($"+{phoneCode}"))
                    {
                        phone = $"+{phoneCode}{phone}";
                    }
                }
            }
            #endregion

            // Add formatted phone number to tokens
            eventMessage.Tokens.Add(new Token("Customer.PhoneNumber", phone));
        }

        #endregion
    }
}