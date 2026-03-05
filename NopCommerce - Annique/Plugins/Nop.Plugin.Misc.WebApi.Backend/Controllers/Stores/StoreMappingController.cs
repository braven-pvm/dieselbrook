using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Stores;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Stores
{
    public partial class StoreMappingController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public StoreMappingController(CatalogSettings catalogSettings,
            IStoreMappingService storeMappingService,
            IRepository<StoreMapping> storeMappingRepository,
            IStaticCacheManager staticCacheManager)
        {
            _catalogSettings = catalogSettings;
            _storeMappingService = storeMappingService;
            _storeMappingRepository = storeMappingRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        protected async Task<IList<int>> GetIdsWithAccess(int entityId, string entityTypeName)
        {
            var entityName = entityTypeName;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopStoreDefaults.StoreMappingIdsCacheKey, entityId, entityName);

            var query = from sm in _storeMappingRepository.Table
                where sm.EntityId == entityId &&
                      sm.EntityName == entityName
                select sm.StoreId;

            return await _staticCacheManager.GetAsync(key, () => query.ToArray());
        }

        #endregion

        #region Methods

        // TODO: move logic to service
        /// <summary>
        /// Find store identifiers with granted access (mapped to the entity)
        /// </summary>
        [HttpGet("{entityId}")]
        [ProducesResponseType(typeof(IList<int>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStoresIdsWithAccess(int entityId, [FromQuery][Required] string entityTypeName)
        {
            return Ok(await GetIdsWithAccess(entityId, entityTypeName));
        }

        // TODO: move logic to service
        [HttpPost("{storeId}/{entityId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Authorize(int storeId,
            int entityId,
            [FromQuery, Required] string entityTypeName,
            [FromQuery, Required] bool subjectToAcl)
        {
            if (entityId <= 0 || storeId <= 0)
                return BadRequest();

            if (_catalogSettings.IgnoreStoreLimitations)
                return Ok(true);

            if (!subjectToAcl)
                return Ok(true);

            foreach (var storeIdWithAccess in await GetIdsWithAccess(entityId, entityTypeName))
                if (storeId == storeIdWithAccess)
                    //yes, we have such permission
                    return Ok(true);

            //no permission found
            return Ok(false);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var storeMapping = await _storeMappingRepository.GetByIdAsync(id);

            if (storeMapping == null)
                return NotFound($"Store mapping Id={id} not found");

            await _storeMappingService.DeleteStoreMappingAsync(storeMapping);

            return Ok();
        }

        /// <summary>
        /// Gets store mapping records
        /// </summary>
        [HttpGet("{entityId}")]
        [ProducesResponseType(typeof(IList<StoreMappingDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStoreMappings(int entityId, [FromQuery][Required] string entityTypeName)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopStoreDefaults.StoreMappingsCacheKey, entityId, entityTypeName);

            var query = from sm in _storeMappingRepository.Table
                where sm.EntityId == entityId &&
                      sm.EntityName == entityTypeName
                        select sm;

            var storeMappings = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());
            var storeMappingsDto = storeMappings.Select(s => s.ToDto<StoreMappingDto>());

            return Ok(storeMappingsDto);
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(StoreMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] StoreMappingDto model)
        {
            var storeMapping = model.FromDto<StoreMapping>();

            await _storeMappingRepository.InsertAsync(storeMapping);

            var storeMappingDto = storeMapping.ToDto<StoreMappingDto>();

            return Ok(storeMappingDto);
        }
        
        #endregion
    }
}
