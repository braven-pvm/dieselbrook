using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Localization;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Localization
{
    public partial class LanguageController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public LanguageController(ILanguageService languageService,
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Export language resources to XML
        /// </summary>
        /// <param name="id">Language identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ExportResourcesToXml(int id)
        {
            if (id <= 0)
                return BadRequest();

            var language = await _languageService.GetLanguageByIdAsync(id);
            if (language == null)
                return NotFound($"Language Id={id} not found");

            var localeStringResource = await _localizationService.ExportResourcesToXmlAsync(language);

            return Ok(localeStringResource);
        }

        /// <summary>
        /// Get localized permission name
        /// </summary>
        /// <param name="permissionRecordId">Permission record identifier</param>
        /// <param name="languageId">Language identifier</param>
        [HttpGet("{permissionRecordId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLocalizedPermissionName(int permissionRecordId, [FromQuery] int? languageId = null)
        {
            if (permissionRecordId <= 0)
                return BadRequest();

            var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync()).Where(pr => pr.Id == permissionRecordId).FirstOrDefault();
            if (permissionRecord == null)
                return NotFound($"Permission record< Id={permissionRecordId} not found");

            var result = await _localizationService.GetLocalizedPermissionNameAsync(permissionRecord, languageId);

            return Ok(result);
        }

        /// <summary>
        /// Save localized name of a permission
        /// </summary>
        /// <param name="permissionRecordId">Permission record identifier</param>
        [HttpGet("{permissionRecordId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SaveLocalizedPermissionName(int permissionRecordId)
        {
            if (permissionRecordId <= 0)
                return BadRequest();

            var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync()).Where(pr => pr.Id == permissionRecordId).FirstOrDefault();
            if (permissionRecord == null)
                return NotFound($"Permission record< Id={permissionRecordId} not found");

            await _localizationService.SaveLocalizedPermissionNameAsync(permissionRecord);

            return Ok();
        }

        /// <summary>
        /// Delete a localized name of a permission
        /// </summary>
        /// <param name="permissionRecordId">Permission record identifier</param>
        [HttpDelete("{permissionRecordId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> DeleteLocalizedPermissionName(int permissionRecordId)
        {
            if (permissionRecordId <= 0)
                return BadRequest();

            var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync()).Where(pr => pr.Id == permissionRecordId).FirstOrDefault();
            if (permissionRecord == null)
                return NotFound($"Permission record< Id={permissionRecordId} not found");

            await _localizationService.DeleteLocalizedPermissionNameAsync(permissionRecord);

            return Ok();
        }

        /// <summary>
        /// Delete a language
        /// </summary>
        /// <param name="id">Language identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var language = await _languageService.GetLanguageByIdAsync(id);

            if (language == null)
                return NotFound($"Language Id={id} not found");

            await _languageService.DeleteLanguageAsync(language);

            return Ok();
        }

        /// <summary>
        /// Gets all languages
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<LanguageDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] bool showHidden = false, [FromQuery] int storeId = 0)
        {
            if (storeId < 0)
                return BadRequest();

            var languages = await _languageService.GetAllLanguagesAsync(showHidden, storeId);
            var languageDtos = languages.Select(language => language.ToDto<LanguageDto>()).ToList();

            return Ok(languageDtos);
        }

        /// <summary>
        /// Gets a language
        /// </summary>
        /// <param name="id">Language identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var language = await _languageService.GetLanguageByIdAsync(id);

            if (language == null)
            {
                return NotFound($"Language Id={id} not found");
            }

            return Ok(language.ToDto<LanguageDto>());
        }

        /// <summary>
        /// Create a language
        /// </summary>
        /// <param name="model">Language Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] LanguageDto model)
        {
            var language = model.FromDto<Language>();

            await _languageService.InsertLanguageAsync(language);

            return Ok(language.ToDto<LanguageDto>());
        }

        /// <summary>
        /// Update a language
        /// </summary>
        /// <param name="model">Language Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] LanguageDto model)
        {
            var language = await _languageService.GetLanguageByIdAsync(model.Id);

            if (language == null)
                return NotFound("Language is not found");

            language = model.FromDto<Language>();
            await _languageService.UpdateLanguageAsync(language);

            return Ok();
        }

        /// <summary>
        /// Get 2 letter ISO language code
        /// </summary>
        /// <param name="id">Language identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTwoLetterIsoLanguageName(int id)
        {
            if (id <= 0)
                return BadRequest();

            var language = await _languageService.GetLanguageByIdAsync(id);

            if (language == null)
            {
                return NotFound($"Language Id={id} not found");
            }

            return Ok(_languageService.GetTwoLetterIsoLanguageName(language));
        }

        #endregion
    }
}
