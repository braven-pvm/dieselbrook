using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Sitemap
{
    public partial class SitemapXmlResponse : BaseDto
    {
        public string SiteMapXML { get; set; }

        public string MimeType { get; set; }
    }
}
