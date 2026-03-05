using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Sitemap
{
    public partial class SitemapItemModelDto : BaseDto
    {
        public string GroupTitle { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }
    }
}
