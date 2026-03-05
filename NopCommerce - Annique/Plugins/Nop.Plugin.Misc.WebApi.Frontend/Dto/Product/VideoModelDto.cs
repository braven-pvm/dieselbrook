using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public class VideoModelDto : ModelDto
    {
        public string VideoUrl { get; set; }

        public string Allow { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
