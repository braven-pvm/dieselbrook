using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models
{
    public record CustomCategoryTabInfoModel : BaseNopEntityModel
    {
        public CustomCategoryTabInfoModel()
        {
            CategoryIntegrationSearchModel = new CategoryIntegrationSearchModel();
            CategoryIntegrationModel = new CategoryIntegrationModel();
        }

        public CategoryIntegrationSearchModel CategoryIntegrationSearchModel { get; set; }

        public CategoryIntegrationModel CategoryIntegrationModel { get; set; }
    }
}
