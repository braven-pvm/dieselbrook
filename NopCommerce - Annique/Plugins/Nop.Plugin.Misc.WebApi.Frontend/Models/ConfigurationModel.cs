using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.WebApi.Frontend.Models
{
    /// <summary>
    /// Represents plugin configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.WebApi.Frontend.DeveloperMode")]
        public bool DeveloperMode { get; set; }

        [NopResourceDisplayName("Plugins.WebApi.Frontend.SecretKey")]
        public string SecretKey { get; set; }
    }
}
