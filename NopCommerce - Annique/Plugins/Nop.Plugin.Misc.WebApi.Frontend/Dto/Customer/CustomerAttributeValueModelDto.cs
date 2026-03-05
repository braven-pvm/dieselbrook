using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class CustomerAttributeValueModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}
