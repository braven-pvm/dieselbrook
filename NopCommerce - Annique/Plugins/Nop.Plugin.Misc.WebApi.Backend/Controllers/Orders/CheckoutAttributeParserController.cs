using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class CheckoutAttributeParserController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;

        #endregion

        #region Ctor

        public CheckoutAttributeParserController(ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService)
        {
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets selected checkout attributes
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IList<CheckoutAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseCheckoutAttributes([FromBody]string attributesXml)
        {
            var attributes = await _checkoutAttributeParser.ParseCheckoutAttributesAsync(attributesXml);
            var attributesDto = attributes.Select(a => a.ToDto<CheckoutAttributeDto>());

            return Ok(attributesDto);
        }

        /// <summary>
        /// Get checkout attribute values
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost]
        [ProducesResponseType(typeof(IList<ParseCheckoutAttributeValuesResponse>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseCheckoutAttributeValues([FromBody]string attributesXml)
        {
            var values = await _checkoutAttributeParser.ParseCheckoutAttributeValues(attributesXml).ToListAsync();

            var valuesDto = await values.SelectAwait(async value => new ParseCheckoutAttributeValuesResponse
            {
                Attribute = value.attribute.ToDto<CheckoutAttributeDto>(),
                Values = (await value.values.ToListAsync()).Select(p => p.ToDto<CheckoutAttributeValueDto>()).ToList()
            }).ToListAsync();

            return Ok(valuesDto);
        }

        /// <summary>
        /// Gets selected checkout attribute value
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="attributeId">Checkout attribute identifier</param>
        [HttpPost("{attributeId}")]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public IActionResult ParseValues([FromBody]string attributesXml, int attributeId)
        {
            var values = _checkoutAttributeParser.ParseValues(attributesXml, attributeId);

            return Ok(values);
        }

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="checkoutAttributeId">Checkout attribute</param>
        /// <param name="value">Value</param>
        [HttpPost("{checkoutAttributeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> AddCheckoutAttribute([FromBody] string attributesXml,
            int checkoutAttributeId,
            [FromQuery, Required] string value)
        {
            if (checkoutAttributeId <= 0)
                return BadRequest();

            var customerAttribute = await _checkoutAttributeService.GetCheckoutAttributeByIdAsync(checkoutAttributeId);

            if (customerAttribute == null)
                return NotFound($"Checkout attribute Id={checkoutAttributeId} not found");

            var rezAttributesXml =
                _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, customerAttribute, value);

            return Ok(rezAttributesXml);
        }

        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="attributeId">Checkout attribute Id</param>
        /// <param name="attributesXml">Selected attributes (XML format)</param>
        [HttpPost("{attributeId}")]
        [ProducesResponseType(typeof(bool?), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> IsConditionMet(int attributeId, [FromBody]string attributesXml)
        {
            if (attributeId <= 0)
                return BadRequest();

            var attribute = await _checkoutAttributeService.GetCheckoutAttributeByIdAsync(attributeId);

            if (attribute == null)
                return NotFound($"Checkout attribute Id={attributeId} not found");

            var flag = await _checkoutAttributeParser.IsConditionMetAsync(attribute, attributesXml);

            return Ok(flag);
        }

        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="attributeId">Checkout attribute Id</param>
        [HttpPost("{attributeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> RemoveCheckoutAttribute([FromBody]string attributesXml, int attributeId)
        {
            if (attributeId <= 0)
                return BadRequest();

            var attribute = await _checkoutAttributeService.GetCheckoutAttributeByIdAsync(attributeId);

            if (attribute == null)
                return NotFound($"Checkout attribute Id={attributeId} not found");

            var xml = _checkoutAttributeParser.RemoveCheckoutAttribute(attributesXml, attribute);

            return Ok(xml);
        }

        #endregion
    }
}
