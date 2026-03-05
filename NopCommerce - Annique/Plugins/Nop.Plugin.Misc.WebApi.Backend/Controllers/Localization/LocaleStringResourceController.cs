using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Localization;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Localization
{
    public partial class LocaleStringResourceController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public LocaleStringResourceController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a locale string resource
        /// </summary>
        /// <param name="id">Locale string resource identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var localeStringResource = await _localizationService.GetLocaleStringResourceByIdAsync(id);

            if (localeStringResource == null)
                return NotFound($"Locale string resource Id={id} not found");

            await _localizationService.DeleteLocaleStringResourceAsync(localeStringResource);

            return Ok();
        }

        /// <summary>
        /// Gets a locale string resource by identifier
        /// </summary>
        /// <param name="id">The locale string resource identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LocaleStringResourceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var localeStringResource = await _localizationService.GetLocaleStringResourceByIdAsync(id);

            if (localeStringResource == null)
            {
                return NotFound($"Locale string resource Id={id} not found");
            }

            return Ok(localeStringResource.ToDto<LocaleStringResourceDto>());
        }

        /// <summary>
        /// Gets a locale string resource by name
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LocaleStringResourceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByName([FromQuery][Required] string resourceName, int languageId, [FromQuery] bool logIfNotFound = true)
        {
            if (languageId <= 0 || string.IsNullOrEmpty(resourceName))
                return BadRequest();

            var localeStringResource = await _localizationService.GetLocaleStringResourceByNameAsync(resourceName, languageId, logIfNotFound);

            if (localeStringResource == null)
            {
                return NotFound($"Locale string resource Name={resourceName} not found");
            }

            return Ok(localeStringResource.ToDto<LocaleStringResourceDto>());
        }

        /// <summary>
        /// Create a locale string resource
        /// </summary>
        /// <param name="model">Locale string resource Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(LocaleStringResourceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] LocaleStringResourceDto model)
        {
            var localeStringResource = model.FromDto<LocaleStringResource>();

            await _localizationService.InsertLocaleStringResourceAsync(localeStringResource);

            return Ok(localeStringResource.ToDto<LocaleStringResourceDto>());
        }

        /// <summary>
        /// Update the locale string resource
        /// </summary>
        /// <param name="model">Locale string resource Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] LocaleStringResourceDto model)
        {
            var localeStringResource = await _localizationService.GetLocaleStringResourceByIdAsync(model.Id);

            if (localeStringResource == null)
                return NotFound("Locale string resource is not found");

            localeStringResource = model.FromDto<LocaleStringResource>();
            await _localizationService.UpdateLocaleStringResourceAsync(localeStringResource);

            return Ok();
        }

        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <param name="loadPublicLocales">A value indicating whether to load data for the public store only (if "false", then for admin area only. If null, then load all locales. We use it for performance optimization of the site startup</param>
        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(Dictionary<string, KeyValuePair<int, string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAllResourceValues(int languageId, [FromQuery] bool? loadPublicLocales)
        {
            if (languageId <= 0)
                return BadRequest();

            var localeStringResource = await _localizationService.GetAllResourceValuesAsync(languageId, loadPublicLocales);

            return Ok(localeStringResource);
        }

        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="returnEmptyIfNotFound">A value indicating whether an empty string will be returned if a resource is not found and default value is set to empty string</param>
        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetResource([FromQuery, Required] string resourceKey,
            int languageId,
            [FromQuery] bool logIfNotFound = true,
            [FromQuery] string defaultValue = "",
            [FromQuery] bool returnEmptyIfNotFound = false)
        {
            var localeStringResource = await _localizationService.GetResourceAsync(resourceKey, languageId,
                logIfNotFound, defaultValue, returnEmptyIfNotFound);

            return Ok(localeStringResource);
        }

        /// <summary>
        /// Add a locale resource (if new) or update an existing one
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="resourceValue">Resource value</param>
        /// <param name="languageCulture">Language culture code. If null or empty, then a resource will be added for all languages</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AddOrUpdateLocaleResource([FromQuery, Required] string resourceName,
            [FromQuery, Required] string resourceValue,
            [FromQuery] string languageCulture = null)
        {
            if (string.IsNullOrEmpty(resourceName))
                return BadRequest();

            await _localizationService.AddOrUpdateLocaleResourceAsync(resourceName, resourceValue, languageCulture);

            return Ok();
        }

        /// <summary>
        /// Delete a locale resource by name (for all languages)
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> DeleteLocaleResource([FromQuery][Required] string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                return BadRequest();

            await _localizationService.DeleteLocaleResourceAsync(resourceName);

            return Ok();
        }

        /// <summary>
        /// Delete locale resources
        /// </summary>
        /// <param name="resourceNames">Resource names (Separator - ;)</param>
        /// <param name="languageId">Language identifier; pass null to delete the passed resources from all languages</param>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> DeleteLocaleResources([FromQuery][Required] string resourceNames, [FromQuery] int? languageId = null)
        {
            if (string.IsNullOrEmpty(resourceNames))
                return BadRequest();

            var names = resourceNames.Split(";").ToList();

            await _localizationService.DeleteLocaleResourcesAsync(names);

            return Ok();
        }

        /// <summary>
        /// Delete locale resources by the passed name prefix
        /// </summary>
        /// <param name="resourceNamePrefix">Resource name prefix</param>
        /// <param name="languageId">Language identifier; pass null to delete resources by prefix from all languages</param>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> DeleteLocaleResourcesByPrefix([FromQuery][Required] string resourceNamePrefix, [FromQuery] int? languageId = null)
        {
            if (string.IsNullOrEmpty(resourceNamePrefix))
                return BadRequest();

            await _localizationService.DeleteLocaleResourcesAsync(resourceNamePrefix, languageId);

            return Ok();
        }

        #endregion
    }
}
