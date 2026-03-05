using Annique.Plugins.Nop.Customization.Models.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Framework.Components;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays claim your rewards link after header 
    /// </summary>
    [ViewComponent(Name = "AwardHeaderLink")]
    public class AwardHeaderLinkViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IWorkContext _workContext;
        private readonly IAwardService _awardService;

        #endregion

        #region Ctor

        public AwardHeaderLinkViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IWorkContext workContext,
            IAwardService awardService)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _workContext = workContext;
            _awardService = awardService;
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
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return Content(string.Empty);

            //Get current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            //check user has consultant role or not
            var isConsultant = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();

            //check consultant role exist or not 
            if (isConsultant)
            {
                //Get all Awards by customerId
                var consultantAwards = _awardService.GetAwardsByCustomerId(customer.Id);
                if(!consultantAwards.Any())
                    return Content(string.Empty);

                //prepare award header link model
                var model = new AwardHeaderLinkModel
                {
                    AwardListPageLink = Url.RouteUrl("AwardList")
                };
                return View(model);
            }

            return Content(string.Empty);
        }

        #endregion
    }
}
