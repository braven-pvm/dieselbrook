using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class InfoResponse : BaseDto
    {
        public CustomerInfoModelDto Model { get; set; }

        public IList<string> Errors { get; set; }
    }
}
