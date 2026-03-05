using Annique.Plugins.Nop.Customization.Models.UserLogin;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays special offer marquee
    /// </summary>
    [ViewComponent(Name = "ActiveSpecialOfferMarquee")]
    public class ActiveSpecialOfferMarqueeViewComponent : ViewComponent
    {
        #region Fields

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ISpecialOffersService _specialOffersService;

        #endregion

        #region Ctor

        public ActiveSpecialOfferMarqueeViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IWorkContext workContext,
            ICustomerService customerService,
            ISpecialOffersService specialOffersService)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _workContext = workContext;
            _customerService = customerService;
            _specialOffersService = specialOffersService;
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
            if (!widgetZone?.Equals(PublicWidgetZones.HeaderAfter, StringComparison.InvariantCultureIgnoreCase) ?? true)
                return Content(string.Empty);

            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return Content(string.Empty);

            var activeOffers = await _specialOffersService.GetActiveSpecialOfferListAsync();
            if (activeOffers.Count > 0) 
            {
                var discountNames = activeOffers.Select(o => o.Item2.Name).ToList();
                return View(discountNames);
            }
            return Content(string.Empty);
        }

        #endregion
    }
}