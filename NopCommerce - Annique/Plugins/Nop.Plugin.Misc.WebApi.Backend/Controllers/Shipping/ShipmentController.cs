using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Shipping
{
    public partial class ShipmentController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IShipmentService _shipmentService;

        #endregion

        #region Ctor

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all shipments
        /// </summary>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier, only shipments with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="shippingCountryId">Shipping country identifier; 0 to load all records</param>
        /// <param name="shippingStateId">Shipping state identifier; 0 to load all records</param>
        /// <param name="shippingCounty">Shipping county; null to load all records</param>
        /// <param name="shippingCity">Shipping city; null to load all records</param>
        /// <param name="trackingNumber">Search by tracking number</param>
        /// <param name="loadNotShipped">A value indicating whether we should load only not shipped shipments</param>
        /// <param name="loadNotReadyForPickup">A value indicating whether we should load only not ready for pickup shipments</param>
        /// <param name="loadNotDelivered">A value indicating whether we should load only not delivered shipments</param>
        /// <param name="orderId">Order identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Shipment, ShipmentDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int vendorId = 0,
            [FromQuery] int warehouseId = 0,
            [FromQuery] int shippingCountryId = 0,
            [FromQuery] int shippingStateId = 0,
            [FromQuery] string shippingCounty = null,
            [FromQuery] string shippingCity = null,
            [FromQuery] string trackingNumber = null,
            [FromQuery] bool loadNotShipped = false,
            [FromQuery] bool loadNotReadyForPickup = false,
            [FromQuery] bool loadNotDelivered = false,
            [FromQuery] int orderId = 0,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var shipments = await _shipmentService.GetAllShipmentsAsync(vendorId, warehouseId, shippingCountryId,
                shippingStateId, shippingCounty, shippingCity, trackingNumber, loadNotShipped, loadNotDelivered,
                loadNotReadyForPickup, orderId, createdFromUtc, createdToUtc, pageIndex, pageSize);

            var shipmentsDto = shipments.ToPagedListDto<Shipment, ShipmentDto>();

            return Ok(shipmentsDto);
        }

        /// <summary>
        /// Gets a shipment
        /// </summary>
        /// <param name="id">Shipment identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var shipment = await _shipmentService.GetShipmentByIdAsync(id);

            if (shipment == null)
            {
                return NotFound($"Shipment Id={id} not found");
            }

            return Ok(shipment.ToDto<ShipmentDto>());
        }

        /// <summary>
        /// Gets a shipment by identifiers
        /// </summary>
        /// <param name="ids">Array of shipment identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<ShipmentDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByIds(string ids)
        {
            var shipmentIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var shipments = await _shipmentService.GetShipmentsByIdsAsync(shipmentIds);

            var shipmentsDto = shipments.Select(shipment => shipment.ToDto<ShipmentDto>()).ToList();

            return Ok(shipmentsDto);
        }

        /// <summary>
        /// Gets a list of order shipments
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <param name="shipped">A value indicating whether to count only shipped or not shipped shipments; pass null to ignore</param>
        /// <param name="readyForPickup">A value indicating whether to load only ready for pickup shipments; pass null to ignore</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(IList<ShipmentDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByOrderId(int orderId,
            [FromQuery] bool? shipped = null,
            [FromQuery] bool? readyForPickup = null,
            [FromQuery] int vendorId = 0)
        {
            var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(orderId, shipped, readyForPickup, vendorId);

            var shipmentsDto = shipments.Select(shipment => shipment.ToDto<ShipmentDto>()).ToList();

            return Ok(shipmentsDto);
        }

        /// <summary>
        /// Create a shipment
        /// </summary>
        /// <param name="model">Shipment Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ShipmentDto model)
        {
            var shipment = model.FromDto<Shipment>();

            await _shipmentService.InsertShipmentAsync(shipment);

            return Ok(shipment.ToDto<ShipmentDto>());
        }

        /// <summary>
        /// Update a shipment
        /// </summary>
        /// <param name="model">Shipment Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ShipmentDto model)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.Id);

            if (shipment == null)
                return NotFound("Shipment is not found");

            shipment = model.FromDto<Shipment>();
            await _shipmentService.UpdateShipmentAsync(shipment);

            return Ok();
        }

        /// <summary>
        /// Delete a shipment
        /// </summary>
        /// <param name="id">Shipment identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var shipment = await _shipmentService.GetShipmentByIdAsync(id);

            if (shipment == null)
                return NotFound($"Shipment Id={id} not found");

            await _shipmentService.DeleteShipmentAsync(shipment);

            return Ok();
        }

        #endregion
    }
}
