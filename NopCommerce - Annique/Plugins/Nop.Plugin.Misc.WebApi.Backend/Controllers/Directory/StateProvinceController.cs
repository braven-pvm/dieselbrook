using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Directory;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;
using Nop.Services.Directory;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Directory
{
    public partial class StateProvinceController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public StateProvinceController(IAddressService addressService,
            IStateProvinceService stateProvinceService)
        {
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a state/province by abbreviation
        /// </summary>
        /// <param name="abbreviation">The state/province abbreviation</param>
        /// <param name="countryId">Country identifier; pass null to load the state regardless of a country</param>
        [HttpGet]
        [ProducesResponseType(typeof(StateProvinceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStateProvinceByAbbreviation([FromQuery][Required] string abbreviation, [FromQuery] int? countryId = null)
        {
            var stateProvince = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(abbreviation, countryId);

            return Ok(stateProvince.ToDto<StateProvinceDto>());
        }

        /// <summary>
        /// Gets a state/province by address 
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StateProvinceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStateProvinceByAddress(int addressId)
        {
            if (addressId <= 0)
                return BadRequest();

            var address = await _addressService.GetAddressByIdAsync(addressId);

            if (address == null)
                return NotFound($"Address Id={addressId} not found");

            var stateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(address);

            return Ok(stateProvince.ToDto<StateProvinceDto>());
        }

        /// <summary>
        /// Gets a state/province collection by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <param name="languageId">Language identifier. It's used to sort states by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{countryId}")]
        [ProducesResponseType(typeof(IList<StateProvinceDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStateProvincesByCountryId(int countryId,
            [FromQuery] int languageId = 0,
            [FromQuery] bool showHidden = false)
        {
            var provinces =
                await _stateProvinceService.GetStateProvincesByCountryIdAsync(countryId, languageId, showHidden);

            var provincesDto = provinces.Select(p => p.ToDto<StateProvinceDto>()).ToList();

            return Ok(provincesDto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(id);

            if (stateProvince == null)
                return NotFound($"State province Id={id} not found");

            await _stateProvinceService.DeleteStateProvinceAsync(stateProvince);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StateProvinceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(id);

            if (stateProvince == null)
                return NotFound($"State province Id={id} not found");

            return Ok(stateProvince.ToDto<StateProvinceDto>());
        }

        /// <summary>
        /// Gets all states/provinces
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<StateProvinceDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] bool showHidden = false)
        {
            var stateProvinces = await _stateProvinceService.GetStateProvincesAsync(showHidden);

            return Ok(stateProvinces.Select(sp => sp.ToDto<StateProvinceDto>()).ToList());
        }

        [HttpPost]
        [ProducesResponseType(typeof(StateProvinceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] StateProvinceDto model)
        {
            var stateProvince = model.FromDto<StateProvince>();

            await _stateProvinceService.InsertStateProvinceAsync(stateProvince);

            var stateProvinceDto = stateProvince.ToDto<StateProvinceDto>();

            return Ok(stateProvinceDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] StateProvinceDto model)
        {
            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(model.Id);

            if (stateProvince == null)
                return NotFound($"State province Id={model.Id} is not found");

            stateProvince = model.FromDto<StateProvince>();

            await _stateProvinceService.UpdateStateProvinceAsync(stateProvince);

            return Ok();
        }

        #endregion
    }
}
