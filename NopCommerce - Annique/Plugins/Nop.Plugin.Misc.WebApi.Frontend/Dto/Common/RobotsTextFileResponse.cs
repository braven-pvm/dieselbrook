using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Common
{
    public partial class RobotsTextFileResponse : BaseDto
    {
        public string RobotsFileContent { get; set; }

        public string MimeType { get; set; }
    }
}
