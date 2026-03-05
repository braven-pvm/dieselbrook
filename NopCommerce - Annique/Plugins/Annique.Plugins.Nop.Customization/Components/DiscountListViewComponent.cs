using Annique.Plugins.Nop.Customization.Factories.DiscountAllocation;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using Nop.Web.Models.ShoppingCart;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays list of auto applied discounts
    /// </summary>
    [ViewComponent(Name = "DiscountList")]
    public class DiscountListViewComponent : NopViewComponent
    {
        #region Field

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IDiscountAllocationModelFactory _discountAllocationModelFactory;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public DiscountListViewComponent(
            IStoreContext storeContext,
            ISettingService settingService,
            IDiscountAllocationModelFactory discountAllocationModelFactory,
            IWorkContext workContext)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _discountAllocationModelFactory = discountAllocationModelFactory;
            _workContext = workContext;
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
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);
            if (settings != null && !settings.IsEnablePlugin)
                Content(string.Empty);

            if (!(additionalData is ShoppingCartModel shoppingCartModel))
                return Content(string.Empty);

            if (shoppingCartModel.Items.Count == 0)
                return Content(string.Empty);

            //Get current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            //prepare model
            var model = await _discountAllocationModelFactory.PrepareDiscountInfoListModelAsync(customer, shoppingCartModel);
            if (model.AvailableDiscounts.Any() || model.AppliedDiscountNames.Any())
                return View(model);

            return Content(string.Empty);
        }

        #endregion
    }
}
