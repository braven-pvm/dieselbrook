using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models.ManufacturerIntegration
{
    public record CustomManufacturerTabInfoModel : BaseNopEntityModel
    {
        public CustomManufacturerTabInfoModel()
        {
            ManufacturerIntegrationSearchModel = new ManufacturerIntegrationSearchModel();
            ManufacturerIntegrationModel = new ManufacturerIntegrationModel();
        }

        public ManufacturerIntegrationSearchModel ManufacturerIntegrationSearchModel { get; set; }

        public ManufacturerIntegrationModel ManufacturerIntegrationModel { get; set; }
    }
}
