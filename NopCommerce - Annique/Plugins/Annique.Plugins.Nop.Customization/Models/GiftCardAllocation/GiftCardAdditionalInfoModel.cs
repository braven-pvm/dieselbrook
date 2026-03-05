using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Annique.Plugins.Nop.Customization.Models.GiftCardAllocation
{
    public record GiftCardAdditionalInfoModel : BaseNopEntityModel
    {
        public int GiftCardId { get; set; }

        [NopResourceDisplayName("Admin.GiftCards.Fields.Username")]
        public string Username { get; set; }
    }
}