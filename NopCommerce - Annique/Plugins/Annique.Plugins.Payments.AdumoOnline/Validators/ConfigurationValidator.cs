using Annique.Plugins.Payments.AdumoOnline.Models;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Annique.Plugins.Payments.AdumoOnline.Validators
{
    /// <summary>
    /// Represents configuration model validator
    /// </summary>
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.FormPostUrl)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Annique.AdumoOnline.Fields.FormPostUrl.Required"));

            RuleFor(model => model.MerchantId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Annique.AdumoOnline.Fields.MerchantId.Required"));

            RuleFor(model => model.ApplicationId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Annique.AdumoOnline.Fields.ApplicationId.Required"));

            RuleFor(model => model.Secret)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Annique.AdumoOnline.Fields.Secret.Required"));
        }

        #endregion
    }
}