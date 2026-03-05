using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Annique.Plugins.Nop.Customization.Validators.AnniqueReports
{
    public class ReportParameterModelValidator : BaseNopValidator<ReportParameterModel>
    {
        public ReportParameterModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Admin.ReportParameter.Fields.Name.Required"));
        }
    }
}

