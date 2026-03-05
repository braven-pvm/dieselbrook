using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class CheckoutAttributeFormatterController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public CheckoutAttributeFormatterController(ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICustomerService customerService)
        {
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerId">Customer Id</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperlink tags could be rendered (if required)</param>
        [HttpPost("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> FormatAttributes([FromBody]string attributesXml,
            int customerId,
            [FromQuery]string separator = "<br />",
            [FromQuery]bool htmlEncode = true,
            [FromQuery]bool renderPrices = true,
            [FromQuery]bool allowHyperlinks = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var attributeXml = await _checkoutAttributeFormatter.FormatAttributesAsync(attributesXml, customer,
                separator, htmlEncode, renderPrices, allowHyperlinks);

            return Ok(attributeXml);
        }

        #endregion
    }
}
