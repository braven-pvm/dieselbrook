using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class CustomerDownloadableProductsModelDto : ModelDto
    {
        public IList<DownloadableProductsModelDto> Items { get; set; }
    }
}
