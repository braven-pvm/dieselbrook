using Nop.Core;
using Nop.Services.Affiliates;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Topics;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// Public Topic model Prepare event
    /// </summary>
    public class PublicTopicModelPrepareModel : IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public PublicTopicModelPrepareModel(IWorkContext workContext,
            IAffiliateService affiliateService,
            ICustomerService customerService,
            ILogger logger)
        {
            _workContext = workContext;
            _affiliateService = affiliateService;
            _customerService = customerService;
            _logger = logger;
        }

        #endregion

        #region Method

        /// <summary>
        /// Represents an event that occurs after Topic model prepare
        /// </summary>
        /// <typeparam name="eventMessage">eventMessage</typeparam>
        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            if (eventMessage.Model is TopicModel)
            {
                var model = eventMessage.Model as TopicModel;

                try
                {
                    //Get current customer
                    var customer = await _workContext.GetCurrentCustomerAsync();

                    //If affiliate Id is greater than 0 means customer has arrived from Affiliate
                    if (customer.AffiliateId > 0)
                    {
                        //Get affiliate 
                        var affiliate = await _affiliateService.GetAffiliateByIdAsync(customer.AffiliateId);

                        //Prepare affiliate string if friendly url not set then use Affiliate Id as affiliate query string value
                        var affiliateQueryStringValue = !string.IsNullOrEmpty(affiliate.FriendlyUrlName) ? affiliate.FriendlyUrlName : affiliate.Id.ToString();

                        //check current customer is loggedin or not
                        var isCurrentUserLoggedIn = await _customerService.IsRegisteredAsync(customer);
                        if (isCurrentUserLoggedIn)
                        {
                            //prepare query string for username
                            var usernameQueryString = $"&username={customer.Username}";

                            //append username query string to affiliate's query string
                            affiliateQueryStringValue += usernameQueryString;
                        }
                        if (!string.IsNullOrEmpty(model.Body))
                        {
                            if (model.Body.Contains("##link1##"))
                            {
                                model.Body = model.Body.Replace("##link1##", affiliateQueryStringValue);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                }
            }
        }

        #endregion
    }
}
