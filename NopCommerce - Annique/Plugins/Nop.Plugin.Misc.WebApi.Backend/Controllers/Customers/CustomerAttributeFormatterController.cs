using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Customers
{
    public partial class CustomerAttributeFormatterController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;

        #endregion

        #region Ctor

        public CustomerAttributeFormatterController(ICustomerAttributeFormatter customerAttributeFormatter)
        {
            _customerAttributeFormatter = customerAttributeFormatter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatAttributes([FromBody] string attributesXml,
            [FromQuery] string separator = "<br />",
            [FromQuery] bool htmlEncode = true)
        {
            var formattedAttr =
                await _customerAttributeFormatter.FormatAttributesAsync(attributesXml, separator, htmlEncode);

            return Ok(formattedAttr);
        }

        #endregion
    }
}
