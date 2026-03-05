using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class CustomerAddressEditModelDto : ModelDto
    {
        public AddressModelDto Address { get; set; }
    }
}
