using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Directory;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Directory
{
    public partial class GeoLookupController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IGeoLookupService _geoLookupService;

        #endregion

        #region Ctor

        public GeoLookupController(IGeoLookupService geoLookupService)
        {
            _geoLookupService = geoLookupService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get country ISO code
        /// </summary>
        /// <param name="ipAddress">IP address</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult LookupCountryIsoCode([FromQuery][Required] string ipAddress)
        {
            return Ok(_geoLookupService.LookupCountryIsoCode(ipAddress));
        }

        /// <summary>
        /// Get country name
        /// </summary>
        /// <param name="ipAddress">IP address</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult LookupCountryName([FromQuery][Required] string ipAddress)
        {
            return Ok(_geoLookupService.LookupCountryName(ipAddress));
        }

        #endregion
    }
}
