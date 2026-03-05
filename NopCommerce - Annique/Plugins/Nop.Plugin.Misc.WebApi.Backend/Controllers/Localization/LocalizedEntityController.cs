using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Localization;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Localization
{
    public partial class LocalizedEntityController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;

        #endregion

        #region Ctor

        public LocalizedEntityController(ILocalizedEntityService localizedEntityService,
            IRepository<LocalizedProperty> localizedPropertyRepository)
        {
            _localizedEntityService = localizedEntityService;
            _localizedPropertyRepository = localizedPropertyRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets localized properties
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="localeKeyGroup">Locale key group</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the localized properties
        /// </returns>
        protected virtual async Task<IList<LocalizedProperty>> GetLocalizedPropertiesAsync(int entityId,
            string localeKeyGroup)
        {
            if (entityId == 0 || string.IsNullOrEmpty(localeKeyGroup))
                return new List<LocalizedProperty>();

            var query = from lp in _localizedPropertyRepository.Table
                orderby lp.Id
                where lp.EntityId == entityId &&
                      lp.LocaleKeyGroup == localeKeyGroup
                select lp;

            var props = await query.ToListAsync();

            return props;
        }

        /// <summary>
        /// Gets all cached localized properties
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the cached localized properties
        /// </returns>
        protected virtual async Task<IList<LocalizedProperty>> GetAllLocalizedPropertiesAsync()
        {
            return await _localizedPropertyRepository.GetAllAsync(query =>
            {
                return from lp in query
                    select lp;
            }, _ => default);
        }

        /// <summary>
        /// Deletes a localized property
        /// </summary>
        /// <param name="localizedProperty">Localized property</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task DeleteLocalizedPropertyAsync(LocalizedProperty localizedProperty)
        {
            await _localizedPropertyRepository.DeleteAsync(localizedProperty);
        }

        /// <summary>
        /// Inserts a localized property
        /// </summary>
        /// <param name="localizedProperty">Localized property</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InsertLocalizedPropertyAsync(LocalizedProperty localizedProperty)
        {
            await _localizedPropertyRepository.InsertAsync(localizedProperty);
        }

        /// <summary>
        /// Updates the localized property
        /// </summary>
        /// <param name="localizedProperty">Localized property</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateLocalizedPropertyAsync(LocalizedProperty localizedProperty)
        {
            await _localizedPropertyRepository.UpdateAsync(localizedProperty);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find localized value
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="localeKeyGroup">Locale key group</param>
        /// <param name="localeKey">Locale key</param>
        [HttpGet("{languageId}/{entityId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLocalizedValue(int languageId,
            int entityId,
            [FromQuery, Required] string localeKeyGroup,
            [FromQuery, Required] string localeKey)
        {
            var localizedValue = await _localizedEntityService.GetLocalizedValueAsync(languageId, entityId,
                localeKeyGroup, localeKey);

            return Ok(localizedValue);
        }

        // TODO: move logic to service
        /// <summary>
        /// Save localized value
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="localeKeyGroup">Locale key group</param>
        /// <param name="localeKey">Locale key</param>
        /// <param name="localeValue">Locale value</param>
        [HttpPost("{languageId}/{entityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SaveLocalizedValue(int languageId,
            int entityId,
            [FromQuery, Required] string localeKeyGroup,
            [FromQuery, Required] string localeKey,
            [FromBody] string localeValue)
        {
            if (languageId <= 0)
                return BadRequest();

            var props = await GetLocalizedPropertiesAsync(entityId, localeKeyGroup);
            var prop = props.FirstOrDefault(lp => lp.LanguageId == languageId &&
                                                  lp.LocaleKey.Equals(localeKey,
                                                      StringComparison
                                                          .InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop != null)
            {
                if (string.IsNullOrWhiteSpace(localeValue))
                    //delete
                    await DeleteLocalizedPropertyAsync(prop);
                else
                {
                    //update
                    prop.LocaleValue = localeValue;
                    await UpdateLocalizedPropertyAsync(prop);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(localeValue))
                    return Ok();

                //insert
                prop = new LocalizedProperty
                {
                    EntityId = entityId,
                    LanguageId = languageId,
                    LocaleKey = localeKey,
                    LocaleKeyGroup = localeKeyGroup,
                    LocaleValue = localeValue
                };

                await InsertLocalizedPropertyAsync(prop);
            }

            return Ok();
        }

        /// <summary>
        /// Find localized properties
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="localeKeyGroup">Locale key group</param>
        /// <param name="localeKey">Locale key</param>
        [HttpGet("{languageId}/{entityId}")]
        [ProducesResponseType(typeof(IList<LocalizedPropertyDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetEntityLocalizedPropertiesAsync(int entityId, string localeKeyGroup, string localeKey)
        {
            var properties = await _localizedEntityService.GetEntityLocalizedPropertiesAsync(entityId, localeKeyGroup, localeKey);
            var dto = properties.Select(p => p.ToDto<LocalizedPropertyDto>()).ToList();

            return Ok(dto);
        }

        #endregion
    }
}
