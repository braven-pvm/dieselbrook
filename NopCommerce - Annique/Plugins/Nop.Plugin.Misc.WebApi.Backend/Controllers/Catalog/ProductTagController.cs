using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ProductTagController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;

        #endregion

        #region Ctor

        public ProductTagController(IProductService productService,
            IProductTagService productTagService)
        {
            _productService = productService;
            _productTagService = productTagService;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Delete product tags
        /// </summary>
        /// <param name="ids">Array of product tag identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        public virtual async Task<IActionResult> DeleteProductTags(string ids)
        {
            var productTagId = ids.ToIdArray();
            var productTags = await _productTagService.GetProductTagsByIdsAsync(productTagId);

            await _productTagService.DeleteProductTagsAsync(productTags);

            return Ok();
        }

        /// <summary>
        /// Gets product tags by identifier
        /// </summary>
        /// <param name="ids">Array of product tags identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategoriesByIds(string ids)
        {
            var productTagsId = ids.ToIdArray();
            var productTags = await _productTagService.GetProductTagsByIdsAsync(productTagsId);

            var productTagDto = productTags.Select(c => c.ToDto<CategoryDto>());

            return Ok(productTagDto);
        }

        /// <summary>
        /// Gets all product tags by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<ProductTagDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllProductTagsByProductId(int productId)
        {
            var productTags = await _productTagService.GetAllProductTagsByProductIdAsync(productId);
            var productTagsDto = productTags.Select(pc => pc.ToDto<ProductTagDto>());

            return Ok(productTagsDto);
        }

        /// <summary>
        /// Get products quantity linked to a passed tag identifier
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{productTagId}/{storeId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductCountByProductTagId(int productTagId,
            int storeId,
            [FromQuery] bool showHidden = false)
        {
            return Ok(await _productTagService.GetProductCountByProductTagIdAsync(productTagId, storeId, showHidden));
        }

        /// <summary>
        /// Get product count for every linked tag
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{storeId}")]
        [ProducesResponseType(typeof(Dictionary<int, int>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductCount(int storeId, [FromQuery] bool showHidden = false)
        {
            return Ok(await _productTagService.GetProductCountAsync(storeId, showHidden));
        }

        /// <summary>
        /// Update product tags
        /// </summary>
        /// <param name="productId">Product id for update</param>
        /// <param name="productTags">Array of product tags (separator - ;)</param>
        [HttpGet("{productId}/{productTags}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateProductTags(int productId, string productTags)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            await _productTagService.UpdateProductTagsAsync(product, productTags.Split(';'));

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productTag = await _productTagService.GetProductTagByIdAsync(id);

            if (productTag == null)
                return NotFound($"Product tag Id={id} not found");

            await _productTagService.DeleteProductTagAsync(productTag);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductTagDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productTag = await _productTagService.GetProductTagByIdAsync(id);

            if (productTag == null)
                return NotFound($"Product tag Id={id} not found");

            return Ok(productTag.ToDto<ProductTagDto>());
        }

        /// <summary>
        /// Gets all product tags
        /// </summary>
        /// <param name="tagName">Tag name</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductTagDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] string tagName = null)
        {
            var productTags = await _productTagService.GetAllProductTagsAsync(tagName);
            var productTagsDto = productTags.Select(pt => pt.ToDto<ProductTagDto>());

            return Ok(productTagsDto);
        }
        
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductTagDto model)
        {
            var productTag = await _productTagService.GetProductTagByIdAsync(model.Id);

            if (productTag == null)
                return NotFound($"Product tag Id={model.Id} is not found");

            productTag = model.FromDto<ProductTag>();

            await _productTagService.UpdateProductTagAsync(productTag);

            return Ok();
        }

        /// <summary>
        /// Inserts a product-product tag mapping
        /// </summary>
        [HttpPost]
        public virtual async Task<IActionResult> InsertProductProductTagMapping([FromBody] ProductProductTagMappingDto model)
        {
            var mapping = model.FromDto<ProductProductTagMapping>();

            await _productTagService.InsertProductProductTagMappingAsync(mapping);

            return Ok();
        }

        #endregion
    }
}
