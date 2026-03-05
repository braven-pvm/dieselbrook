using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Stores;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Shipping
{
    public partial class ShippingMethodController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IShippingService _shippingService;

        #endregion

        #region Ctor

        public ShippingMethodController(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all shipping methods
        /// </summary>
        /// <param name="filterByCountryId">The country identifier to filter by</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ShippingMethodDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery]int? filterByCountryId = null)
        {
            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync(filterByCountryId);

            var shippingMethodsDto = shippingMethods.Select(shippingMethod => shippingMethod.ToDto<ShippingMethodDto>()).ToList();

            return Ok(shippingMethodsDto);
        }

        /// <summary>
        /// Gets a shipping method
        /// </summary>
        /// <param name="id">The shipping method identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ShippingMethodDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(id);

            if (shippingMethod == null)
            {
                return NotFound($"Shipping method Id={id} not found");
            }

            return Ok(shippingMethod.ToDto<StoreDto>());
        }

        /// <summary>
        /// Create a shipping method
        /// </summary>
        /// <param name="model">Shipping method Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ShippingMethodDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ShippingMethodDto model)
        {
            var shippingMethod = model.FromDto<ShippingMethod>();

            await _shippingService.InsertShippingMethodAsync(shippingMethod);

            return Ok(shippingMethod.ToDto<ShippingMethodDto>());
        }

        /// <summary>
        /// Update the shipping method
        /// </summary>
        /// <param name="model">Shipping method Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ShippingMethodDto model)
        {
            var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(model.Id);

            if (shippingMethod == null)
                return NotFound("Shipping method is not found");

            shippingMethod = model.FromDto<ShippingMethod>();
            await _shippingService.UpdateShippingMethodAsync(shippingMethod);

            return Ok();
        }

        /// <summary>
        /// Delete a shipping method
        /// </summary>
        /// <param name="id">The shipping method identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(id);

            if (shippingMethod == null)
                return NotFound($"Shipping method Id={id} not found");

            await _shippingService.DeleteShippingMethodAsync(shippingMethod);

            return Ok();
        }

        /// <summary>
        /// Does country restriction exist
        /// </summary>
        /// <param name="id">Shipping method identifier</param>
        /// <param name="countryId">Country identifier</param>
        [HttpGet("{id}/{countryId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CountryRestrictionExists(int id, int countryId)
        {
            if (id <= 0)
                return BadRequest();

            var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(id);

            if (shippingMethod == null)
            {
                return NotFound($"Shipping method Id={id} not found");
            }

            return Ok(await _shippingService.CountryRestrictionExistsAsync(shippingMethod, countryId));
        }

        #endregion
    }
}
