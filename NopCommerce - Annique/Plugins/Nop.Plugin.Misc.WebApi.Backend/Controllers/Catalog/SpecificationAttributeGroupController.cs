using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class SpecificationAttributeGroupController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ISpecificationAttributeService _specificationAttributeService;

        #endregion

        #region Ctor

        public SpecificationAttributeGroupController(ISpecificationAttributeService specificationAttributeService)
        {
            _specificationAttributeService = specificationAttributeService;
        }

        #endregion

        #region Methods
        
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var specificationAttributeGroup = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(id);

            if (specificationAttributeGroup == null)
                return NotFound($"Specification attribute group Id={id} not found");

            await _specificationAttributeService.DeleteSpecificationAttributeGroupAsync(specificationAttributeGroup);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SpecificationAttributeGroupDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var specificationAttributeGroup = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(id);

            if (specificationAttributeGroup == null)
                return NotFound($"Specification attribute group Id={id} not found");

            return Ok(specificationAttributeGroup.ToDto<SpecificationAttributeGroupDto>());
        }

        /// <summary>
        /// Gets specification attribute groups
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<SpecificationAttributeGroup, SpecificationAttributeGroupDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = int.MaxValue)
        {
            var specificationAttributeGroups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync(pageIndex, pageSize);

            return Ok(specificationAttributeGroups.ToPagedListDto<SpecificationAttributeGroup, SpecificationAttributeGroupDto>());
        }

        /// <summary>
        /// Gets product specification attribute groups
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<SpecificationAttributeGroupDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductSpecificationAttributeGroups(int productId)
        {
            var specificationAttributeGroups = await _specificationAttributeService.GetProductSpecificationAttributeGroupsAsync(productId);
            var specificationAttributeGroupsDto =
                specificationAttributeGroups.Select(s => s.ToDto<SpecificationAttributeGroupDto>());

            return Ok(specificationAttributeGroupsDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SpecificationAttributeGroupDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] SpecificationAttributeGroupDto model)
        {
            var specificationAttributeGroup = model.FromDto<SpecificationAttributeGroup>();

            await _specificationAttributeService.InsertSpecificationAttributeGroupAsync(specificationAttributeGroup);

            var specificationAttributeGroupDto = specificationAttributeGroup.ToDto<SpecificationAttributeGroupDto>();

            return Ok(specificationAttributeGroupDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] SpecificationAttributeGroupDto model)
        {
            var specificationAttributeGroup = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(model.Id);

            if (specificationAttributeGroup == null)
                return NotFound($"Specification attribute group Id={model.Id} is not found");

            specificationAttributeGroup = model.FromDto<SpecificationAttributeGroup>();

            await _specificationAttributeService.UpdateSpecificationAttributeGroupAsync(specificationAttributeGroup);

            return Ok();
        }

        #endregion
    }
}
