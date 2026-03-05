using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Country;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class CountryController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly ICountryModelFactory _countryModelFactory;

        #endregion

        #region Ctor

        public CountryController(ICountryModelFactory countryModelFactory)
        {
            _countryModelFactory = countryModelFactory;
        }

        #endregion

        #region States / provinces

        [HttpGet("{countryId}")]
        [ProducesResponseType(typeof(IList<StateProvinceModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStatesByCountryId(int countryId, [FromQuery][Required] bool addSelectStateItem)
        {
            //TODO: We need to change the parameters of the GetStatesByCountryIdAsync method to pass the int value
            var model = await _countryModelFactory.GetStatesByCountryIdAsync(countryId.ToString(), addSelectStateItem);
            var modelDto = model.Select(p => p.ToDto<StateProvinceModelDto>()).ToList();

            return Ok(modelDto);
        }

        #endregion
    }
}