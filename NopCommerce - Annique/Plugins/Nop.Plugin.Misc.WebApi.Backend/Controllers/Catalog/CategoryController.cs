using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class CategoryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IRepository<DiscountCategoryMapping> _discountCategoryMappingRepository;

        #endregion

        #region Ctor

        public CategoryController(ICategoryService categoryService,
            IRepository<DiscountCategoryMapping> discountCategoryMappingRepository)
        {
            _categoryService = categoryService;
            _discountCategoryMappingRepository = discountCategoryMappingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all categories filtered by parent category identifier
        /// </summary>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{parentCategoryId}")]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllCategoriesByParentCategoryId(int parentCategoryId,
            [FromQuery] bool showHidden = false)
        {
            var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentCategoryId, showHidden);

            var categoriesDto = categories.Select(c => c.ToDto<CategoryDto>());

            return Ok(categoriesDto);
        }
        
        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllCategoriesDisplayedOnHomepage([FromQuery] bool showHidden = false)
        {
            var categories = await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync(showHidden);

            var categoriesDto = categories.Select(c => c.ToDto<CategoryDto>());

            return Ok(categoriesDto);
        }

        /// <summary>
        /// Gets child category identifiers
        /// </summary>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{parentCategoryId}")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetChildCategoryIds(int parentCategoryId, 
            [FromQuery] int storeId = 0, [FromQuery] bool showHidden = false)
        {
            var rez = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId, storeId, showHidden);

            return Ok(rez.ToList());
        }

        /// <summary>
        /// Get categories for which a discount is applied
        /// </summary>
        /// <param name="discountId">Discount identifier; pass null to load all records</param>
        /// <param name="showHidden">A value indicating whether to load deleted categories</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Category, CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategoriesByAppliedDiscount([FromQuery] int? discountId = null,
            [FromQuery] bool showHidden = false,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var categories = await _categoryService.GetCategoriesByAppliedDiscountAsync(discountId,
                showHidden, pageIndex, pageSize);

            return Ok(categories.ToPagedListDto<Category, CategoryDto>());
        }

        /// <summary>
        /// Delete a list of categories
        /// </summary>
        /// <param name="ids">Array of category identifiers (separator - ;)</param>
        [HttpDelete("{ids}")]
        public virtual async Task<IActionResult> DeleteCategories(string ids)
        {
            var categoriesId = ids.ToIdArray();
            var categories = await _categoryService.GetCategoriesByIdsAsync(categoriesId);

            await _categoryService.DeleteCategoriesAsync(categories);

            return Ok();
        }

        /// <summary>
        /// Returns a list of names of not existing categories
        /// </summary>
        /// <param name="idsNames">Array of names and/or IDs of the categories to check (separator - ;)</param>
        [HttpGet("{idsNames}")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNotExistingCategories(string idsNames)
        {
            var categoryIdsNames = idsNames.Split(";").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var notExisting = await _categoryService.GetNotExistingCategoriesAsync(categoryIdsNames);

            return Ok(notExisting.ToList());
        }

        /// <summary>
        /// Gets categories by identifier
        /// </summary>
        /// <param name="ids">Array of category identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategoriesByIds(string ids)
        {
            var categoriesId = ids.ToIdArray();
            var categories = await _categoryService.GetCategoriesByIdsAsync(categoriesId);

            var categoriesDto = categories.Select(c => c.ToDto<CategoryDto>());

            return Ok(categoriesDto);
        }

        /// <summary>
        /// Get formatted category breadcrumb 
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        [HttpGet("{categoryId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetFormattedBreadCrumb(int categoryId,
            [FromQuery] string separator = ">>", [FromQuery] int languageId = 0)
        {
            if (categoryId <= 0)
                return BadRequest();

            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (category == null)
                return NotFound($"Category Id={categoryId} not found");

            var breadCrumb = await _categoryService.GetFormattedBreadCrumbAsync(category, null, separator,
                languageId);

            return Ok(breadCrumb);
        }

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="id">Category id</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IList<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetCategoryBreadCrumb(int id, [FromQuery] bool showHidden = false)
        {
            if (id <= 0)
                return BadRequest();

            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound($"Category Id={id} not found");

            var categories = await _categoryService.GetCategoryBreadCrumbAsync(category, null, showHidden);
            var categoriesDto = categories.Select(c => c.ToDto<CategoryDto>());

            return Ok(categoriesDto);
        }

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="id">Category</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound($"Category Id={id} not found");

            await _categoryService.DeleteCategoryAsync(category);

            return Ok();
        }

        /// <summary>
        /// Gets a category
        /// </summary>
        /// <param name="id">Category identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound($"Category Id={id} not found");

            return Ok(category.ToDto<CategoryDto>());
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Category, CategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] string categoryName = null,
            [FromQuery] int storeId = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false,
            [FromQuery] bool? overridePublished = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(categoryName, storeId,
                pageIndex, pageSize, showHidden, overridePublished);

            return Ok(categories.ToPagedListDto<Category, CategoryDto>());
        }

        /// <summary>
        /// Inserts category
        /// </summary>
        /// <param name="model">Category</param>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CategoryDto model)
        {
            var category = model.FromDto<Category>();

            await _categoryService.InsertCategoryAsync(category);

            var categoryDto = category.ToDto<CategoryDto>();

            return Ok(categoryDto);
        }

        /// <summary>
        /// Updates the category
        /// </summary>
        /// <param name="model">Category</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CategoryDto model)
        {
            var category = await _categoryService.GetCategoryByIdAsync(model.Id);

            if (category == null)
                return NotFound($"Category Id={model.Id} is not found");

            category = model.FromDto<Category>();

            await _categoryService.UpdateCategoryAsync(category);

            return Ok();
        }

        /// <summary>
        /// Inserts a discount-category mapping record
        /// </summary>
        /// <param name="model">Discount-category mapping</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> InsertDiscountCategoryMapping([FromBody] DiscountCategoryMappingDto model)
        {
            await _categoryService.InsertDiscountCategoryMappingAsync(model.FromDto<DiscountCategoryMapping>());

            return Ok();
        }

        /// <summary>
        /// Deletes a discount-category mapping record
        /// </summary>
        /// <param name="id">Discount-category mapping</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> DeleteDiscountCategoryMapping(int id)
        {
            if (id <= 0)
                return BadRequest();

            var mapping = await _discountCategoryMappingRepository.GetByIdAsync(id);

            if (mapping == null)
                return NotFound($"Mapping Id={id} not found");

            await _categoryService.DeleteDiscountCategoryMappingAsync(mapping);

            return Ok();
        }

        /// <summary>
        /// Get a discount-category mapping record
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="discountId">Discount identifier</param>
        [HttpGet("{categoryId}/{discountId}")]
        [ProducesResponseType(typeof(DiscountCategoryMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetDiscountAppliedToCategory(int categoryId, int discountId)
        {
            var mapping = await _categoryService.GetDiscountAppliedToCategoryAsync(categoryId, discountId);

            return Ok(mapping.ToDto<DiscountCategoryMappingDto>());
        }

        #endregion
    }
}
