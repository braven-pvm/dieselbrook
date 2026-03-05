using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class RecentlyViewedProductsController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;

        #endregion

        #region Ctor

        public RecentlyViewedProductsController(IRecentlyViewedProductsService recentlyViewedProductsService)
        {
            _recentlyViewedProductsService = recentlyViewedProductsService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a "recently viewed products" list
        /// </summary>
        /// <param name="number">Number of products to load</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRecentlyViewedProducts([FromQuery][Required] int number)
        {
            var products = await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(number);
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Adds a product to a recently viewed products list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        public virtual async Task<IActionResult> AddProductToRecentlyViewedList(int productId)
        {
            await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(productId);

            return Ok();
        }

        #endregion
    }
}
