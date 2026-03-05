using Annique.Plugins.Nop.Customization.Models.QuickCheckout;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Annique.Plugins.Nop.Customization.Validators.QuickCheckout
{
    public partial class CheckoutLoginValidator : BaseNopValidator<CheckoutLoginModal>
    {
        public CheckoutLoginValidator(ILocalizationService localizationService)
        {
                //login by username
                RuleFor(x => x.Username).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Login.Fields.Name.Required"));
                RuleFor(x => x.Password).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Login.Fields.Password.Required"));
        }
    }
}