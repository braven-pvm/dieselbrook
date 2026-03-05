using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ProductReviewController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public ProductReviewController(IProductService productService)
        {
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get product reviews by identifiers
        /// </summary>
        /// <param name="ids">Array of Product review identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<ProductReviewDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductReviewsByIds(string ids)
        {
            var productReviewsId = ids.ToIdArray();

            var productReviews = await _productService.GetProductReviewsByIdsAsync(productReviewsId);
            var productReviewsDto = productReviews.Select(pr => pr.ToDto<ProductReviewDto>());

            return Ok(productReviewsDto);
        }

        /// <summary>
        /// Deletes product reviews
        /// </summary>
        /// <param name="ids">Array of Product review identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        public virtual async Task<IActionResult> DeleteProductReviews(string ids)
        {
            var productReviewsId = ids.ToIdArray();

            var productReviews = await _productService.GetProductReviewsByIdsAsync(productReviewsId);

            await _productService.DeleteProductReviewsAsync(productReviews);

            return Ok();
        }

        /// <summary>
        /// Sets or create a product review helpfulness record
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        /// <param name="helpfulness">Value indicating whether a review a helpful</param>
        [HttpGet("{productReviewId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SetProductReviewHelpfulness(int productReviewId, [FromQuery][Required] bool helpfulness)
        {
            if (productReviewId <= 0)
                return BadRequest();

            var productReview = await _productService.GetProductReviewByIdAsync(productReviewId);

            if (productReview == null)
                return NotFound($"Product review Id={productReviewId} not found");

            await _productService.SetProductReviewHelpfulnessAsync(productReview, helpfulness);

            return Ok();
        }

        /// <summary>
        /// Updates a totals helpfulness count for product review
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        [HttpGet("{productReviewId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateProductReviewHelpfulnessTotals(int productReviewId)
        {
            if (productReviewId <= 0)
                return BadRequest();

            var productReview = await _productService.GetProductReviewByIdAsync(productReviewId);

            if (productReview == null)
                return NotFound($"Product review Id={productReviewId} not found");

            await _productService.UpdateProductReviewHelpfulnessTotalsAsync(productReview);

            return Ok();
        }

        /// <summary>
        /// Check possibility added review for current customer
        /// </summary>
        /// <param name="productId">Current product</param>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CanAddReview(int productId, [FromQuery] int storeId = 0)
        {
            var flag = await _productService.CanAddReviewAsync(productId, storeId);

            return Ok(flag);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productReview = await _productService.GetProductReviewByIdAsync(id);

            if (productReview == null)
                return NotFound($"Product review Id={id} not found");

            await _productService.DeleteProductReviewAsync(productReview);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productReview = await _productService.GetProductReviewByIdAsync(id);

            if (productReview == null)
                return NotFound($"Product review Id={id} not found");

            return Ok(productReview.ToDto<ProductReviewDto>());
        }

        /// <summary>
        /// Gets all product reviews
        /// </summary>
        /// <param name="customerId">Customer identifier (who wrote a review); 0 to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item item creation to; null to load all records</param>
        /// <param name="message">Search title or review text; null to load all records</param>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="productId">The product identifier; pass 0 to load all records</param>
        /// <param name="vendorId">The vendor identifier (limit to products of this vendor); pass 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ProductReview, ProductReviewDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int customerId = 0,
            [FromQuery] bool? approved = null,
            [FromQuery] DateTime? fromUtc = null,
            [FromQuery] DateTime? toUtc = null,
            [FromQuery] string message = null,
            [FromQuery] int storeId = 0,
            [FromQuery] int productId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] bool showHidden = false,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var productReviews = await _productService.GetAllProductReviewsAsync(customerId, approved, fromUtc, toUtc,
                message, storeId, productId, vendorId, showHidden, pageIndex, pageSize);

            return Ok(productReviews.ToPagedListDto<ProductReview, ProductReviewDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductReviewDto model)
        {
            var productReview = model.FromDto<ProductReview>();

            await _productService.InsertProductReviewAsync(productReview);

            var productReviewDto = productReview.ToDto<ProductReviewDto>();

            return Ok(productReviewDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductReviewDto model)
        {
            var productReview = await _productService.GetProductReviewByIdAsync(model.Id);

            if (productReview == null)
                return NotFound($"Product review Id={model.Id} is not found");

            productReview = model.FromDto<ProductReview>();

            await _productService.UpdateProductReviewAsync(productReview);

            return Ok();
        }

        #endregion
    }
}
