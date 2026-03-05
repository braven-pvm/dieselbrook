using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ProductAttributeFormatterController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public ProductAttributeFormatterController(ICustomerService customerService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductService productService,
            IStoreService storeService)
        {
            _customerService = customerService;
            _productAttributeFormatter = productAttributeFormatter;
            _productService = productService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatAttributes(int productId, [FromBody] string attributesXml)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            return Ok(await _productAttributeFormatter.FormatAttributesAsync(product, attributesXml));
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// /// <param name="customerId">Customer Id</param>
        /// <param name="storeId">Store Id</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="renderProductAttributes">A value indicating whether to render product attributes</param>
        /// <param name="renderGiftCardAttributes">A value indicating whether to render gift card attributes</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        [HttpPost("{productId}/{customerId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatAttributes(int productId,
            int customerId,
            int storeId,
            [FromBody] string attributesXml,
            [FromQuery] string separator = "<br />",
            [FromQuery] bool htmlEncode = true,
            [FromQuery] bool renderPrices = true,
            [FromQuery] bool renderProductAttributes = true,
            [FromQuery] bool renderGiftCardAttributes = true,
            [FromQuery] bool allowHyperlinks = true)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={productId} not found");

            var store = await _storeService.GetStoreByIdAsync(storeId);

            if (store == null)
                return NotFound($"Store Id={storeId} not found");

            return Ok(await _productAttributeFormatter.FormatAttributesAsync(product, attributesXml, customer, store,
                separator, htmlEncode, renderPrices, renderProductAttributes,
                renderGiftCardAttributes, allowHyperlinks));
        }

        #endregion
    }
}
