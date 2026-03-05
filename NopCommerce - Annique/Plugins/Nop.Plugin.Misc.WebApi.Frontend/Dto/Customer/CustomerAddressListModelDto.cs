using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class CustomerAddressListModelDto : ModelDto
    {
        public IList<AddressModelDto> Addresses { get; set; }
    }
}
