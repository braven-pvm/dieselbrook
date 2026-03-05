using Nop.Web.Framework.Models;

namespace Annique.Plugins.Payments.AdumoOnline.Models
{
    /// <summary>
    /// Represents Payment info model
    /// </summary>
    public record PaymentInfoModel : BaseNopModel
    {
        //gets sets form post url property
        public string FormPostUrl { get; set; }

        //gets sets Merchant id property
        public string Cuid { get; set; }

        //gets sets Application id property
        public string Auid { get; set; }

        //gets sets merchant ref property
        public string Mref { get; set; }

        //gets sets currency code property
        public string Currency { get; set; }

        //gets sets order amount property
        public string Amount { get; set; }

        //gets sets JWT signed token property
        public string Token { get; set; }

        //gets sets success call back url property
        public string SuccessCallBackUrl { get; set; }

        //gets sets fail call back url property
        public string FailCallBackUrl { get; set; }
    }
}
