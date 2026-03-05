using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Annique.Plugins.Payments.AdumoOnline.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel: BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        //gets sets plugin enable property
        [NopResourceDisplayName("Annique.AdumoOnline.Fields.IsEnablePlugin")]
        public bool IsEnablePlugin { get; set; }
        public bool IsEnablePlugin_OverrideForStore { get; set; }

        //gets sets plugin Form post Url property
        [NopResourceDisplayName("Annique.AdumoOnline.Fields.FormPostUrl")]
        public string FormPostUrl { get; set; }
        public bool FormPostUrl_OverrideForStore { get; set; }

        //gets sets plugin Merchant Id property
        [NopResourceDisplayName("Annique.AdumoOnline.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        //gets sets plugin Application Id property
        [NopResourceDisplayName("Annique.AdumoOnline.Fields.ApplicationId")]
        public string ApplicationId { get; set; }
        public bool ApplicationId_OverrideForStore { get; set; }

        //gets sets plugin JWT secret property
        [NopResourceDisplayName("Annique.AdumoOnline.Fields.Secret")]
        public string Secret { get; set; }
        public bool Secret_OverrideForStore { get; set; }

        //gets sets plugin Additional fee property
        [NopResourceDisplayName("Annique.AdumoOnline.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        //gets sets plugin Additional fee percentage property
        [NopResourceDisplayName("Annique.AdumoOnline.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

    }
}
