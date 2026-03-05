using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class CopyProductController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICopyProductService _copyProductService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public CopyProductController(ICopyProductService copyProductService,
            IProductService productService)
        {
            _copyProductService = copyProductService;
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a copy of product with all depended data
        /// </summary>
        /// <param name="productId">The product id to copy</param>
        /// <param name="newName">The name of product duplicate</param>
        /// <param name="isPublished">A value indicating whether the product duplicate should be published</param>
        /// <param name="copyMultimedia">A value indicating whether the product images and videos should be copied</param>
        /// <param name="copyAssociatedProducts">A value indicating whether the copy associated products</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CopyProduct(int productId,
            [FromQuery, Required] string newName,
            [FromQuery] bool isPublished = true,
            [FromQuery] bool copyMultimedia = true,
            [FromQuery] bool copyAssociatedProducts = true)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var newProduct = await _copyProductService.CopyProductAsync(product, newName, isPublished,
                copyMultimedia, copyAssociatedProducts);

            return Ok(newProduct.ToDto<ProductDto>());
        }

        #endregion
    }
}
