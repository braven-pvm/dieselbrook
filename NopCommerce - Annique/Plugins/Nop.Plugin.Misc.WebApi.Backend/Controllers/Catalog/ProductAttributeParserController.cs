using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ProductAttributeParserController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public ProductAttributeParserController(IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService)
        {
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
        }

        #endregion

        #region Methods

        #region Product attributes

        /// <summary>
        /// Gets selected product attribute mappings
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost]
        [ProducesResponseType(typeof(IList<ProductAttributeMappingDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseProductAttributeMappings([FromBody] string attributesXml)
        {
            var mapping = await _productAttributeParser.ParseProductAttributeMappingsAsync(attributesXml);
            var mappingDto = mapping.Select(c => c.ToDto<ProductAttributeMappingDto>());

            return Ok(mappingDto);
        }

        /// <summary>
        /// Get product attribute values
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="mappingId">Product attribute mapping identifier; pass 0 to load all values</param>
        [HttpPost]
        [ProducesResponseType(typeof(IList<ProductAttributeValueDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseProductAttributeValues([FromBody] string attributesXml, [FromQuery] int mappingId = 0)
        {
            var values = await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml, mappingId);
            var valuesDto = values.Select(c => c.ToDto<ProductAttributeMappingDto>());

            return Ok(valuesDto);
        }

        /// <summary>
        /// Gets selected product attribute values
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="mappingId">Product attribute mapping identifier</param>
        [HttpPost("{mappingId}")]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public IActionResult ParseValues([FromBody] string attributesXml, int mappingId)
        {
            var values = _productAttributeParser.ParseValues(attributesXml, mappingId);

            return Ok(values);
        }

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="productAttributeMappingId">Product attribute mapping</param>
        /// <param name="value">Value</param>
        /// <param name="quantity">Quantity (used with AttributeValueType.AssociatedToProduct to specify the quantity entered by the customer)</param>
        [HttpPost("{productAttributeMappingId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddProductAttribute([FromBody] string attributesXml,
            int productAttributeMappingId,
            [FromQuery] string value,
            [FromQuery] int? quantity = null)
        {
            if (productAttributeMappingId <= 0)
                return BadRequest();

            var mapping =
                await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeMappingId);

            if (mapping == null)
                return NotFound($"Product attribute mapping Id={productAttributeMappingId} not found");

            var newAttributesXml = _productAttributeParser.AddProductAttribute(attributesXml, mapping, value, quantity);

            return Ok(newAttributesXml);
        }

        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="mappingId">Product attribute mapping id</param>
        [HttpPost("{mappingId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveProductAttribute([FromBody] string attributesXml, int mappingId)
        {
            if (mappingId <= 0)
                return BadRequest();

            var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(mappingId);

            if (mapping == null)
                return NotFound($"Product attribute mapping Id={mappingId} not found");

            var value = _productAttributeParser.RemoveProductAttribute(attributesXml, mapping);

            return Ok(value);
        }

        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <param name="ignoreQuantity">A value indicating whether we should ignore the quantity of attribute value entered by the customer</param>
        /// <param name="request">The attributes of the compared products</param>
        [HttpPost]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AreProductAttributesEqual(
            [FromBody] AreProductAttributesEqualRequest request,
            [FromQuery, Required] bool ignoreNonCombinableAttributes,
            [FromQuery] bool ignoreQuantity = true)
        {
            var areProductAttributesEqual = await _productAttributeParser.AreProductAttributesEqualAsync(
                request.AttributesXml1, request.AttributesXml2, ignoreNonCombinableAttributes, ignoreQuantity);

            return Ok(areProductAttributesEqual);
        }

        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="mappingId">Product attribute mapping Id</param>
        /// <param name="selectedAttributesXml">Selected attributes (XML format)</param>
        [HttpPost("{mappingId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool?), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsConditionMet(int mappingId, [FromBody] string selectedAttributesXml)
        {
            if (mappingId <= 0)
                return BadRequest();

            var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(mappingId);

            if (mapping == null)
                return NotFound($"Product attribute mapping Id={mappingId} not found");

            var value = await _productAttributeParser.IsConditionMetAsync(mapping, selectedAttributesXml);

            return Ok(value);
        }

        /// <summary>
        /// Finds a product attribute combination by attributes stored in XML 
        /// </summary>
        /// <param name="productId">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductAttributeCombinationDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FindProductAttributeCombination(int productId,
            [FromBody] string attributesXml,
            [FromQuery] bool ignoreNonCombinableAttributes = true)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product,
                attributesXml, ignoreNonCombinableAttributes);

            return Ok(combination.ToDto<ProductAttributeCombinationDto>());
        }

        /// <summary>
        /// Generate all combinations
        /// </summary>
        /// <param name="productId">Product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <param name="ids">List of allowed attribute identifiers (separator - ;). If null or empty then all attributes would be used.</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GenerateAllCombinations(int productId,
            [FromQuery] bool ignoreNonCombinableAttributes = false,
            [FromQuery] string ids = null)
        {
            var allowedAttributeIds = ids.ToIdArray();

            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var combinations = await _productAttributeParser.GenerateAllCombinationsAsync(product,
                ignoreNonCombinableAttributes, allowedAttributeIds);

            return Ok(combinations);
        }

        #endregion

        #region Gift card attributes

        /// <summary>
        /// Add gift card attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult AddGiftCardAttribute([FromBody] string attributesXml,
            [FromQuery, Required] string recipientName,
            [FromQuery, Required] string recipientEmail,
            [FromQuery, Required] string senderName,
            [FromQuery, Required] string senderEmail,
            [FromQuery, Required] string giftCardMessage)
        {
            var newAttributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml,
                recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);

            return Ok(newAttributesXml);
        }

        /// <summary>
        /// Get gift card attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost]
        [ProducesResponseType(typeof(GiftCardAttributeResponse), StatusCodes.Status200OK)]
        public IActionResult GetGiftCardAttribute([FromBody] string attributesXml)
        {
            _productAttributeParser.GetGiftCardAttribute(attributesXml, out var recipientName, out var recipientEmail, out var senderName, out var senderEmail, out var giftCardMessage);

            return Ok(new GiftCardAttributeResponse
            {
                GiftCardMessage = giftCardMessage,
                RecipientEmail = recipientEmail,
                RecipientName = recipientName,
                SenderEmail = senderEmail,
                SenderName = senderName
            });
        }

        #endregion

        #endregion
    }
}
