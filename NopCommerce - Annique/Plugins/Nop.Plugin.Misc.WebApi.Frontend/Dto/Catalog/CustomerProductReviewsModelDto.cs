using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class CustomerProductReviewsModelDto : ModelDto
    {
        public IList<CustomerProductReviewModelDto> ProductReviews { get; set; }

        public PagerModelDto PagerModel { get; set; }
    }
}
