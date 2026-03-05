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
    public partial class ProductPicturesController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public ProductPicturesController(IProductService productService)
        {
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a product pictures by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<ProductPictureDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductPicturesByProductId(int productId)
        {
            var pictures = await _productService.GetProductPicturesByProductIdAsync(productId);
            var picturesDto = pictures.Select(pp => pp.ToDto<ProductPictureDto>());

            return Ok(picturesDto);
        }

        /// <summary>
        /// Get the IDs of all product images 
        /// </summary>
        /// <param name="productsIds">Array of product identifiers (separator - ;)</param>
        [HttpGet("{productsIds}")]
        [ProducesResponseType(typeof(IDictionary<int, int[]>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductsImagesIds(string productsIds)
        {
            var ids = productsIds.ToIdArray();
            var rez = await _productService.GetProductsImagesIdsAsync(ids);

            return Ok(rez);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productPicture = await _productService.GetProductPictureByIdAsync(id);

            if (productPicture == null)
                return NotFound($"Product picture Id={id} not found");

            await _productService.DeleteProductPictureAsync(productPicture);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductPictureDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productPicture = await _productService.GetProductPictureByIdAsync(id);

            if (productPicture == null)
                return NotFound($"Product picture Id={id} not found");

            return Ok(productPicture.ToDto<ProductPictureDto>());
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ProductPictureDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductPictureDto model)
        {
            var productPicture = model.FromDto<ProductPicture>();

            await _productService.InsertProductPictureAsync(productPicture);

            var productPictureDto = productPicture.ToDto<ProductPictureDto>();

            return Ok(productPictureDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductPictureDto model)
        {
            var productPicture = await _productService.GetProductPictureByIdAsync(model.Id);

            if (productPicture == null)
                return NotFound($"Product picture Id={model.Id} is not found");

            productPicture = model.FromDto<ProductPicture>();

            await _productService.UpdateProductPictureAsync(productPicture);

            return Ok();
        }

        #endregion
    }
}
