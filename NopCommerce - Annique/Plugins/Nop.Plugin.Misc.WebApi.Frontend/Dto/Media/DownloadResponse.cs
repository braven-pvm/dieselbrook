using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Media
{
    public class DownloadResponse : BaseDto
    {
        /// <summary>
        /// File name with extension
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The URL to redirect
        /// </summary>
        public string RedirectToUrl { get; set; }

        /// <summary>
        /// The file content UTF8 encode
        /// </summary>
        public string DownloadBinary { get; set; }
    }
}
