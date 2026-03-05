using Annique.Plugins.Nop.Customization.Models;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Logging;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays trip promotion message on cart page
    /// </summary>
    [ViewComponent(Name = "TripPromotion")]
    public class TripPromotionViewComponent : ViewComponent
    {
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;

        public TripPromotionViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IWorkContext workContext,
            ILogger logger)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _workContext = workContext;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(string totalRSPAmount)
        {
            try
            {
                // Parse and sanitize input
                decimal totalRsp = ParseDecimalOrDefault(totalRSPAmount);

                // Check if trip promotion should be shown
                var (shouldShow, promotionMessage) = await _anniqueCustomizationConfigurationService
                    .ShouldShowTripPromotionAsync(totalRsp);

                if (!shouldShow)
                    return Content(string.Empty);

                var model = new TripPromotionModel
                {
                    PromotionMessage = promotionMessage
                };

                return View(model);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Content(string.Empty);
            }
        }

        private decimal ParseDecimalOrDefault(string input)
        {
            return decimal.TryParse(input, out var value) ? value : decimal.Zero;
        }
    }
}
