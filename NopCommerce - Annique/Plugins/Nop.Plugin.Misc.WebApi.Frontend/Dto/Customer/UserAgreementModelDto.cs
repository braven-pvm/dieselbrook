using System;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class UserAgreementModelDto : ModelDto
    {
        public Guid OrderItemGuid { get; set; }

        public string UserAgreementText { get; set; }
    }
}
