using Annique.Plugins.Nop.Customization.Models.UserProfile;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Annique.Plugins.Nop.Customization.Validators.UserProfile
{
    public class UserProfileAdditionalInfoValidator : BaseNopValidator<UserProfileAdditionalInfoModel>
    {
        public UserProfileAdditionalInfoValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Account.UserProfileAdditionalInfo.Title.Required"))
               .When(x => x.IsConsultant);

            RuleFor(x => x.Nationality)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Account.UserProfileAdditionalInfo.Nationality.Required"))
               .When(x => x.IsConsultant);

            RuleFor(x => x.IdNumber)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Account.UserProfileAdditionalInfo.IdNumber.Required"))
               .When(x => x.IsConsultant);

            RuleFor(x => x.Language)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Account.UserProfileAdditionalInfo.Language.Required"))
               .When(x => x.IsConsultant);

            RuleFor(x => x.Ethnicity)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Account.UserProfileAdditionalInfo.Ethnicity.Required"))
               .When(x => x.IsConsultant);
        }
    }
}
