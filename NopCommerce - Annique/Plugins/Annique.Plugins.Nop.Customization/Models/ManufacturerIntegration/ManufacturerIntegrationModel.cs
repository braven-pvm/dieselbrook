using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Annique.Plugins.Nop.Customization.Models.ManufacturerIntegration
{
    public record ManufacturerIntegrationModel :BaseNopEntityModel
    {
        public int ManufacturerId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Manufacturer.ManufacturerIntegration.Fields.IntegrationField")]
        public string IntegrationField { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Manufacturer.ManufacturerIntegration.Fields.IntegrationValue")]
        public string IntegrationValue { get; set; }
    }
}
