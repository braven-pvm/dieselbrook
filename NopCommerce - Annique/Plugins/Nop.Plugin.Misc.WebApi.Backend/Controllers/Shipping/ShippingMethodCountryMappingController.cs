using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Shipping
{
    public partial class ShippingMethodCountryMappingController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IShippingService _shippingService;

        #endregion

        #region Ctor

        public ShippingMethodCountryMappingController(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets shipping country mappings
        /// </summary>
        /// <param name="model">Shipping country mapping Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(IList<ShippingMethodCountryMappingDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Get([FromBody] ShippingMethodCountryMappingDto model)
        {
            var shippingMethodCountryMappings = await _shippingService.GetShippingMethodCountryMappingAsync(model.ShippingMethodId, model.CountryId);

            var shippingMethodCountryMappingsDto = shippingMethodCountryMappings.Select(shippingMethodCountryMapping => shippingMethodCountryMapping.ToDto<ShippingMethodCountryMappingDto>()).ToList();

            return Ok(shippingMethodCountryMappingsDto);
        }

        /// <summary>
        /// Create a shipping country mapping
        /// </summary>
        /// <param name="model">Shipping country mapping Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ShippingMethodCountryMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ShippingMethodCountryMappingDto model)
        {
            var shippingMethodCountryMapping = model.FromDto<ShippingMethodCountryMapping>();

            await _shippingService.InsertShippingMethodCountryMappingAsync(shippingMethodCountryMapping);

            return Ok(shippingMethodCountryMapping.ToDto<ShippingMethodCountryMappingDto>());
        }

        /// <summary>
        /// Delete the shipping country mapping
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <param name="countryId">Country identifier</param>
        [HttpDelete("{shippingMethodId}/{countryId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int shippingMethodId, int countryId)
        {
            if (shippingMethodId <= 0 || countryId <= 0)
                return BadRequest();

            var shippingMethodCountryMapping = await _shippingService.GetShippingMethodCountryMappingAsync(shippingMethodId, countryId);

            if (shippingMethodCountryMapping == null)
                return NotFound($"Shipping country mapping not found");

            await _shippingService.DeleteShippingMethodCountryMappingAsync(shippingMethodCountryMapping.FirstOrDefault());

            return Ok();
        }

        #endregion
    }
}
