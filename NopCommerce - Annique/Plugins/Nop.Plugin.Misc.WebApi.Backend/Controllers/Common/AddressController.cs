using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Common
{
    public partial class AddressController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IAddressService _addressService;

        #endregion

        #region Ctor

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets total number of addresses by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        [HttpGet("{countryId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAddressTotalByCountryId(int countryId)
        {
            var total = await _addressService.GetAddressTotalByCountryIdAsync(countryId);

            return Ok(total);
        }

        /// <summary>
        /// Gets total number of addresses by state/province identifier
        /// </summary>
        /// <param name="stateProvinceId">State/province identifier</param>
        [HttpGet("{stateProvinceId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAddressTotalByStateProvinceId(int stateProvinceId)
        {
            var total = await _addressService.GetAddressTotalByStateProvinceIdAsync(stateProvinceId);

            return Ok(total);
        }

        /// <summary>
        /// Gets a value indicating whether address is valid (can be saved)
        /// </summary>
        /// <param name="address">Address to validate</param>
        [HttpPost]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsAddressValid([FromBody] AddressDto address)
        {
            var isValid = await _addressService.IsAddressValidAsync(address.FromDto<Address>());

            return Ok(isValid);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var address = await _addressService.GetAddressByIdAsync(id);

            if (address == null)
                return NotFound($"Address Id={id} not found");

            await _addressService.DeleteAddressAsync(address);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var address = await _addressService.GetAddressByIdAsync(id);

            if (address == null)
                return NotFound($"Address Id={id} not found");

            return Ok(address.ToDto<AddressDto>());
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] AddressDto model)
        {
            var address = model.FromDto<Address>();

            await _addressService.InsertAddressAsync(address);

            var addressDto = address.ToDto<AddressDto>();

            return Ok(addressDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] AddressDto model)
        {
            var address = await _addressService.GetAddressByIdAsync(model.Id);

            if (address == null)
                return NotFound($"Address Id={model.Id} is not found");

            address = model.FromDto<Address>();

            await _addressService.UpdateAddressAsync(address);

            return Ok();
        }

        #endregion
    }
}
