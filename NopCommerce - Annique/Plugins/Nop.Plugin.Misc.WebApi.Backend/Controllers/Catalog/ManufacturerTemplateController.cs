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
    public partial class ManufacturerTemplateController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IManufacturerTemplateService _manufacturerTemplateService;

        #endregion

        #region Ctor

        public ManufacturerTemplateController(IManufacturerTemplateService manufacturerTemplateService)
        {
            _manufacturerTemplateService = manufacturerTemplateService;
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

            var manufacturerTemplate = await _manufacturerTemplateService.GetManufacturerTemplateByIdAsync(id);

            if (manufacturerTemplate == null)
                return NotFound($"Manufacturer template Id={id} not found");

            await _manufacturerTemplateService.DeleteManufacturerTemplateAsync(manufacturerTemplate);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ManufacturerTemplateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var manufacturerTemplate = await _manufacturerTemplateService.GetManufacturerTemplateByIdAsync(id);

            if (manufacturerTemplate == null)
                return NotFound($"Manufacturer template Id={id} not found");

            return Ok(manufacturerTemplate.ToDto<ManufacturerTemplateDto>());
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<ManufacturerTemplateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var manufacturerTemplates = await _manufacturerTemplateService.GetAllManufacturerTemplatesAsync();
            var manufacturerTemplatesDto = manufacturerTemplates.Select(p => p.ToDto<ManufacturerTemplateDto>());

            return Ok(manufacturerTemplatesDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ManufacturerTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ManufacturerTemplateDto model)
        {
            var manufacturerTemplate = model.FromDto<ManufacturerTemplate>();

            await _manufacturerTemplateService.InsertManufacturerTemplateAsync(manufacturerTemplate);

            var manufacturerTemplateDto = manufacturerTemplate.ToDto<ManufacturerTemplateDto>();

            return Ok(manufacturerTemplateDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ManufacturerTemplateDto model)
        {
            var manufacturerTemplate = await _manufacturerTemplateService.GetManufacturerTemplateByIdAsync(model.Id);

            if (manufacturerTemplate == null)
                return NotFound($"Manufacturer template Id={model.Id} is not found");

            manufacturerTemplate = model.FromDto<ManufacturerTemplate>();

            await _manufacturerTemplateService.UpdateManufacturerTemplateAsync(manufacturerTemplate);

            return Ok();
        }

        #endregion
    }
}
