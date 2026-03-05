using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Vendors;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Vendors
{
    public partial class VendorAttributeParserController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;

        #endregion

        #region Ctor

        public VendorAttributeParserController(IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService)
        {
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets vendor attributes from XML
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost]
        [ProducesResponseType(typeof(IList<VendorAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseVendorAttributes([FromBody]string attributesXml)
        {
            var attributes = await _vendorAttributeParser.ParseVendorAttributesAsync(attributesXml);
            var attributesDto = attributes.Select(a => a.ToDto<VendorAttributeDto>());

            return Ok(attributesDto);
        }

        /// <summary>
        /// Get vendor attribute values from XML
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost]
        [ProducesResponseType(typeof(IList<VendorAttributeValueDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseVendorAttributeValues([FromBody]string attributesXml)
        {
            var attributes = await _vendorAttributeParser.ParseVendorAttributeValuesAsync(attributesXml);
            var attributesDto = attributes.Select(a => a.ToDto<VendorAttributeDto>());

            return Ok(attributesDto);
        }

        /// <summary>
        /// Gets values of the selected vendor attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="attributeId">Vendor attribute identifier</param>
        [HttpPost("{attributeId}")]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public IActionResult ParseValues([FromBody]string attributesXml, int attributeId)
        {
            var values = _vendorAttributeParser.ParseValues(attributesXml, attributeId);

            return Ok(values);
        }

        /// <summary>
        /// Adds a vendor attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="vendorAttributeId">Vendor attribute Id</param>
        /// <param name="value">Value</param>
        [HttpPost("{vendorAttributeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> AddVendorAttribute([FromBody] string attributesXml,
            int vendorAttributeId,
            [FromQuery, Required] string value)
        {
            if (vendorAttributeId <= 0)
                return BadRequest();

            var vendorAttribute = await _vendorAttributeService.GetVendorAttributeByIdAsync(vendorAttributeId);

            if (vendorAttribute == null)
                return NotFound($"Vendor attribute Id={vendorAttributeId} not found");

            var rezXml = _vendorAttributeParser.AddVendorAttribute(attributesXml, vendorAttribute, value);

            return Ok(rezXml);
        }

        /// <summary>
        /// Validates vendor attributes
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAttributeWarnings([FromBody]string attributesXml)
        {
            var warnings = await _vendorAttributeParser.GetAttributeWarningsAsync(attributesXml);

            return Ok(warnings);
        }

        #endregion
    }
}
