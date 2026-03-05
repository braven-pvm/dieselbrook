using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Seo;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Seo;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Seo
{
    public partial class UrlRecordController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public UrlRecordController(IRepository<UrlRecord> urlRecordRepository,
            IUrlRecordService urlRecordService)
        {
            _urlRecordRepository = urlRecordRepository;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all URL records
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <param name="languageId">Language ID; "null" to load records with any language; "0" to load records with standard language only; otherwise to load records with specify language ID only</param>
        /// <param name="isActive">A value indicating whether to get active records; "null" to load all records; "false" to load only inactive records; "true" to load only active records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<UrlRecord, UrlRecordDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] string slug = "",
            [FromQuery] int? languageId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var urlRecords =
                await _urlRecordService.GetAllUrlRecordsAsync(slug, languageId, isActive, pageIndex, pageSize);
            var urlRecordsDto = urlRecords.ToPagedListDto<UrlRecord, UrlRecordDto>();

            return Ok(urlRecordsDto);
        }

        /// <summary>
        /// Gets an URL records by identifiers
        /// </summary>
        /// <param name="ids">Array of URL record identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<UrlRecordDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByIds(string ids)
        {
            var urlRecordIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var urlRecords = await _urlRecordService.GetUrlRecordsByIdsAsync(urlRecordIds);

            var urlRecordsDto = urlRecords.Select(urlRecord => urlRecord.ToDto<UrlRecordDto>()).ToList();

            return Ok(urlRecordsDto);
        }

        /// <summary>
        /// Gets a URL record by slug
        /// </summary>
        /// <param name="slug">Slug</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UrlRecordDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBySlug([FromQuery][Required] string slug)
        {
            var decodedSlug = HttpUtility.UrlDecode(slug);
            var urlRecord = await _urlRecordService.GetBySlugAsync(decodedSlug);

            if (urlRecord == null)
            {
                return NotFound($"URL record by slug={slug} not found");
            }

            return Ok(urlRecord.ToDto<UrlRecordDto>());
        }

        /// <summary>
        /// Create a URL record
        /// </summary>
        /// <param name="model">URL record Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(UrlRecordDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] UrlRecordDto model)
        {
            var urlRecord = model.FromDto<UrlRecord>();

            await _urlRecordService.InsertUrlRecordAsync(urlRecord);

            return Ok(urlRecord.ToDto<UrlRecordDto>());
        }

        /// <summary>
        /// Delete a URL records
        /// </summary>
        /// <param name="ids">Array of URL record identifiers (separator - ;)</param>
        [HttpDelete("{ids}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Delete(string ids)
        {
            var urlRecordIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var urlRecords = await _urlRecordService.GetUrlRecordsByIdsAsync(urlRecordIds);

            if (urlRecords == null)
                return NotFound($"URL records not found");

            await _urlRecordService.DeleteUrlRecordsAsync(urlRecords);

            return Ok();
        }

        /// <summary>
        /// Updates the URL record
        /// </summary>
        /// <param name="model">URL record Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] UrlRecordDto model)
        {
            var urlRecord = (await _urlRecordService.GetUrlRecordsByIdsAsync(new int[] { model.Id })).FirstOrDefault();

            if (urlRecord == null)
                return NotFound("URL record is not found");

            urlRecord = model.FromDto<UrlRecord>();
            await _urlRecordService.UpdateUrlRecordAsync(urlRecord);

            return Ok();
        }

        /// <summary>
        /// Find slug
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="languageId">Language identifier</param>
        [HttpGet("{entityId}/{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetActiveSlug(int entityId, [FromQuery][Required] string entityName, int languageId)
        {
            var slug = await _urlRecordService.GetActiveSlugAsync(entityId, entityName, languageId);

            if (string.IsNullOrEmpty(slug))
                return NotFound($"Slug is not found");

            return Ok(slug);
        }

        /// <summary>
        /// Get search engine friendly name (slug)
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="languageId">Language identifier; pass null to use the current language</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if language specified one is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        [HttpGet("{entityId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetSeName(int entityId,
            [FromQuery, Required] string entityName,
            [FromQuery] int? languageId = null,
            [FromQuery] bool returnDefaultValue = true,
            [FromQuery] bool ensureTwoPublishedLanguages = true)
        {
            var slug = await _urlRecordService.GetSeNameAsync(entityId, entityName, languageId, returnDefaultValue,
                ensureTwoPublishedLanguages);

            if (string.IsNullOrEmpty(slug))
                return NotFound("Slug is not found");

            return Ok(slug);
        }

        /// <summary>
        /// Get SE name
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="convertNonWesternChars">A value indicating whether non western chars should be converted</param>
        /// <param name="allowUnicodeCharsInUrls">A value indicating whether Unicode chars are allowed</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetSeName([FromQuery, Required] string name,
            [FromQuery, Required] bool convertNonWesternChars,
            [FromQuery, Required] bool allowUnicodeCharsInUrls)
        {
            var seName = await _urlRecordService.GetSeNameAsync(name, convertNonWesternChars, allowUnicodeCharsInUrls);

            if (string.IsNullOrEmpty(seName))
                return NotFound($"SE name is not found");

            return Ok(seName);
        }

        /// <summary>
        /// Validate search engine name
        /// </summary>
        /// <param name="entityId">Entity</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="seName">Search engine name to validate</param>
        /// <param name="name">User-friendly name used to generate sename</param>
        /// <param name="ensureNotEmpty">Ensure that sename is not empty</param>
        [HttpPost("entityId")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ValidateSeName(int entityId,
            [FromQuery, Required] string entityName,
            [FromQuery, Required] string seName,
            [FromQuery, Required] string name,
            [FromQuery, Required] bool ensureNotEmpty)
        {
            var result = await _urlRecordService.ValidateSeNameAsync(entityId, entityName,
                seName, name, ensureNotEmpty);

            return Ok(result);
        }

        // TODO: move logic to service
        /// <summary>
        /// Save slug
        /// </summary>
        [HttpPost("{languageId}/{entityId}")]
        public virtual async Task<IActionResult> SaveSlug(int languageId,
            int entityId,
            [FromQuery, Required] string entityTypeName,
            [FromQuery, Required] string slug)
        {
            var query = from ur in _urlRecordRepository.Table
                where ur.EntityId == entityId &&
                      ur.EntityName == entityTypeName &&
                      ur.LanguageId == languageId
                orderby ur.Id descending
                select ur;
            var allUrlRecords = await query.ToListAsync();
            var activeUrlRecord = allUrlRecords.FirstOrDefault(x => x.IsActive);
            UrlRecord nonActiveRecordWithSpecifiedSlug;

            if (activeUrlRecord == null && !string.IsNullOrWhiteSpace(slug))
            {
                //find in non-active records with the specified slug
                nonActiveRecordWithSpecifiedSlug = allUrlRecords
                    .FirstOrDefault(
                        x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                if (nonActiveRecordWithSpecifiedSlug != null)
                {
                    //mark non-active record as active
                    nonActiveRecordWithSpecifiedSlug.IsActive = true;
                    await _urlRecordRepository.UpdateAsync(nonActiveRecordWithSpecifiedSlug);
                }
                else
                {
                    //new record
                    var urlRecord = new UrlRecord
                    {
                        EntityId = entityId,
                        EntityName = entityTypeName,
                        Slug = slug,
                        LanguageId = languageId,
                        IsActive = true
                    };
                    await _urlRecordService.InsertUrlRecordAsync(urlRecord);
                }
            }

            if (activeUrlRecord != null && string.IsNullOrWhiteSpace(slug))
            {
                //disable the previous active URL record
                activeUrlRecord.IsActive = false;
                await _urlRecordRepository.UpdateAsync(activeUrlRecord);
            }

            if (activeUrlRecord == null || string.IsNullOrWhiteSpace(slug))
                return Ok();

            //it should not be the same slug as in active URL record
            if (activeUrlRecord.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase))
                return Ok();

            //find in non-active records with the specified slug
            nonActiveRecordWithSpecifiedSlug = allUrlRecords
                .FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
            if (nonActiveRecordWithSpecifiedSlug != null)
            {
                //mark non-active record as active
                nonActiveRecordWithSpecifiedSlug.IsActive = true;
                await _urlRecordRepository.UpdateAsync(nonActiveRecordWithSpecifiedSlug);

                //disable the previous active URL record
                activeUrlRecord.IsActive = false;
                await _urlRecordRepository.UpdateAsync(activeUrlRecord);
            }
            else
            {
                //insert new record
                //we do not update the existing record because we should track all previously entered slugs
                //to ensure that URLs will work fine
                var urlRecord = new UrlRecord
                {
                    EntityId = entityId,
                    EntityName = entityTypeName,
                    Slug = slug,
                    LanguageId = languageId,
                    IsActive = true
                };
                await _urlRecordService.InsertUrlRecordAsync(urlRecord);

                //disable the previous active URL record
                activeUrlRecord.IsActive = false;
                await _urlRecordRepository.UpdateAsync(activeUrlRecord);
            }

            return Ok();
        }

        #endregion
    }
}
