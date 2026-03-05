using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Affiliates;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace Annique.Plugins.Nop.Customization.Filters
{
    /// <summary>
    /// Represents filter attribute that checks and updates affiliate of customer
    /// </summary>
    public sealed class CheckCustomAffiliateAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public CheckCustomAffiliateAttribute() : base(typeof(CheckAffiliateFilter))
        {
        }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter that checks and updates affiliate of customer
        /// </summary>
        private class CheckAffiliateFilter : IAsyncActionFilter
        {
            #region Constants

            private const string AFFILIATE_ID_QUERY_PARAMETER_NAME = "ref";
            private const string AFFILIATE_FRIENDLYURLNAME_QUERY_PARAMETER_NAME = "ref";

            #endregion

            #region Fields

            private readonly IAffiliateService _affiliateService;
            private readonly ICustomerService _customerService;
            private readonly IWorkContext _workContext;
            private readonly ICustomerActivityService _customerActivityService;
            private readonly IWebHelper _webHelper;
            private readonly ILocalizationService _localizationService;
            private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;

            #endregion

            #region Ctor

            public CheckAffiliateFilter(IAffiliateService affiliateService,
                ICustomerService customerService,
                IWorkContext workContext,
                ICustomerActivityService customerActivityService,
                IWebHelper webHelper,
                ILocalizationService localizationService,
                IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService)
            {
                _affiliateService = affiliateService;
                _customerService = customerService;
                _workContext = workContext;
                _webHelper = webHelper;
                _customerActivityService = customerActivityService;
                _localizationService = localizationService;
                _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Set the affiliate identifier of current customer
            /// </summary>
            /// <param name="affiliate">Affiliate</param>
            /// <param name="customer">Customer</param>
            /// <returns>A task that represents the asynchronous operation</returns>
            private async Task SetCustomerAffiliateIdAsync(Affiliate affiliate, Customer customer, QueryString queryString)
            {
                if (affiliate == null || affiliate.Deleted || !affiliate.Active)
                    return;

                if (affiliate.Id == customer.AffiliateId)
                    return;

                //ignore search engines
                if (customer.IsSearchEngineAccount())
                    return;

                //update affiliate identifier
                customer.AffiliateId = affiliate.Id;

                #region Task 575 save details Affiliate / Referral Link   ? ref=  into admin comment

                // Convert QueryString to string 
                var queryStringAsString = queryString.HasValue ? queryString.Value : string.Empty;

                if (!string.IsNullOrEmpty(queryStringAsString) && string.IsNullOrEmpty(customer.AdminComment))
                {
                    customer.AdminComment = queryStringAsString;
                }

                #endregion

                await _customerService.UpdateCustomerAsync(customer);

                #region Task 575 save details Affiliate / Referral Link   ? ref=  into activity log

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewCustomer",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.GuestCustomer.RequestUrl"), _webHelper.GetThisPageUrl(true), _webHelper.GetCurrentIpAddress()), customer);

                #endregion

                //Task 634 Client Role not allowed to change
            }

            /// <summary>
            /// Called asynchronously before the action, after model binding is complete.
            /// </summary>
            /// <param name="context">A context for action filters</param>
            /// <returns>A task that represents the asynchronous operation</returns>
            private async Task CheckAffiliateAsync(ActionExecutingContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                //check request query parameters
                var request = context.HttpContext.Request;
                if (request?.Query == null || !request.Query.Any())
                    return;

                if (!DataSettingsManager.IsDatabaseInstalled())
                    return;

                //try to find by ID
                var customer = await _workContext.GetCurrentCustomerAsync();
                var affiliateIds = request.Query[AFFILIATE_ID_QUERY_PARAMETER_NAME];

                #region Task 587 do not overwritten affiliate id

                if (customer.AffiliateId > 0)
                    return;

                #endregion

                if (int.TryParse(affiliateIds.FirstOrDefault(), out var affiliateId) && affiliateId > 0 && affiliateId != customer.AffiliateId)
                {
                    var affiliate = await _affiliateService.GetAffiliateByFriendlyUrlNameAsync(affiliateId.ToString());
                    await SetCustomerAffiliateIdAsync(affiliate, customer, request.QueryString);
                    return;
                }

                //try to find by friendly name
                var affiliateNames = request.Query[AFFILIATE_FRIENDLYURLNAME_QUERY_PARAMETER_NAME];
                var affiliateName = affiliateNames.FirstOrDefault();
                if (!string.IsNullOrEmpty(affiliateName))
                {
                    var affiliate = await _affiliateService.GetAffiliateByFriendlyUrlNameAsync(affiliateName);
                    await SetCustomerAffiliateIdAsync(affiliate, customer, request.QueryString);
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called asynchronously before the action, after model binding is complete.
            /// </summary>
            /// <param name="context">A context for action filters</param>
            /// <param name="next">A delegate invoked to execute the next action filter or the action itself</param>
            /// <returns>A task that represents the asynchronous operation</returns>
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                await CheckAffiliateAsync(context);
                if (context.Result == null)
                    await next();
            }

            #endregion
        }

        #endregion
    }
}