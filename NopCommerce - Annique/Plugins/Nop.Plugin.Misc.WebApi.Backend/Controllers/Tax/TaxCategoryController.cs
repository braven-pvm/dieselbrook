using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Tax;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Tax;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Tax
{
    public partial class TaxCategoryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ITaxCategoryService _taxCategoryService;

        #endregion

        #region Ctor

        public TaxCategoryController(ITaxCategoryService taxCategoryService)
        {
            _taxCategoryService = taxCategoryService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all tax categories
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<TaxCategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var taxCategories = await _taxCategoryService.GetAllTaxCategoriesAsync();

            var taxCategoriesDto = taxCategories.Select(taxCategory => taxCategory.ToDto<TaxCategoryDto>()).ToList();

            return Ok(taxCategoriesDto);
        }

        /// <summary>
        /// Gets a tax category
        /// </summary>
        /// <param name="id">Tax category identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TaxCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var taxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(id);

            if (taxCategory == null)
            {
                return NotFound($"Tax category Id={id} not found");
            }

            var taxCategoryDto = taxCategory.ToDto<TaxCategoryDto>();

            return Ok(taxCategoryDto);
        }

        /// <summary>
        /// Create a tax category
        /// </summary>
        /// <param name="model">Tax category Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(TaxCategoryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] TaxCategoryDto model)
        {
            var taxCategory = model.FromDto<TaxCategory>();

            await _taxCategoryService.InsertTaxCategoryAsync(taxCategory);

            var taxCategoryDto = taxCategory.ToDto<TaxCategoryDto>();

            return Ok(taxCategoryDto);
        }

        /// <summary>
        /// Update a tax category
        /// </summary>
        /// <param name="model">Tax category Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] TaxCategoryDto model)
        {
            var taxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(model.Id);

            if (taxCategory == null)
                return NotFound("Tax category is not found");

            taxCategory = model.FromDto<TaxCategory>();
            await _taxCategoryService.UpdateTaxCategoryAsync(taxCategory);

            return Ok();
        }

        /// <summary>
        /// Delete a tax category
        /// </summary>
        /// <param name="id">Tax category identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var taxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(id);

            if (taxCategory == null)
                return NotFound($"Tax category Id={id} not found");

            await _taxCategoryService.DeleteTaxCategoryAsync(taxCategory);

            return Ok();
        }

        #endregion
    }
}
