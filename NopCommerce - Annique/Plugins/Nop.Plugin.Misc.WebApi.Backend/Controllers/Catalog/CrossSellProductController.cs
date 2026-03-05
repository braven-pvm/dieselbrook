using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class CrossSellProductController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemrRepository;

        #endregion

        #region Ctor

        public CrossSellProductController(IProductService productService,
            IRepository<ShoppingCartItem> shoppingCartItemrRepository)
        {
            _productService = productService;
            _shoppingCartItemrRepository = shoppingCartItemrRepository;
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

            var crossSellProduct = await _productService.GetCrossSellProductByIdAsync(id);

            if (crossSellProduct == null)
                return NotFound($"Cross sell product Id={id} not found");

            await _productService.DeleteCrossSellProductAsync(crossSellProduct);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CrossSellProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var crossSellProduct = await _productService.GetCrossSellProductByIdAsync(id);

            if (crossSellProduct == null)
                return NotFound($"Cross sell product Id={id} not found");

            return Ok(crossSellProduct.ToDto<CrossSellProductDto>());
        }

        /// <summary>
        /// Gets cross-sell products by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{productId1}")]
        [ProducesResponseType(typeof(IList<CrossSellProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCrossSellProductsByProductId1(int productId1, [FromQuery] bool showHidden = false)
        {
            var crossSellProducts = await _productService.GetCrossSellProductsByProductId1Async(productId1, showHidden);
            var crossSellProductsDto = crossSellProducts.Select(csp => csp.ToDto<CrossSellProductDto>());

            return Ok(crossSellProductsDto);
        }

        /// <summary>
        /// Gets a cross-sells
        /// </summary>
        /// <param name="cart">Array of Shopping cart item identifiers (separator - ;)</param>
        /// <param name="numberOfProducts">Number of products to return</param>
        [HttpGet("{cart}")]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCrossSellProductsByShoppingCart(string cart, [FromQuery][Required] int numberOfProducts)
        {
            var ids = cart.ToIdArray();

            var items = await _shoppingCartItemrRepository.GetByIdsAsync(ids);

            var products = await _productService.GetCrossSellProductsByShoppingCartAsync(items, numberOfProducts);
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CrossSellProductDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CrossSellProductDto model)
        {
            var crossSellProduct = model.FromDto<CrossSellProduct>();

            await _productService.InsertCrossSellProductAsync(crossSellProduct);

            var crossSellProductDto = crossSellProduct.ToDto<CrossSellProductDto>();

            return Ok(crossSellProductDto);
        }
        
        #endregion
    }
}
