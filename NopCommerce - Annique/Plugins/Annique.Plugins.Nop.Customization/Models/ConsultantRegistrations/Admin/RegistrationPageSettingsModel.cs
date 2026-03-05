using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Admin
{
    public record RegistrationPageSettingsModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Consultant.Registration.CustomCSS")]
        public string CustomCSS { get; set; }

        [NopResourceDisplayName("Plugins.Consultant.Registration.CustomJS")]
        public string CustomJS { get; set; }
        [NopResourceDisplayName("Plugins.Consultant.Registration.TopSectionPublished")]
        public bool TopSectionPublished { get; set; }

        [NopResourceDisplayName("Plugins.Consultant.Registration.TopSectionBody")]
        public string TopSectionBody { get; set; }

        [NopResourceDisplayName("Plugins.Consultant.Registration.LeftSectionPublished")]
        public bool LeftSectionPublished { get; set; }

        [NopResourceDisplayName("Plugins.Consultant.Registration.LeftSectionBody")]
        public string LeftSectionBody { get; set; }

        [NopResourceDisplayName("Plugins.Consultant.Registration.BottomSectionPublished")]
        public bool BottomSectionPublished { get; set; }

        [NopResourceDisplayName("Plugins.Consultant.Registration.BottomSectionBody")]
        public string BottomSectionBody { get; set; }
    }
}
