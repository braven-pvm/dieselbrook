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
    public partial class ProductSpecificationAttributeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ISpecificationAttributeService _specificationAttributeService;

        #endregion

        #region Ctor

        public ProductSpecificationAttributeController(ISpecificationAttributeService specificationAttributeService)
        {
            _specificationAttributeService = specificationAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a count of product specification attribute mapping records
        /// </summary>
        /// <param name="productId">Product identifier; 0 to load all records</param>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier; 0 to load all records</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductSpecificationAttributeCount([FromQuery] int productId = 0,
            [FromQuery] int specificationAttributeOptionId = 0)
        {
            var productSpecificationAttributes =
                await _specificationAttributeService.GetProductSpecificationAttributeCountAsync(productId,
                    specificationAttributeOptionId);

            return Ok(productSpecificationAttributes);
        }

        /// <summary>
        /// Get mapped products for specification attribute
        /// </summary>
        /// <param name="specificationAttributeId">The specification attribute identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{specificationAttributeId}")]
        [ProducesResponseType(typeof(PagedListDto<Product, ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductsBySpecificationAttributeId(int specificationAttributeId, 
            [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = int.MaxValue)
        {
            var products = await _specificationAttributeService.GetProductsBySpecificationAttributeIdAsync(specificationAttributeId, pageIndex, pageSize);

            return Ok(products.ToPagedListDto<Product, ProductDto>());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productSpecificationAttribute = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(id);

            if (productSpecificationAttribute == null)
                return NotFound($"Product specification attribute Id={id} not found");

            await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(productSpecificationAttribute);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductSpecificationAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productSpecificationAttribute = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(id);

            if (productSpecificationAttribute == null)
                return NotFound($"Product specification attribute Id={id} not found");

            return Ok(productSpecificationAttribute.ToDto<ProductSpecificationAttributeDto>());
        }

        /// <summary>
        /// Gets a product specification attribute mapping collection
        /// </summary>
        /// <param name="productId">Product identifier; 0 to load all records</param>
        /// <param name="specificationAttributeOptionId">Specification attribute option identifier; 0 to load all records</param>
        /// <param name="allowFiltering">0 to load attributes with AllowFiltering set to false, 1 to load attributes with AllowFiltering set to true, null to load all attributes</param>
        /// <param name="showOnProductPage">0 to load attributes with ShowOnProductPage set to false, 1 to load attributes with ShowOnProductPage set to true, null to load all attributes</param>
        /// <param name="specificationAttributeGroupId">Specification attribute group identifier; 0 to load all records; null to load attributes without group</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductSpecificationAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int productId = 0,
            [FromQuery] int specificationAttributeOptionId = 0,
            [FromQuery] bool? allowFiltering = null,
            [FromQuery] bool? showOnProductPage = null,
            [FromQuery] int? specificationAttributeGroupId = 0)
        {
            var productSpecificationAttributes =
                await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId,
                    specificationAttributeOptionId, allowFiltering, showOnProductPage, specificationAttributeGroupId);
            var productSpecificationAttributesDto =
                productSpecificationAttributes.Select(p => p.ToDto<ProductSpecificationAttributeDto>());

            return Ok(productSpecificationAttributesDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductSpecificationAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductSpecificationAttributeDto model)
        {
            var productSpecificationAttribute = model.FromDto<ProductSpecificationAttribute>();

            await _specificationAttributeService.InsertProductSpecificationAttributeAsync(productSpecificationAttribute);

            var productSpecificationAttributeDto = productSpecificationAttribute.ToDto<ProductSpecificationAttributeDto>();

            return Ok(productSpecificationAttributeDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductSpecificationAttributeDto model)
        {
            var productSpecificationAttribute = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.Id);

            if (productSpecificationAttribute == null)
                return NotFound($"Product specification attribute Id={model.Id} is not found");

            productSpecificationAttribute = model.FromDto<ProductSpecificationAttribute>();

            await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(productSpecificationAttribute);

            return Ok();
        }

        #endregion
    }
}
