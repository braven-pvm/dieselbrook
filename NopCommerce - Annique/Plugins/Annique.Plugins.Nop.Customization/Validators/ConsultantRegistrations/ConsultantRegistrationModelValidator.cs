using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public;
using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Annique.Plugins.Nop.Customization.Validators
{
    public class ConsultantRegistrationModelValidator : BaseNopValidator<ConsultantRegistrationModel>
    {
        public ConsultantRegistrationModelValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            RuleFor(x => x.FirstName)
             .NotEmpty()
             .WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.FirstName.Required"));

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.LastName.Required"));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Email.Required"))
                .EmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));

            RuleFor(x => x.ConfirmEmail)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.ConfirmEmail.Required"))
                .EmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"))
                .Equal(x => x.Email)
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Email.EnteredEmailsDoNotMatch"));

            RuleFor(x => x.Cell)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.Phone.Required"))
                .Matches(@"^0(6|7|8){1}[0-9]{1}[0-9]{7}$")
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));

            RuleFor(x => x.Whatsapp)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.Whatsapp.Required"))
                .Matches(@"^0(6|7|8){1}[0-9]{1}[0-9]{7}$")
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));

            RuleFor(x => x.Postcode)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.Postcode.Required"));

            RuleFor(x => x.SelectedLanguage)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.Language.Required"));

            RuleFor(x => x.SelectedCallTime)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.BestTimeToCall.Required"));
        }
    }
}
