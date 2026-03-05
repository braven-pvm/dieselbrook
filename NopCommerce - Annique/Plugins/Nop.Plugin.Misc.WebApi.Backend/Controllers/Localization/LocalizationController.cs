using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Localization
{
    public partial class LocalizationController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public LocalizationController(ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IWorkContext workContext)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        // TODO: move logic to service
        /// <summary>
        /// Get localized value of enum
        /// </summary>
        /// <param name="enumeTypeName">Enume type name</param>
        /// <param name="enumValue">Enum value</param>
        /// <param name="languageId">Language identifier; pass null to use the current working language</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLocalizedEnum([FromQuery, Required] string enumeTypeName,
            [FromQuery, Required] string enumValue,
            [FromQuery] int? languageId = null)
        {
            var tEnum = Type.GetType(enumeTypeName);

            if (!tEnum?.IsEnum ?? true)
                return BadRequest();

            //localized value
            var workingLanguage = await _workContext.GetWorkingLanguageAsync();
            var resourceName =
                $"{NopLocalizationDefaults.EnumLocaleStringResourcesPrefix}{Type.GetType(enumeTypeName)}.{enumValue}";
            var result = await _localizationService.GetResourceAsync(resourceName, languageId ?? workingLanguage.Id,
                false, string.Empty, true);

            //set default value if required
            if (string.IsNullOrEmpty(result))
                result = CommonHelper.ConvertEnum(enumValue);

            return Ok(result);
        }

        // TODO: move logic to service
        /// <summary>
        /// Get localized friendly name of a plugin
        /// </summary>
        /// <param name="pluginSystemName">Plugin</param>
        /// <param name="pluginFriendlyName">Plugin</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLocalizedFriendlyName(
            [FromQuery, Required] string pluginSystemName,
            [FromQuery, Required] string pluginFriendlyName,
            int languageId,
            [FromQuery] bool returnDefaultValue = true)
        {
            //localized value
            var resourceName = $"{NopLocalizationDefaults.PluginNameLocaleStringResourcesPrefix}{pluginSystemName}";
            var result =
                await _localizationService.GetResourceAsync(resourceName, languageId, false, string.Empty, true);

            //set default value if required
            if (string.IsNullOrEmpty(result) && returnDefaultValue)
                result = pluginFriendlyName;

            return Ok(result);
        }

        // TODO: move logic to service
        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="localeKeyGroup">Locale key group</param>
        /// <param name="localeKey">Locale key</param>
        /// <param name="languageId">Language identifier; pass null to use the current working language; pass 0 to get standard language value</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        [HttpGet("{entityId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLocalized(int entityId,
            [FromQuery, Required] string localeKeyGroup,
            [FromQuery, Required] string localeKey,
            [FromQuery] int? languageId = null,
            [FromQuery] bool ensureTwoPublishedLanguages = true)


        {
            if (entityId <= 0)
                return BadRequest();

            var workingLanguage = await _workContext.GetWorkingLanguageAsync();

            var langId = languageId ?? workingLanguage.Id;
            var localized = string.Empty;

            if (langId > 0)
            {
                //ensure that we have at least two published languages
                var loadLocalizedValue = true;
                if (ensureTwoPublishedLanguages)
                {
                    var totalPublishedLanguages = (await _languageService.GetAllLanguagesAsync()).Count;
                    loadLocalizedValue = totalPublishedLanguages >= 2;
                }

                //localized value
                if (loadLocalizedValue)
                    localized = await _localizedEntityService
                        .GetLocalizedValueAsync(langId, entityId, localeKeyGroup, localeKey);
            }

            return Ok(localized);
        }

        #endregion
    }
}
