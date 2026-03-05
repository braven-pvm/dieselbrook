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
    public partial class RelatedProductsController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public RelatedProductsController(IProductService productService)
        {
            _productService = productService;
        }

        #endregion

        #region Methods

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var relatedProduct = await _productService.GetRelatedProductByIdAsync(id);

            if (relatedProduct == null)
                return NotFound($"Related product Id={id} not found");

            await _productService.DeleteRelatedProductAsync(relatedProduct);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RelatedProductDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var relatedProduct = await _productService.GetRelatedProductByIdAsync(id);

            if (relatedProduct == null)
                return NotFound($"Related product Id={id} not found");

            return Ok(relatedProduct.ToDto<RelatedProductDto>());
        }

        /// <summary>
        /// Gets related products by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{productId1}")]
        [ProducesResponseType(typeof(IList<RelatedProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRelatedProductsByProductId1(int productId1, [FromQuery] bool showHidden = false)
        {
            var relatedProducts = await _productService.GetRelatedProductsByProductId1Async(productId1, showHidden);
            var relatedProductsDto = relatedProducts.Select(rp => rp.ToDto<RelatedProductDto>());

            return Ok(relatedProductsDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RelatedProductDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] RelatedProductDto model)
        {
            var relatedProduct = model.FromDto<RelatedProduct>();

            await _productService.InsertRelatedProductAsync(relatedProduct);

            var relatedProductDto = relatedProduct.ToDto<RelatedProductDto>();

            return Ok(relatedProductDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] RelatedProductDto model)
        {
            var relatedProduct = await _productService.GetRelatedProductByIdAsync(model.Id);

            if (relatedProduct == null)
                return NotFound($"Related product Id={model.Id} is not found");

            relatedProduct = model.FromDto<RelatedProduct>();

            await _productService.UpdateRelatedProductAsync(relatedProduct);

            return Ok();
        }

        #endregion
    }
}
