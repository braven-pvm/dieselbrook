using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class ProductReviewsModelDto : ModelDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public IList<ProductReviewModelDto> Items { get; set; }

        public AddProductReviewModelDto AddProductReview { get; set; }

        public IList<ReviewTypeModelDto> ReviewTypeList { get; set; }

        public IList<AddProductReviewReviewTypeMappingModelDto> AddAdditionalProductReviewList { get; set; }
    }
}
