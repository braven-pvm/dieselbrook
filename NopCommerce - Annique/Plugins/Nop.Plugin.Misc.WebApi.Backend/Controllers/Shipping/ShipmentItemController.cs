using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Shipping
{
    public partial class ShipmentItemController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;

        #endregion

        #region Ctor

        public ShipmentItemController(IProductService productService, 
            IShipmentService shipmentService)
        {
            _productService = productService;
            _shipmentService = shipmentService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a shipment item
        /// </summary>
        /// <param name="id">Shipment item identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ShipmentItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var shipmentItem = await _shipmentService.GetShipmentItemByIdAsync(id);

            if (shipmentItem == null)
            {
                return NotFound($"Shipment item Id={id} not found");
            }

            return Ok(shipmentItem.ToDto<ShipmentItemDto>());
        }

        /// <summary>
        /// Gets a shipment items of shipment
        /// </summary>
        /// <param name="id">Shipment identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IList<ShipmentItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetByShipmentId(int id)
        {
            if (id <= 0)
                return BadRequest();

            var shipmentItems = await _shipmentService.GetShipmentItemsByShipmentIdAsync(id);
            var shipmentItemsDto = shipmentItems.Select(shipmentItem => shipmentItem.ToDto<ShipmentItemDto>()).ToList();

            return Ok(shipmentItemsDto);
        }

        /// <summary>
        /// Create a shipment item
        /// </summary>
        /// <param name="model">Shipment item Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ShipmentItemDto model)
        {
            var shipmentItem = model.FromDto<ShipmentItem>();

            await _shipmentService.InsertShipmentItemAsync(shipmentItem);

            return Ok(shipmentItem.ToDto<ShipmentItemDto>());
        }

        /// <summary>
        /// Update a shipment item
        /// </summary>
        /// <param name="model">Shipment item Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ShipmentItemDto model)
        {
            var shipmentItem = await _shipmentService.GetShipmentItemByIdAsync(model.Id);

            if (shipmentItem == null)
                return NotFound("Shipment item is not found");

            shipmentItem = model.FromDto<ShipmentItem>();
            await _shipmentService.UpdateShipmentItemAsync(shipmentItem);

            return Ok();
        }

        /// <summary>
        /// Delete a shipment item
        /// </summary>
        /// <param name="id">Shipment item identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var shipmentItem = await _shipmentService.GetShipmentItemByIdAsync(id);

            if (shipmentItem == null)
                return NotFound($"Shipment item Id={id} not found");

            await _shipmentService.DeleteShipmentItemAsync(shipmentItem);

            return Ok();
        }

        /// <summary>
        /// Get quantity in shipments. For example, get planned quantity to be shipped
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="ignoreShipped">Ignore already shipped shipments</param>
        /// <param name="ignoreDelivered">Ignore already delivered shipments</param>
        [HttpGet("{productId}/{warehouseId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetQuantityInShipments(int productId,
            int warehouseId,
            [FromQuery, Required] bool ignoreShipped,
            [FromQuery, Required] bool ignoreDelivered)
        {
            if (productId <= 0 || warehouseId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var result =
                await _shipmentService.GetQuantityInShipmentsAsync(product, warehouseId, ignoreShipped,
                    ignoreDelivered);

            return Ok(result);
        }

        #endregion
    }
}
