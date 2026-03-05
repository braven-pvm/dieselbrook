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
    public partial class ProductAttributeValueController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public ProductAttributeValueController(IProductAttributeService productAttributeService)
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

            var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);

            if (productAttributeValue == null)
                return NotFound($"Product attribute value Id={id} not found");

            await _productAttributeService.DeleteProductAttributeValueAsync(productAttributeValue);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductAttributeValueDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);

            if (productAttributeValue == null)
                return NotFound($"Product attribute value Id={id} not found");

            return Ok(productAttributeValue.ToDto<ProductAttributeValueDto>());
        }

        [HttpGet("{mappingId}")]
        [ProducesResponseType(typeof(IList<ProductAttributeValueDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(int mappingId)
        {
            var productAttributeValues = await _productAttributeService.GetProductAttributeValuesAsync(mappingId);
            var productAttributeValuesDto = productAttributeValues.Select(p => p.ToDto<ProductAttributeValueDto>());

            return Ok(productAttributeValuesDto);
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ProductAttributeValueDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductAttributeValueDto model)
        {
            var productAttributeValue = model.FromDto<ProductAttributeValue>();

            await _productAttributeService.InsertProductAttributeValueAsync(productAttributeValue);

            var productAttributeValueDto = productAttributeValue.ToDto<ProductAttributeValueDto>();

            return Ok(productAttributeValueDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductAttributeValueDto model)
        {
            var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(model.Id);

            if (productAttributeValue == null)
                return NotFound($"Product attribute value Id={model.Id} is not found");

            productAttributeValue = model.FromDto<ProductAttributeValue>();

            await _productAttributeService.UpdateProductAttributeValueAsync(productAttributeValue);

            return Ok();
        }

        #endregion
    }
}
