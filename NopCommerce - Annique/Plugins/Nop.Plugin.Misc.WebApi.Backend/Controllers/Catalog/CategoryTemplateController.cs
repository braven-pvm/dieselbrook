using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class CategoryTemplateController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICategoryTemplateService _categoryTemplateService;

        #endregion

        #region Ctor

        public CategoryTemplateController(ICategoryTemplateService categoryTemplateService)
        {
            _categoryTemplateService = categoryTemplateService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete category template
        /// </summary>
        /// <param name="id">Category template identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var categoryTemplate = await _categoryTemplateService.GetCategoryTemplateByIdAsync(id);

            if (categoryTemplate == null)
                return NotFound($"Category template Id={id} not found");

            await _categoryTemplateService.DeleteCategoryTemplateAsync(categoryTemplate);

            return Ok();
        }

        /// <summary>
        /// Gets a category template
        /// </summary>
        /// <param name="id">Category template identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CategoryTemplateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var categoryTemplate = await _categoryTemplateService.GetCategoryTemplateByIdAsync(id);

            if (categoryTemplate == null)
                return NotFound($"Category template Id={id} not found");

            return Ok(categoryTemplate.ToDto<CategoryTemplateDto>());
        }

        /// <summary>
        /// Gets all category templates
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CategoryTemplateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var categoryTemplates = await _categoryTemplateService.GetAllCategoryTemplatesAsync();
            var categoryTemplatesDto = categoryTemplates.Select(p => p.ToDto<CategoryTemplateDto>());

            return Ok(categoryTemplatesDto);
        }

        /// <summary>
        /// Inserts category template
        /// </summary>
        /// <param name="model">Category template</param>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CategoryTemplateDto model)
        {
            var categoryTemplate = model.FromDto<CategoryTemplate>();

            await _categoryTemplateService.InsertCategoryTemplateAsync(categoryTemplate);

            var categoryTemplateDto = categoryTemplate.ToDto<CategoryTemplateDto>();

            return Ok(categoryTemplateDto);
        }

        /// <summary>
        /// Updates the category template
        /// </summary>
        /// <param name="model">Category template</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CategoryTemplateDto model)
        {
            var categoryTemplate = await _categoryTemplateService.GetCategoryTemplateByIdAsync(model.Id);

            if (categoryTemplate == null)
                return NotFound($"Category template Id={model.Id} is not found");

            categoryTemplate = model.FromDto<CategoryTemplate>();

            await _categoryTemplateService.UpdateCategoryTemplateAsync(categoryTemplate);

            return Ok();
        }
        
        #endregion
    }
}
