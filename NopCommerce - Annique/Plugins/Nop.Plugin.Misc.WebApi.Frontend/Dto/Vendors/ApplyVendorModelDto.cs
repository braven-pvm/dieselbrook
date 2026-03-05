using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Vendors
{
    public partial class ApplyVendorModelDto : ModelDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public IList<VendorAttributeModelDto> VendorAttributes { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool TermsOfServiceEnabled { get; set; }

        public bool TermsOfServicePopup { get; set; }

        public bool DisableFormInput { get; set; }

        public string Result { get; set; }
    }
}
