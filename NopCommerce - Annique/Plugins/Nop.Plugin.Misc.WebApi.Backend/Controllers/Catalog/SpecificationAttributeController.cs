using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class SpecificationAttributeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ISpecificationAttributeService _specificationAttributeService;

        #endregion

        #region Ctor

        public SpecificationAttributeController(ISpecificationAttributeService specificationAttributeService)
        {
            _specificationAttributeService = specificationAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets specification attributes
        /// </summary>
        /// <param name="ids">The specification attribute identifiers</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetSpecificationAttributeByIds(string ids)
        {
            var specificationAttributesId = ids.ToIdArray();
            var specificationAttributes = await _specificationAttributeService.GetSpecificationAttributeByIdsAsync(specificationAttributesId);

            var specificationAttributesDto = specificationAttributes.Select(c => c.ToDto<CategoryDto>());

            return Ok(specificationAttributesDto);
        }

        /// <summary>
        /// Gets specification attributes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<SpecificationAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllWithOptions()
        {
            var specificationAttributes = await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync();
            var specificationAttributesDto = specificationAttributes.Select(s => s.ToDto<SpecificationAttributeDto>());

            return Ok(specificationAttributesDto);
        }

        /// <summary>
        /// Gets specification attributes by group identifier
        /// </summary>
        /// <param name="groupId">The specification attribute group identifier</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<SpecificationAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllByGroupId([FromQuery] int? groupId = null)
        {
            var specificationAttributes = await _specificationAttributeService.GetSpecificationAttributesByGroupIdAsync(groupId);
            var specificationAttributesDto = specificationAttributes.Select(s => s.ToDto<SpecificationAttributeDto>());

            return Ok(specificationAttributesDto);
        }

        /// <summary>
        /// Deletes specifications attributes
        /// </summary>
        /// <param name="ids">Specification attributes id</param>
        [HttpGet("{ids}")]
        public virtual async Task<IActionResult> DeleteSpecificationAttributes(string ids)
        {
            var specificationAttributesId = ids.ToIdArray();
            var specificationAttributes = await _specificationAttributeService.GetSpecificationAttributeByIdsAsync(specificationAttributesId);

            await _specificationAttributeService.DeleteSpecificationAttributesAsync(specificationAttributes);

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(id);

            if (specificationAttribute == null)
                return NotFound($"Specification attribute Id={id} not found");

            await _specificationAttributeService.DeleteSpecificationAttributeAsync(specificationAttribute);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SpecificationAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(id);

            if (specificationAttribute == null)
                return NotFound($"Specification attribute Id={id} not found");

            return Ok(specificationAttribute.ToDto<SpecificationAttributeDto>());
        }

        /// <summary>
        /// Gets specification attributes
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<SpecificationAttribute, SpecificationAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = int.MaxValue)
        {
            var specificationAttributes = await _specificationAttributeService.GetSpecificationAttributesAsync(pageIndex, pageSize);

            return Ok(specificationAttributes.ToPagedListDto<SpecificationAttribute, SpecificationAttributeDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(SpecificationAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] SpecificationAttributeDto model)
        {
            var specificationAttribute = model.FromDto<SpecificationAttribute>();

            await _specificationAttributeService.InsertSpecificationAttributeAsync(specificationAttribute);

            var specificationAttributeDto = specificationAttribute.ToDto<SpecificationAttributeDto>();

            return Ok(specificationAttributeDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] SpecificationAttributeDto model)
        {
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(model.Id);

            if (specificationAttribute == null)
                return NotFound($"Specification attribute Id={model.Id} is not found");

            specificationAttribute = model.FromDto<SpecificationAttribute>();

            await _specificationAttributeService.UpdateSpecificationAttributeAsync(specificationAttribute);

            return Ok();
        }

        #endregion
    }
}
