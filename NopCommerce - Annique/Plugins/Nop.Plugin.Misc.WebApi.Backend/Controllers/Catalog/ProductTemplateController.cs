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
    public partial class ProductTemplateController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductTemplateService _productTemplateService;

        #endregion

        #region Ctor

        public ProductTemplateController(IProductTemplateService productTemplateService)
        {
            _productTemplateService = productTemplateService;
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

            var productTemplate = await _productTemplateService.GetProductTemplateByIdAsync(id);

            if (productTemplate == null)
                return NotFound($"Product template Id={id} not found");

            await _productTemplateService.DeleteProductTemplateAsync(productTemplate);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productTemplate = await _productTemplateService.GetProductTemplateByIdAsync(id);

            if (productTemplate == null)
                return NotFound($"Product template Id={id} not found");

            return Ok(productTemplate.ToDto<ProductTemplateDto>());
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductTemplateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var productTemplates = await _productTemplateService.GetAllProductTemplatesAsync();
            var productTemplatesDto = productTemplates.Select(p => p.ToDto<ProductTemplateDto>());

            return Ok(productTemplatesDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductTemplateDto model)
        {
            var productTemplate = model.FromDto<ProductTemplate>();

            await _productTemplateService.InsertProductTemplateAsync(productTemplate);

            var productTemplateDto = productTemplate.ToDto<ProductTemplateDto>();

            return Ok(productTemplateDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductTemplateDto model)
        {
            var productTemplate = await _productTemplateService.GetProductTemplateByIdAsync(model.Id);

            if (productTemplate == null)
                return NotFound($"Product template Id={model.Id} is not found");

            productTemplate = model.FromDto<ProductTemplate>();

            await _productTemplateService.UpdateProductTemplateAsync(productTemplate);

            return Ok();
        }

        #endregion
    }
}
