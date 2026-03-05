using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Directory;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;
using Nop.Services.Directory;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Directory
{
    public partial class CountryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public CountryController(IAddressService addressService,
            ICountryService countryService)
        {
            _addressService = addressService;
            _countryService = countryService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all countries
        /// </summary>
        /// <param name="languageId">Language identifier. It's used to sort countries by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CountryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllCountries([FromQuery] int languageId = 0, [FromQuery] bool showHidden = false)
        {
            var countries = await _countryService.GetAllCountriesAsync(languageId, showHidden);
            var countriesDto = countries.Select(c => c.ToDto<CountryDto>()).ToList();

            return Ok(countriesDto);
        }

        /// <summary>
        /// Gets all countries that allow billing
        /// </summary>
        /// <param name="languageId">Language identifier. It's used to sort countries by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CountryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllCountriesForBilling([FromQuery] int languageId = 0, [FromQuery] bool showHidden = false)
        {
            var countries = await _countryService.GetAllCountriesForBillingAsync(languageId, showHidden);
            var countriesDto = countries.Select(c => c.ToDto<CountryDto>()).ToList();

            return Ok(countriesDto);
        }

        /// <summary>
        /// Gets all countries that allow shipping
        /// </summary>
        /// <param name="languageId">Language identifier. It's used to sort countries by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CountryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllCountriesForShipping([FromQuery] int languageId = 0, [FromQuery] bool showHidden = false)
        {
            var countries = await _countryService.GetAllCountriesForShippingAsync(languageId, showHidden);
            var countriesDto = countries.Select(c => c.ToDto<CountryDto>()).ToList();

            return Ok(countriesDto);
        }

        /// <summary>
        /// Gets a country by address 
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetCountryByAddress(int addressId)
        {
            if (addressId <= 0)
                return BadRequest();

            var address = await _addressService.GetAddressByIdAsync(addressId);

            if (address == null)
                return NotFound($"Address Id={addressId} not found");

            var country = await _countryService.GetCountryByAddressAsync(address);

            return Ok(country.ToDto<CountryDto>());
        }

        /// <summary>
        /// Get countries by identifiers
        /// </summary>
        /// <param name="ids">Array of country identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<CountryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCountriesByIds(string ids)
        {
            var countriesId = ids.ToIdArray();
            var countries = await _countryService.GetCountriesByIdsAsync(countriesId);

            return Ok(countries.Select(c => c.ToDto<CountryDto>()).ToList());
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var country = await _countryService.GetCountryByIdAsync(id);

            if (country == null)
                return NotFound($"Country Id={id} not found");

            await _countryService.DeleteCountryAsync(country);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var country = await _countryService.GetCountryByIdAsync(id);

            if (country == null)
                return NotFound($"Country Id={id} not found");

            return Ok(country.ToDto<CountryDto>());
        }

        /// <summary>
        /// Gets a country by two letter ISO code
        /// </summary>
        /// <param name="twoLetterIsoCode">Country two letter ISO code</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCountryByTwoLetterIsoCode([FromQuery][Required] string twoLetterIsoCode)
        {
            if (string.IsNullOrEmpty(twoLetterIsoCode))
                return BadRequest();

            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(twoLetterIsoCode);

            if (country == null)
                return NotFound($"Country two letter ISO code={twoLetterIsoCode} not found");

            return Ok(country.ToDto<CountryDto>());
        }

        /// <summary>
        /// Gets a country by three letter ISO code
        /// </summary>
        /// <param name="threeLetterIsoCode">Country three letter ISO code</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCountryByThreeLetterIsoCode([FromQuery][Required] string threeLetterIsoCode)
        {
            if (string.IsNullOrEmpty(threeLetterIsoCode))
                return BadRequest();

            var country = await _countryService.GetCountryByThreeLetterIsoCodeAsync(threeLetterIsoCode);

            if (country == null)
                return NotFound($"Country three letter ISO code={threeLetterIsoCode} not found");

            return Ok(country.ToDto<CountryDto>());
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CountryDto model)
        {
            var country = model.FromDto<Country>();

            await _countryService.InsertCountryAsync(country);

            var countryDto = country.ToDto<CountryDto>();

            return Ok(countryDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CountryDto model)
        {
            var country = await _countryService.GetCountryByIdAsync(model.Id);

            if (country == null)
                return NotFound($"Country Id={model.Id} is not found");

            country = model.FromDto<Country>();

            await _countryService.UpdateCountryAsync(country);

            return Ok();
        }

        #endregion
    }
}
