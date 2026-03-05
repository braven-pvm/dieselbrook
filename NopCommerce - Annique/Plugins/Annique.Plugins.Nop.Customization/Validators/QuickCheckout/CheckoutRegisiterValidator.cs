using Annique.Plugins.Nop.Customization.Models.QuickCheckout;
using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Annique.Plugins.Nop.Customization.Validators.QuickCheckout
{
    public class CheckoutRegisiterValidator : BaseNopValidator<CheckoutRegisterModel>
    {
        public CheckoutRegisiterValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));

            RuleFor(x => x.ConfirmEmail).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.ConfirmEmail.Required"));
            RuleFor(x => x.ConfirmEmail).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
            RuleFor(x => x.ConfirmEmail).Equal(x => x.Email).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Email.EnteredEmailsDoNotMatch"));
            
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.Name.Required"));
           
            //Password rule
            RuleFor(x => x.Password).IsPassword(localizationService, customerSettings);

            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.ConfirmPassword.Required"));
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Password.EnteredPasswordsDoNotMatch"));

            RuleFor(x => x.Phone).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Register.Fields.Phone.Required"));
            RuleFor(x => x.Phone).Matches(@"^0(6|7|8){1}[0-9]{1}[0-9]{7}$").WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
        }
    }
}
