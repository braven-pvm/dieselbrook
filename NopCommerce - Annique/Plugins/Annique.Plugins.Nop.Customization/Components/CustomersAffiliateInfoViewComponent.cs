using Annique.Plugins.Nop.Customization.Models.AffiliateInfo;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Affiliates;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays Customer Affiliate Info 
    /// </summary>
    [ViewComponent(Name = "CustomersAffiliateInfo")]
    public class CustomersAffiliateInfoViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IWorkContext _workContext;
        private readonly IAffiliateService _affiliateService;

        #endregion

        #region Ctor

        public CustomersAffiliateInfoViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IWorkContext workContext,
            IAffiliateService affiliateService)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _workContext = workContext;
            _affiliateService = affiliateService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            var isEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!isEnable)
                return Content(string.Empty);

            var isConsultant = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();

            //if user consultant then return empty as for #Task 572 Remove Affiliate block on Profile for Consultant Role
            if (isConsultant)
                return Content(string.Empty);

            //Get current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            //If affiliate Id is greater than 0 means customer has Affiliate
            if (customer.AffiliateId > 0)
            {
                //Get affiliate 
                var affiliate = await _affiliateService.GetAffiliateByIdAsync(customer.AffiliateId);

                var model = new AffiliateInfoModel
                {
                    AffiliateName = await _affiliateService.GetAffiliateFullNameAsync(affiliate)
                };
                return View(model);
            }

            return Content(string.Empty);
        }

        #endregion
    }
}