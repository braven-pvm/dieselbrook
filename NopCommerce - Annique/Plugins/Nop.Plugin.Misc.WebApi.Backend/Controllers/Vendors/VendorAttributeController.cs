using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Vendors;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Vendors
{
    public partial class VendorAttributeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IVendorAttributeService _vendorAttributeService;

        #endregion

        #region Ctor

        public VendorAttributeController(IVendorAttributeService vendorAttributeService)
        {
            _vendorAttributeService = vendorAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all vendor attributes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<VendorAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var vendorAttributes = await _vendorAttributeService.GetAllVendorAttributesAsync();
            var vendorAttributesDto = vendorAttributes.Select(vendorAttr => vendorAttr.ToDto<VendorAttributeDto>()).ToList();

            return Ok(vendorAttributesDto);
        }

        /// <summary>
        /// Gets a vendor attributes by identifier
        /// </summary>
        /// <param name="id">The attribute identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(VendorAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var vendorAttribute = await _vendorAttributeService.GetVendorAttributeByIdAsync(id);

            if (vendorAttribute == null) return NotFound($"Vendor attribute Id={id} not found");

            var vendorAttributeDto = vendorAttribute.ToDto<VendorAttributeDto>();

            return Ok(vendorAttributeDto);
        }

        /// <summary>
        /// Create a vendor attribute
        /// </summary>
        /// <param name="model">Vendor attribute Dto</param>
        [HttpPost]
        [ProducesResponseType(typeof(VendorAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] VendorAttributeDto model)
        {
            var vendorAttribute = model.FromDto<VendorAttribute>();

            await _vendorAttributeService.InsertVendorAttributeAsync(vendorAttribute);

            var vendorAttributeDto = vendorAttribute.ToDto<VendorAttributeDto>();

            return Ok(vendorAttributeDto);
        }

        /// <summary>
        /// Updates the vendor attribute
        /// </summary>
        /// <param name="model">Vendor attribute Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] VendorAttributeDto model)
        {
            var vendorAttribute = await _vendorAttributeService.GetVendorAttributeByIdAsync(model.Id);

            if (vendorAttribute == null)
                return NotFound("Vendor attribute is not found");

            vendorAttribute = model.FromDto<VendorAttribute>();

            await _vendorAttributeService.UpdateVendorAttributeAsync(vendorAttribute);

            return Ok();
        }

        /// <summary>
        /// Delete a vendor attribute
        /// </summary>
        /// <param name="id">Vendor attribute identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var vendorAttr = await _vendorAttributeService.GetVendorAttributeByIdAsync(id);

            if (vendorAttr == null)
                return NotFound($"Vendor attribute Id={id} not found");

            await _vendorAttributeService.DeleteVendorAttributeAsync(vendorAttr);

            return Ok();
        }

        #endregion
    }
}
