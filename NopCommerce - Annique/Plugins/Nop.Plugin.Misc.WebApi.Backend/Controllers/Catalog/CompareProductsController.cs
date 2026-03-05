using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class CompareProductsController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICompareProductsService _compareProductsService;

        #endregion

        #region Ctor

        public CompareProductsController(ICompareProductsService compareProductsService)
        {
            _compareProductsService = compareProductsService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears a "compare products" list
        /// </summary>
        [HttpGet]
        public IActionResult ClearCompareProducts()
        {
            _compareProductsService.ClearCompareProducts();

            return Ok();
        }

        /// <summary>
        /// Gets a "compare products" list
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetComparedProducts()
        {
            var products = await _compareProductsService.GetComparedProductsAsync();
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Removes a product from a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> RemoveProductFromCompareList(int productId)
        {
            if (productId <= 0)
                return BadRequest();

            await _compareProductsService.RemoveProductFromCompareListAsync(productId);

            return Ok();
        }

        /// <summary>
        /// Adds a product to a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AddProductToCompareList(int productId)
        {
            if (productId <= 0)
                return BadRequest();

            await _compareProductsService.AddProductToCompareListAsync(productId);

            return Ok();
        }

        #endregion
    }
}
