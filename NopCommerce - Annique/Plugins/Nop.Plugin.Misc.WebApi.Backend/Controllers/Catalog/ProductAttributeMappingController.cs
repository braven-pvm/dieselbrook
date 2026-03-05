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
    public partial class ProductAttributeMappingController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public ProductAttributeMappingController(IProductAttributeService productAttributeService)
        {
            _productAttributeService = productAttributeService;
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

            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id);

            if (productAttributeMapping == null)
                return NotFound($"Product attribute mapping Id={id} not found");

            await _productAttributeService.DeleteProductAttributeMappingAsync(productAttributeMapping);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductAttributeMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id);

            if (productAttributeMapping == null)
                return NotFound($"Product attribute mapping Id={id} not found");

            return Ok(productAttributeMapping.ToDto<ProductAttributeMappingDto>());
        }

        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<ProductAttributeMappingDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllByProductId(int productId)
        {
            var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
            var productAttributeMappingsDto =
                productAttributeMappings.Select(p => p.ToDto<ProductAttributeMappingDto>());

            return Ok(productAttributeMappingsDto);
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ProductAttributeMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductAttributeMappingDto model)
        {
            var productAttributeMapping = model.FromDto<ProductAttributeMapping>();

            await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMapping);

            var productAttributeMappingDto = productAttributeMapping.ToDto<ProductAttributeMappingDto>();

            return Ok(productAttributeMappingDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductAttributeMappingDto model)
        {
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.Id);

            if (productAttributeMapping == null)
                return NotFound($"Product attribute mapping Id={model.Id} is not found");

            productAttributeMapping = model.FromDto<ProductAttributeMapping>();

            await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);

            return Ok();
        }

        #endregion
    }
}
