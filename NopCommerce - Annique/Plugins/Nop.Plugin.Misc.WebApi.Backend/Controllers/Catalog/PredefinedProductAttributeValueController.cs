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
    public partial class PredefinedProductAttributeValueController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public PredefinedProductAttributeValueController(IProductAttributeService productAttributeService)
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

            var predefinedProductAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(id);

            if (predefinedProductAttributeValue == null)
                return NotFound($"Predefined product attribute value Id={id} not found");

            await _productAttributeService.DeletePredefinedProductAttributeValueAsync(predefinedProductAttributeValue);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PredefinedProductAttributeValueDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var predefinedProductAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(id);

            if (predefinedProductAttributeValue == null)
                return NotFound($"Predefined product attribute value Id={id} not found");

            return Ok(predefinedProductAttributeValue.ToDto<PredefinedProductAttributeValueDto>());
        }
        
        [HttpGet("{productAttributeId}")]
        [ProducesResponseType(typeof(IList<PredefinedProductAttributeValueDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(int productAttributeId)
        {
            var predefinedProductAttributeValues = await _productAttributeService.GetPredefinedProductAttributeValuesAsync(productAttributeId);
            var predefinedProductAttributeValuesDto =
                predefinedProductAttributeValues.Select(p => p.ToDto<PredefinedProductAttributeValueDto>());

            return Ok(predefinedProductAttributeValuesDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PredefinedProductAttributeValueDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] PredefinedProductAttributeValueDto model)
        {
            var predefinedProductAttributeValue = model.FromDto<PredefinedProductAttributeValue>();

            await _productAttributeService.InsertPredefinedProductAttributeValueAsync(predefinedProductAttributeValue);

            var predefinedProductAttributeValueDto = predefinedProductAttributeValue.ToDto<PredefinedProductAttributeValueDto>();

            return Ok(predefinedProductAttributeValueDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] PredefinedProductAttributeValueDto model)
        {
            var predefinedProductAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(model.Id);

            if (predefinedProductAttributeValue == null)
                return NotFound($"Predefined product attribute value Id={model.Id} is not found");

            predefinedProductAttributeValue = model.FromDto<PredefinedProductAttributeValue>();

            await _productAttributeService.UpdatePredefinedProductAttributeValueAsync(predefinedProductAttributeValue);

            return Ok();
        }

        #endregion
    }
}
