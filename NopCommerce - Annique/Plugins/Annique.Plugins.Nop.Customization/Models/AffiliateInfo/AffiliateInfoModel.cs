using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Annique.Plugins.Nop.Customization.Models.AffiliateInfo
{
    public record AffiliateInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Account.AffiliateInfo.AffiliateName")]
        public string AffiliateName { get; set; }
    }
}
