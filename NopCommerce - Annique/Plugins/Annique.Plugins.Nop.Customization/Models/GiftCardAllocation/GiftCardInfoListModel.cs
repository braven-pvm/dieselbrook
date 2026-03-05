using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.GiftCardAllocation
{
    public record GiftCardInfoListModel : BaseNopModel
    {
        public GiftCardInfoListModel()
        {
            GiftCardModels = new List<GiftCardInfoModel>();
        }
        public IList<GiftCardInfoModel> GiftCardModels { get; set; }

        public record GiftCardInfoModel : BaseNopModel
        {
            public string CouponCode { get; set; }

            public string Amount { get; set; }

            public string Message { get; set; }
        }
    }
}
