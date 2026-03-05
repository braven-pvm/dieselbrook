using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using XcellenceIT.Plugin.ProductRibbons.Models;

namespace XcellenceIT.Plugin.ProductRibbons.Validator
{
    public class ProductRibbonValidator : BaseNopValidator<ProductRibbonModel>
    {
        public ProductRibbonValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.RibbonName)
                .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.RibbonName.Required"));

            // Ensure TechnicalHeader is provided
            When(x => x.EndDateUtc.HasValue == true && x.StartDateUtc.HasValue == true, async () =>
            {
                RuleFor(m => m.EndDateUtc.Value)
              .GreaterThan(m => m.StartDateUtc.Value)
                      .WithMessageAwait(localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.EndDateUtc.GreaterThenStartDate"));

            });
        }
    }
}
