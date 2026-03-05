using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models.ConsultantAwards
{
    public record AwardHeaderLinkModel : BaseNopModel
    {
        public string AwardListPageLink { get; set; }
    }
}
