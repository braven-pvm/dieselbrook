using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models
{
    public record CategoryIntegrationSearchModel : BaseSearchModel
    {
        public int CategoryId { get; set; }
    }
}
