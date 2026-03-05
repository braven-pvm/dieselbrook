using Nop.Core.Configuration;

namespace Annique.Plugins.Payments.AdumoOnline
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class AdumoOnlineSettings : ISettings
    {
        //gets sets plugin enable property
        public bool IsEnablePlugin { get; set; }

        //gets sets plugin Form post URl property
        public string FormPostUrl { get; set; }

        //gets sets plugin Merchant Id property
        public string MerchantId { get; set; }

        //gets sets plugin Application Id property
        public string ApplicationId { get; set; }

        //gets sets plugin JWT secret property
        public string Secret { get; set; }

        //gets sets plugin Additional Fee property
        public decimal AdditionalFee { get; set; }

        //gets sets plugin Additional Fee percentage property
        public bool AdditionalFeePercentage { get; set; }
    }
}
