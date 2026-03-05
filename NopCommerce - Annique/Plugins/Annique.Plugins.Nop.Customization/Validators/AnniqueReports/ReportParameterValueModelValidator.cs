using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Annique.Plugins.Nop.Customization.Validators.AnniqueReports
{
    public class ReportParameterValueModelValidator:BaseNopValidator<ReportParameterValueModel>
    {
        public ReportParameterValueModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Admin.ReportParameterValue.Fields.Name.Required"));
        }
    }
}
