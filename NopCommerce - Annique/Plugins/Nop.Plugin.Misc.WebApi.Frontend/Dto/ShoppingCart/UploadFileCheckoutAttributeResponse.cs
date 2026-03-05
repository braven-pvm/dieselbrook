using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class UploadFileCheckoutAttributeResponse : BaseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string DownloadUrl { get; set; }

        public string DownloadGuid { get; set; }
    }
}
