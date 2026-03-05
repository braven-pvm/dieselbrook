using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ReturnRequests
{
    public partial class CustomerReturnRequestsModelDto : ModelDto
    {
        public IList<ReturnRequestModelDto> Items { get; set; }
    }
}
