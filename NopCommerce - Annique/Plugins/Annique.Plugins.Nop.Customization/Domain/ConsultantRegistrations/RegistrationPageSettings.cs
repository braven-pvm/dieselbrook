using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations
{
    public class RegistrationPageSettings : BaseEntity
    {
        public string CustomCSS { get; set; }

        public string CustomJS { get; set; }

        // Top section
        public bool TopSectionPublished { get; set; }
        public string TopSectionBody { get; set; }

        // Left section
        public bool LeftSectionPublished { get; set; }
        public string LeftSectionBody { get; set; }

        // Bottom section
        public bool BottomSectionPublished { get; set; }
        public string BottomSectionBody { get; set; }
    }
}
