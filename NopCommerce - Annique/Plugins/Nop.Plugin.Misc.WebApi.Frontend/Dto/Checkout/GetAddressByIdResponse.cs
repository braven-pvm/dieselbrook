using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class GetAddressByIdResponse : BaseDto
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
    }
}
