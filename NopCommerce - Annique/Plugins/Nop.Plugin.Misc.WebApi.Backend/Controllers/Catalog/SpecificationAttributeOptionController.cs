using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class SpecificationAttributeOptionController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ISpecificationAttributeService _specificationAttributeService;

        #endregion

        #region Ctor

        public SpecificationAttributeOptionController(ISpecificationAttributeService specificationAttributeService)
        {
            _specificationAttributeService = specificationAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get specification attribute options by identifiers
        /// </summary>
        /// <param name="ids">Identifiers</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetSpecificationAttributeOptionsByIds(string ids)
        {
            var optionsId = ids.ToIdArray();
            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(optionsId);

            var optionsDto = options.Select(c => c.ToDto<CategoryDto>());

            return Ok(optionsDto);
        }

        /// <summary>
        /// Returns a list of IDs of not existing specification attribute options
        /// </summary>
        /// <param name="ids">The IDs of the attribute options to check</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNotExistingSpecificationAttributeOptions(string ids)
        {
            var optionsIds = ids.ToIdArray();
            var notExisting = await _specificationAttributeService.GetNotExistingSpecificationAttributeOptionsAsync(optionsIds);

            return Ok(notExisting.ToList());
        }

        /// <summary>
        /// Gets a specification attribute option by specification attribute id
        /// </summary>
        /// <param name="specificationAttributeId">The specification attribute identifier</param>
        [HttpGet("{specificationAttributeId}")]
        [ProducesResponseType(typeof(IList<SpecificationAttributeOptionDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetSpecificationAttributeOptionsBySpecificationAttribute(int specificationAttributeId)
        {
            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specificationAttributeId);

            var optionsDto = options.Select(c => c.ToDto<SpecificationAttributeOptionDto>());

            return Ok(optionsDto);
        }

        /// <summary>
        /// Gets the filtrable specification attribute options by category id
        /// </summary>
        /// <param name="categoryId">The category id</param>
        [HttpGet("{categoryId}")]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetFiltrableSpecificationAttributeOptionsByCategoryId(int categoryId)
        {
            var options = await _specificationAttributeService.GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(categoryId);

            var optionsDto = options.Select(c => c.ToDto<CategoryDto>());

            return Ok(optionsDto);
        }

        /// <summary>
        /// Gets the filtrable specification attribute options by manufacturer id
        /// </summary>
        /// <param name="manufacturerId">The manufacturer id</param>
       [HttpGet("{manufacturerId}")]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetFiltrableSpecificationAttributeOptionsByManufacturerId(int manufacturerId)
        {
            var options = await _specificationAttributeService.GetFiltrableSpecificationAttributeOptionsByManufacturerIdAsync(manufacturerId);

            var optionsDto = options.Select(c => c.ToDto<CategoryDto>());

            return Ok(optionsDto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(id);

            if (specificationAttributeOption == null)
                return NotFound($"Specification attribute option Id={id} not found");

            await _specificationAttributeService.DeleteSpecificationAttributeOptionAsync(specificationAttributeOption);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SpecificationAttributeOptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(id);

            if (specificationAttributeOption == null)
                return NotFound($"Specification attribute option Id={id} not found");

            return Ok(specificationAttributeOption.ToDto<SpecificationAttributeOptionDto>());
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(SpecificationAttributeOptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] SpecificationAttributeOptionDto model)
        {
            var specificationAttributeOption = model.FromDto<SpecificationAttributeOption>();

            await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specificationAttributeOption);

            var specificationAttributeOptionDto = specificationAttributeOption.ToDto<SpecificationAttributeOptionDto>();

            return Ok(specificationAttributeOptionDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] SpecificationAttributeOptionDto model)
        {
            var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(model.Id);

            if (specificationAttributeOption == null)
                return NotFound($"Specification attribute option Id={model.Id} is not found");

            specificationAttributeOption = model.FromDto<SpecificationAttributeOption>();

            await _specificationAttributeService.UpdateSpecificationAttributeOptionAsync(specificationAttributeOption);

            return Ok();
        }

        #endregion
    }
}
