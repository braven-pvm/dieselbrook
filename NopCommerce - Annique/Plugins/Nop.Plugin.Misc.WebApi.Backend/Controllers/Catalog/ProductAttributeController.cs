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
    public partial class ProductAttributeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public ProductAttributeController(IProductAttributeService productAttributeService)
        {
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Delete a list of product attributes
        /// </summary>
        /// <param name="ids">Array of product attribute identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        public virtual async Task<IActionResult> DeleteProductAttributes(string ids)
        {
            var productAttributesId = ids.ToIdArray();
            var productAttributes = await _productAttributeService.GetProductAttributeByIdsAsync(productAttributesId);

            await _productAttributeService.DeleteProductAttributesAsync(productAttributes);

            return Ok();
        }

        /// <summary>
        /// Gets product attributes 
        /// </summary>
        /// <param name="ids">Array of product attribute identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<ProductAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductAttributeByIds(string ids)
        {
            var productAttributesId = ids.ToIdArray();
            var productAttributes = await _productAttributeService.GetProductAttributeByIdsAsync(productAttributesId);
            var productAttributesDto = productAttributes.Select(pa => pa.ToDto<ProductAttributeDto>());

            return Ok(productAttributesDto);
        }

        /// <summary>
        /// Returns a list of IDs of not existing attributes
        /// </summary>
        /// <param name="idsNames">Array of IDs of the attributes to check</param>
        [HttpGet("{idsNames}")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNotExistingAttributes(string idsNames)
        {
            var productAttributesId = idsNames.ToIdArray();
            var notExisting = await _productAttributeService.GetNotExistingAttributesAsync(productAttributesId);

            return Ok(notExisting.ToList());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(id);

            if (productAttribute == null)
                return NotFound($"Product attribute Id={id} not found");

            await _productAttributeService.DeleteProductAttributeAsync(productAttribute);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(id);

            if (productAttribute == null)
                return NotFound($"Product attribute Id={id} not found");

            return Ok(productAttribute.ToDto<ProductAttributeDto>());
        }

        /// <summary>
        /// Gets all product attributes
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ProductAttribute, ProductAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var productAttributes = await _productAttributeService.GetAllProductAttributesAsync(pageIndex, pageSize);

            return Ok(productAttributes.ToPagedListDto<ProductAttribute, ProductAttributeDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductAttributeDto model)
        {
            var productAttribute = model.FromDto<ProductAttribute>();

            await _productAttributeService.InsertProductAttributeAsync(productAttribute);

            var productAttributeDto = productAttribute.ToDto<ProductAttributeDto>();

            return Ok(productAttributeDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductAttributeDto model)
        {
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(model.Id);

            if (productAttribute == null)
                return NotFound($"Product attribute Id={model.Id} is not found");

            productAttribute = model.FromDto<ProductAttribute>();

            await _productAttributeService.UpdateProductAttributeAsync(productAttribute);

            return Ok();
        }
        
        #endregion
    }
}
