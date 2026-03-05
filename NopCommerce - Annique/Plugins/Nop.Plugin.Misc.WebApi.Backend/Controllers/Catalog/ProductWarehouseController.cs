using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ProductWarehouseController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;

        #endregion

        #region Ctor

        public ProductWarehouseController(IProductService productService,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository)
        {
            _productService = productService;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
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

            var productWarehouseInventory = await _productWarehouseInventoryRepository.GetByIdAsync(id);

            if (productWarehouseInventory == null)
                return NotFound($"Product warehouse inventory Id={id} not found");

            await _productService.DeleteProductWarehouseInventoryAsync(productWarehouseInventory);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductWarehouseInventoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productWarehouseInventory = await _productWarehouseInventoryRepository.GetByIdAsync(id);

            if (productWarehouseInventory == null)
                return NotFound($"Product warehouse inventory Id={id} not found");

            return Ok(productWarehouseInventory.ToDto<ProductWarehouseInventoryDto>());
        }
        
        /// <summary>
        /// Get a product warehouse-inventory records by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<ProductWarehouseInventoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllProductWarehouseInventoryRecords(int productId)
        {
            var productWarehouseInventories = await _productService.GetAllProductWarehouseInventoryRecordsAsync(productId);
            var productWarehouseInventoriesDto =
                productWarehouseInventories.Select(p => p.ToDto<ProductWarehouseInventoryDto>());

            return Ok(productWarehouseInventoriesDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductWarehouseInventoryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductWarehouseInventoryDto model)
        {
            var productWarehouseInventory = model.FromDto<ProductWarehouseInventory>();

            await _productService.InsertProductWarehouseInventoryAsync(productWarehouseInventory);

            var productWarehouseInventoryDto = productWarehouseInventory.ToDto<ProductWarehouseInventoryDto>();

            return Ok(productWarehouseInventoryDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductWarehouseInventoryDto model)
        {
            var productWarehouseInventory = await _productWarehouseInventoryRepository.GetByIdAsync(model.Id);

            if (productWarehouseInventory == null)
                return NotFound($"Product warehouse inventory Id={model.Id} is not found");

            productWarehouseInventory = model.FromDto<ProductWarehouseInventory>();

            await _productService.UpdateProductWarehouseInventoryAsync(productWarehouseInventory);

            return Ok();
        }

        #endregion
    }
}
