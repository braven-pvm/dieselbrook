using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public partial class AddProductToCompareListResponse : BaseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }
    }
}
