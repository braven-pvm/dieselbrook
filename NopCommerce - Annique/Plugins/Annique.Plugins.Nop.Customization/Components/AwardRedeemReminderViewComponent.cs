using Annique.Plugins.Nop.Customization.Models.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays reminder to redeem Award
    /// </summary>
    [ViewComponent(Name = "AwardRedeemReminder")]
    public class AwardRedeemReminderViewComponent : NopViewComponent
    {
        #region Field

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IAwardService _awardService;

        #endregion

        #region Ctor

        public AwardRedeemReminderViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerService customerService,
            IAwardService awardService)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
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
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return Content(string.Empty);
           
            var customer = await _workContext.GetCurrentCustomerAsync();

            var isConsultant = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();
            if (!isConsultant)
                return Content(string.Empty);

            //get all customer awards
            var awards =  _awardService.GetAwardsByCustomerId(customer.Id);
            if (awards.Any())
            {
                var awardIds = awards.Select(a => a.Id);

                //get awards in cart
                var awardInCart = await _awardService.GetAwardShoppingCartItemsByCustomerIdAsync(customer.Id);

                // Check if there is at least one record in awardInCart associated with each award.
                bool hasRecordForEachAward = awardIds.All(id => awardInCart.Any(item => item.AwardId == id));

                //if cart has not record for each Award then return award redemtption link with reminder
                if (!hasRecordForEachAward)
                {
                    var model = new AwardHeaderLinkModel
                    {
                        AwardListPageLink = Url.RouteUrl("AwardList")
                    };
                    return View(model);
                }
            }
            return Content(string.Empty);
        }

        #endregion
    }
}


