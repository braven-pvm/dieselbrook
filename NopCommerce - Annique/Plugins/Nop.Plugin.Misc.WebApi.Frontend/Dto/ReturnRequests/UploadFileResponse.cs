using System;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ReturnRequests
{
    public partial class UploadFileResponse : BaseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string DownloadUrl { get; set; }

        public Guid DownloadGuid { get; set; }
    }
}
