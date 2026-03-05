using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Annique.Plugins.Nop.Customization.Validators.AnniqueReports
{
    public class ReportModelValidator : BaseNopValidator<ReportModel>
    {
        public ReportModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Admin.AnniqueReport.Fields.Name.Required"));
            RuleFor(x => x.DisplayOrder)
               .GreaterThanOrEqualTo(0)
               .WithMessageAwait(localizationService.GetResourceAsync("Admin.AnniqueReport.Fields.DisplayOrder.Required"));
        }
    }
}
