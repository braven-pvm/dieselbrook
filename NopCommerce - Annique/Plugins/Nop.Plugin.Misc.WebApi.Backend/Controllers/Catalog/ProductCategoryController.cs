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
    public partial class ProductCategoryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICategoryService _categoryService;

        #endregion

        #region Ctor

        public ProductCategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets product category mapping collection
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{categoryId}")]
        [ProducesResponseType(typeof(PagedListDto<ProductCategory, ProductCategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductCategoriesByCategoryId(int categoryId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false)
        {
            var productCategorys =
                await _categoryService.GetProductCategoriesByCategoryIdAsync(categoryId, pageIndex, pageSize,
                    showHidden);

            return Ok(productCategorys.ToPagedListDto<ProductCategory, ProductCategoryDto>());
        }

        /// <summary>
        /// Gets a product category mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<ProductCategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductCategoriesByProductId(int productId, [FromQuery] bool showHidden = false)
        {
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId, showHidden);
            var productCategoriesDto = productCategories.Select(pc => pc.ToDto<ProductCategoryDto>());

            return Ok(productCategoriesDto);
        }

        /// <summary>
        /// Get category IDs for products
        /// </summary>
        /// <param name="ids">Array of Products Id (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(Dictionary<int, int[]>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductCategoryIds(string ids)
        {
            var categoryIdsNames = ids.ToIdArray();
            var productCategory = await _categoryService.GetProductCategoryIdsAsync(categoryIdsNames);

            return Ok(productCategory);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productCategory = await _categoryService.GetProductCategoryByIdAsync(id);

            if (productCategory == null)
                return NotFound($"Product category Id={id} not found");

            await _categoryService.DeleteProductCategoryAsync(productCategory);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductCategoryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productCategory = await _categoryService.GetProductCategoryByIdAsync(id);

            if (productCategory == null)
                return NotFound($"Product category Id={id} not found");

            return Ok(productCategory.ToDto<ProductCategoryDto>());
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ProductCategoryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductCategoryDto model)
        {
            var productCategory = model.FromDto<ProductCategory>();

            await _categoryService.InsertProductCategoryAsync(productCategory);

            var productCategoryDto = productCategory.ToDto<ProductCategoryDto>();

            return Ok(productCategoryDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductCategoryDto model)
        {
            var productCategory = await _categoryService.GetProductCategoryByIdAsync(model.Id);

            if (productCategory == null)
                return NotFound($"Product category Id={model.Id} is not found");

            productCategory = model.FromDto<ProductCategory>();

            await _categoryService.UpdateProductCategoryAsync(productCategory);

            return Ok();
        }
        
        #endregion
    }
}
