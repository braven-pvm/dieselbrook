using FluentValidation;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Frontend.Models;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.WebApi.Frontend.Validators
{
    /// <summary>
    /// Represents configuration model validator
    /// </summary>
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationValidator()
        {
            RuleFor(model => model.SecretKey)
                .NotEmpty()
                .MinimumLength(WebApiCommonDefaults.MinSecretKeyLength);
        }

        #endregion
    }
}