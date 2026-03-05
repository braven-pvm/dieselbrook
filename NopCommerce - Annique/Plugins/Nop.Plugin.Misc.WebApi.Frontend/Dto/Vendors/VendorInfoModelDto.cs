using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Vendors
{
    public partial class VendorInfoModelDto : ModelDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public string PictureUrl { get; set; }

        public IList<VendorAttributeModelDto> VendorAttributes { get; set; }
    }
}
