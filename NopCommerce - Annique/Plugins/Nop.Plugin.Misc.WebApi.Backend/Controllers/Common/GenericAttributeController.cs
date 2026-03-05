using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Common
{
    public partial class GenericAttributeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<GenericAttribute> _genericAttributerRepository;

        #endregion

        #region Ctor

        public GenericAttributeController(IGenericAttributeService genericAttributeService,
            IRepository<GenericAttribute> genericAttributerRepository)
        {
            _genericAttributeService = genericAttributeService;
            _genericAttributerRepository = genericAttributerRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes an attributes
        /// </summary>
        /// <param name="ids">Array of attributes identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        public virtual async Task<IActionResult> DeleteAttributes(string ids)
        {
            var attributesId = ids.ToIdArray();
            var categories = await _genericAttributerRepository.GetByIdsAsync(attributesId);

            await _genericAttributeService.DeleteAttributesAsync(categories);

            return Ok();
        }

        /// <summary>
        /// Get attributes
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="keyGroup">Key group</param>
        [HttpGet("{entityId}")]
        [ProducesResponseType(typeof(IList<GenericAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAttributesForEntity(int entityId, [FromQuery][Required] string keyGroup)
        {
            var attributes = await _genericAttributeService.GetAttributesForEntityAsync(entityId, keyGroup);

            return Ok(attributes.Select(a => a.ToDto<GenericAttributeDto>()).ToList());
        }

        // TODO: move logic to service
        /// <summary>
        /// Save attribute value
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 if this attribute will be available for all stores</param>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityTypeName">Entity type name</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [HttpPost("{storeId}/{entityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SaveAttribute(int storeId,
            int entityId,
            [FromQuery, Required] string entityTypeName,
            [FromQuery, Required] string key,
            [FromQuery, Required] string value)
        {
            if (entityId <= 0 || key == null)
                return BadRequest();

            var keyGroup = entityTypeName;

            var props = (await _genericAttributeService.GetAttributesForEntityAsync(entityId, keyGroup))
                .Where(x => x.StoreId == storeId)
                .ToList();
            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop != null)
            {
                if (string.IsNullOrWhiteSpace(value))
                    //delete
                    await _genericAttributeService.DeleteAttributeAsync(prop);
                else
                {
                    //update
                    prop.Value = value;
                    await _genericAttributeService.UpdateAttributeAsync(prop);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(value))
                    return Ok();

                //insert
                prop = new GenericAttribute
                {
                    EntityId = entityId,
                    Key = key,
                    KeyGroup = keyGroup,
                    Value = value,
                    StoreId = storeId
                };

                await _genericAttributeService.InsertAttributeAsync(prop);
            }

            return Ok();
        }

        // TODO: move logic to service
        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        [HttpGet("{entityId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAttribute(int entityId,
            [FromQuery, Required] string key,
            [FromQuery, Required] string entityTypeName,
            [FromQuery] int storeId = 0,
            [FromQuery] object defaultValue = null)
        {
            if (entityId <= 0)
                return BadRequest();

            var keyGroup = entityTypeName;

            var props = await _genericAttributeService.GetAttributesForEntityAsync(entityId, keyGroup);

            //little hack here (only for unit testing). we should write expect-return rules in unit tests for such cases
            if (props == null)
                return Ok(defaultValue);

            props = props.Where(x => x.StoreId == storeId).ToList();
            if (!props.Any())
                return Ok(defaultValue);

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop == null || string.IsNullOrEmpty(prop.Value))
                return Ok(defaultValue);

            return Ok(prop.Value);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var genericAttribute = await _genericAttributerRepository.GetByIdAsync(id);

            if (genericAttribute == null)
                return NotFound($"Generic attribute Id={id} not found");

            await _genericAttributeService.DeleteAttributeAsync(genericAttribute);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GenericAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var genericAttribute = await _genericAttributerRepository.GetByIdAsync(id);

            if (genericAttribute == null)
                return NotFound($"Generic attribute Id={id} not found");

            return Ok(genericAttribute.ToDto<GenericAttributeDto>());
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(GenericAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] GenericAttributeDto model)
        {
            var genericAttribute = model.FromDto<GenericAttribute>();

            await _genericAttributeService.InsertAttributeAsync(genericAttribute);

            var genericAttributeDto = genericAttribute.ToDto<GenericAttributeDto>();

            return Ok(genericAttributeDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] GenericAttributeDto model)
        {
            var genericAttribute = await _genericAttributerRepository.GetByIdAsync(model.Id);

            if (genericAttribute == null)
                return NotFound($"Generic attribute Id={model.Id} is not found");

            genericAttribute = model.FromDto<GenericAttribute>();

            await _genericAttributeService.UpdateAttributeAsync(genericAttribute);

            return Ok();
        }

        #endregion
    }
}
