using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models.ManufacturerIntegration
{
    public record ManufacturerIntegrationSearchModel : BaseSearchModel
    {
        public int ManufacturerId { get; set; }
    }
}
