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

    public partial class ProductVideosController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public ProductVideosController(IProductService productService)
        {
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a product videos by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<ProductVideoDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductVideosByProductId(int productId)
        {
            var videos = await _productService.GetProductVideosByProductIdAsync(productId);
            var videosDto = videos.Select(pp => MappingExtensions.ToDto<ProductVideoDto>(pp));

            return Ok(videosDto);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productVideo = await _productService.GetProductVideoByIdAsync(id);

            if (productVideo == null)
                return NotFound($"Product video Id={id} not found");

            await _productService.DeleteProductVideoAsync(productVideo);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductVideoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productVideo = await _productService.GetProductVideoByIdAsync(id);

            if (productVideo == null)
                return NotFound($"Product video Id={id} not found");

            return Ok(productVideo.ToDto<ProductVideoDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductVideoDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductVideoDto model)
        {
            var productVideo = model.FromDto<ProductVideo>();

            await _productService.InsertProductVideoAsync(productVideo);

            var productVideoDto = productVideo.ToDto<ProductVideoDto>();

            return Ok(productVideoDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductVideoDto model)
        {
            var productVideo = await _productService.GetProductVideoByIdAsync(model.Id);

            if (productVideo == null)
                return NotFound($"Product video Id={model.Id} is not found");

            productVideo = model.FromDto<ProductVideo>();

            await _productService.UpdateProductVideoAsync(productVideo);

            return Ok();
        }

        #endregion
    }
}