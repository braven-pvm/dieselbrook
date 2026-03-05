using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Vendors
{
    public partial class VendorAttributeValueModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}
