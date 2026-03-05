using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Common
{
    public partial class AddressAttributeValueModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}
