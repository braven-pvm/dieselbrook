using System.Collections.Generic;
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
    public partial class ProductManufacturerController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IManufacturerService _manufacturerService;

        #endregion

        #region Ctor

        public ProductManufacturerController(IManufacturerService manufacturerService)
        {
            _manufacturerService = manufacturerService;
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

            var productManufacturer = await _manufacturerService.GetProductManufacturerByIdAsync(id);

            if (productManufacturer == null)
                return NotFound($"Product manufacturer Id={id} not found");

            await _manufacturerService.DeleteProductManufacturerAsync(productManufacturer);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductManufacturerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productManufacturer = await _manufacturerService.GetProductManufacturerByIdAsync(id);

            if (productManufacturer == null)
                return NotFound($"Product manufacturer Id={id} not found");

            return Ok(productManufacturer.ToDto<ProductManufacturerDto>());
        }

        /// <summary>
        /// Gets a product manufacturer mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<ProductManufacturerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductManufacturersByProductId(int productId, [FromQuery] bool showHidden = false)
        {
            var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(productId, showHidden);
            var productManufacturersDto = productManufacturers.Select(pc => pc.ToDto<ProductCategoryDto>());

            return Ok(productManufacturersDto);
        }

        /// <summary>
        /// Get manufacturer IDs for products
        /// </summary>
        /// <param name="ids">Array of Products Id (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(Dictionary<int, int[]>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductManufacturerIds(string ids)
        {
            var productIdsNames = ids.ToIdArray();
            var productManufacturer = await _manufacturerService.GetProductManufacturerIdsAsync(productIdsNames);

            return Ok(productManufacturer);
        }

        /// <summary>
        /// Gets product manufacturer collection
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{manufacturerId}")]
        [ProducesResponseType(typeof(PagedListDto<ProductManufacturer, ProductManufacturerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductManufacturersByManufacturerId(int manufacturerId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false)
        {
            var productManufacturers =
                await _manufacturerService.GetProductManufacturersByManufacturerIdAsync(manufacturerId, pageIndex,
                    pageSize, showHidden);

            return Ok(productManufacturers.ToPagedListDto<ProductManufacturer, ProductManufacturerDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductManufacturerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductManufacturerDto model)
        {
            var productManufacturer = model.FromDto<ProductManufacturer>();

            await _manufacturerService.InsertProductManufacturerAsync(productManufacturer);

            var productManufacturerDto = productManufacturer.ToDto<ProductManufacturerDto>();

            return Ok(productManufacturerDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductManufacturerDto model)
        {
            var productManufacturer = await _manufacturerService.GetProductManufacturerByIdAsync(model.Id);

            if (productManufacturer == null)
                return NotFound($"Product manufacturer Id={model.Id} is not found");

            productManufacturer = model.FromDto<ProductManufacturer>();

            await _manufacturerService.UpdateProductManufacturerAsync(productManufacturer);

            return Ok();
        }

        #endregion
    }
}
