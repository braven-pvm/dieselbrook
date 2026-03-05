using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ProductReviewReviewTypeMappingController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IReviewTypeService _reviewTypeService;

        #endregion

        #region Ctor

        public ProductReviewReviewTypeMappingController(IReviewTypeService reviewTypeService)
        {
            _reviewTypeService = reviewTypeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get product review and review type mappings by product review identifier
        /// </summary>
        /// <param name="productReviewId">The product review identifier</param>
        [HttpGet("{productReviewId}")]
        [ProducesResponseType(typeof(IList<ProductReviewReviewTypeMappingDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductReviewReviewTypeMappingsByProductReviewId(int productReviewId)
        {
            var mapping = await _reviewTypeService.GetProductReviewReviewTypeMappingsByProductReviewIdAsync(productReviewId);
            var mappingDto = mapping.Select(p => p.ToDto<ProductReviewReviewTypeMappingDto>());

            return Ok(mappingDto);
        }

        /// <summary>
        /// Inserts a product review and review type mapping
        /// </summary>
        /// <param name="model">Product review and review type mapping</param>
        [HttpPost]
        [ProducesResponseType(typeof(ProductReviewReviewTypeMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> InsertProductReviewReviewTypeMappings(
            [FromBody] ProductReviewReviewTypeMappingDto model)
        {
            var mapping = model.FromDto<ProductReviewReviewTypeMapping>();

            await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(mapping);

            var mappingDto = mapping.ToDto<ProductReviewReviewTypeMappingDto>();

            return Ok(mappingDto);
        }

        #endregion
    }
}
