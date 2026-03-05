using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Stores;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Stores
{
    public partial class StoreController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all stores
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<StoreDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var stores = await _storeService.GetAllStoresAsync();

            var storesDto = stores.Select(store => store.ToDto<StoreDto>()).ToList();

            return Ok(storesDto);
        }

        /// <summary>
        /// Gets a store
        /// </summary>
        /// <param name="id">Store identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var store = await _storeService.GetStoreByIdAsync(id);

            if (store == null)
            {
                return NotFound($"Store Id={id} not found");
            }

            var storeDto = store.ToDto<StoreDto>();

            return Ok(storeDto);
        }

        /// <summary>
        /// Create a store
        /// </summary>
        /// <param name="model">Store Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] StoreDto model)
        {
            var store = model.FromDto<Store>();

            await _storeService.InsertStoreAsync(store);

            var storeDto = store.ToDto<StoreDto>();

            return Ok(storeDto);
        }

        /// <summary>
        /// Update a store
        /// </summary>
        /// <param name="model">Store Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] StoreDto model)
        {
            var store = await _storeService.GetStoreByIdAsync(model.Id);

            if (store == null)
                return NotFound("Store is not found");

            store = model.FromDto<Store>();
            await _storeService.UpdateStoreAsync(store);

            return Ok();
        }

        /// <summary>
        /// Delete a store
        /// </summary>
        /// <param name="id">Store identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var store = await _storeService.GetStoreByIdAsync(id);

            if (store == null)
                return NotFound($"Store Id={id} not found");

            await _storeService.DeleteStoreAsync(store);

            return Ok();
        }

        /// <summary>
        /// Indicates whether a store contains a specified host
        /// </summary>
        /// <param name="id">Store identifier</param>
        /// <param name="host">Host</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ContainsHostValue(int id, [FromQuery][Required] string host)
        {
            if (id <= 0)
                return BadRequest();

            var store = await _storeService.GetStoreByIdAsync(id);

            if (store == null)
            {
                return NotFound($"Store Id={id} not found");
            }

            return Ok(_storeService.ContainsHostValue(store, host));
        }

        /// <summary>
        /// Returns a list of names of not existing stores
        /// </summary>
        /// <param name="ids">The names and/or IDs of the store to check (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetNotExistingStores(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest();

            var storeIds = ids.Split(";");

            return Ok(await _storeService.GetNotExistingStoresAsync(storeIds));
        }

        #endregion
    }
}
