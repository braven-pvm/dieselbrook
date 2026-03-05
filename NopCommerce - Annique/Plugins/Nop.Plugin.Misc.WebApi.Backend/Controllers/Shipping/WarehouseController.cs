using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Stores;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Shipping
{
    public partial class WarehouseController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IShippingService _shippingService;

        #endregion

        #region Ctor

        public WarehouseController(IAddressService addressService,
            IShippingService shippingService)
        {
            _addressService = addressService;
            _shippingService = shippingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all warehouses
        /// </summary>
        /// <param name="name">Warehouse name</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<WarehouseDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery]string name = null)
        {
            var warehouses = await _shippingService.GetAllWarehousesAsync(name);

            var warehousesDto = warehouses.Select(warehouse => warehouse.ToDto<WarehouseDto>()).ToList();

            return Ok(warehousesDto);
        }

        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="id">The warehouse identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var warehouse = await _shippingService.GetWarehouseByIdAsync(id);

            if (warehouse == null)
            {
                return NotFound($"Warehouse Id={id} not found");
            }

            return Ok(warehouse.ToDto<WarehouseDto>());
        }

        /// <summary>
        /// Create a warehouse
        /// </summary>
        /// <param name="model">Warehouse Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] WarehouseDto model)
        {
            var warehouse = model.FromDto<Warehouse>();

            await _shippingService.InsertWarehouseAsync(warehouse);

            return Ok(warehouse.ToDto<WarehouseDto>());
        }

        /// <summary>
        /// Update a warehouse
        /// </summary>
        /// <param name="model">Warehouse Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] WarehouseDto model)
        {
            var warehouse = await _shippingService.GetWarehouseByIdAsync(model.Id);

            if (warehouse == null)
                return NotFound("Warehouse is not found");

            warehouse = model.FromDto<Warehouse>();
            await _shippingService.UpdateWarehouseAsync(warehouse);

            return Ok();
        }

        /// <summary>
        /// Delete a warehouse
        /// </summary>
        /// <param name="id">Warehouse identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var warehouse = await _shippingService.GetWarehouseByIdAsync(id);

            if (warehouse == null)
                return NotFound($"Warehouse Id={id} not found");

            await _shippingService.DeleteWarehouseAsync(warehouse);

            return Ok();
        }

        /// <summary>
        /// Get the nearest warehouse for the specified address
        /// </summary>
        /// <param name="addressId">Address</param>
        /// <param name="warehousesIds">List of warehouses, if null all warehouses are used. (separator - ;)</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetNearestWarehouse(int addressId, [FromQuery] string warehousesIds = null)
        {
            if (addressId <= 0)
                return BadRequest();

            var address = await _addressService.GetAddressByIdAsync(addressId);
            if (address == null)
                return NotFound($"Address Id={addressId} not found");

            var warehouses = await warehousesIds.ToIdArray()
                .SelectAwait(async id => await _shippingService.GetWarehouseByIdAsync(id)).ToListAsync();

            var warehouse = await _shippingService.GetNearestWarehouseAsync(address, warehouses);
            if (warehouse == null)
                return NotFound("Warehouse not found");

            return Ok(warehouse.ToDto<WarehouseDto>());
        }

        #endregion
    }
}
