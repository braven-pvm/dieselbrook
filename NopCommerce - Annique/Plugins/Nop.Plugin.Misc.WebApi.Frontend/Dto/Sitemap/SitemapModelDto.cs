using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Sitemap
{
    public partial class SitemapModelDto : ModelDto
    {
        public List<SitemapItemModelDto> Items { get; set; }

        public SitemapPageModelDto PageModel { get; set; }
    }
}
