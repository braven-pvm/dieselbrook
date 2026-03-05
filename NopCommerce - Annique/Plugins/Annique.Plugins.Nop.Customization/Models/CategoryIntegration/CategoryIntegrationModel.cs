using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Annique.Plugins.Nop.Customization.Models
{
    public record CategoryIntegrationModel : BaseNopEntityModel
    {
        public int CategoryId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Category.CategoryIntegration.Fields.IntegrationField")]
        public string IntegrationField { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Category.CategoryIntegration.Fields.IntegrationValue")]
        public string IntegrationValue { get; set; }
    }
}
