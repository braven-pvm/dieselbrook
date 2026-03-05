using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Affiliates;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class AffiliateLinksActionFilter : IActionFilter
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public AffiliateLinksActionFilter(IWebHelper webHelper,
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Generate affiliate URL
        /// </summary>
        /// <param name="AffilateModel">Affiliate Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the generated affiliate URL
        /// </returns>
        public Task<string> GenerateUrlAsync(AffiliateModel affiliateModel)
        {
            var storeUrl = _webHelper.GetStoreLocation();
            var url = !string.IsNullOrEmpty(affiliateModel.FriendlyUrlName) ?
                //use friendly URL
                _webHelper.ModifyQueryString(storeUrl, AnniqueCustomizationDefaults.AffiliateQueryParameter, affiliateModel.FriendlyUrlName) :
                //use ID
                _webHelper.ModifyQueryString(storeUrl, AnniqueCustomizationDefaults.AffiliateQueryParameter, affiliateModel.Id.ToString());

            return Task.FromResult(url);
        }

        #endregion

        #region Methods

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {

        }

        public async void OnActionExecuted(ActionExecutedContext context)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

            if (controllerActionDescriptor.ControllerTypeInfo == typeof(AffiliateController) && context.HttpContext.Request.Method.ToString() == "GET" &&
                       controllerActionDescriptor.ActionName.Equals("Edit"))
            {
                var controller = context.Controller as Controller;
                AffiliateModel affilateModel = controller.ViewData.Model as AffiliateModel;

                //get active store
                var storeScope = await _storeContext.GetCurrentStoreAsync();

                //get Active store Annique Settings
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

                //If plugin is disable do not continue
                if (!settings.IsEnablePlugin)
                    return;

                //if affilate model is null return
                if (affilateModel == null)
                    return;

                //populate url with custom query string
                affilateModel.Url = await GenerateUrlAsync(affilateModel);
            }
        }

        #endregion
    }
}
