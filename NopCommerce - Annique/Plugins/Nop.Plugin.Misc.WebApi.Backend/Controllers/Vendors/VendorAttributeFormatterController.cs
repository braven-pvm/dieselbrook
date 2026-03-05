using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Vendors
{
    public partial class VendorAttributeFormatterController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IVendorAttributeFormatter _vendorAttributeFormatter;

        #endregion

        #region Ctor

        public VendorAttributeFormatterController(IVendorAttributeFormatter vendorAttributeFormatter)
        {
            _vendorAttributeFormatter = vendorAttributeFormatter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Format vendor attributes
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
            var formattedAttr = await _vendorAttributeFormatter.FormatAttributesAsync(attributesXml, separator, htmlEncode);

            return Ok(formattedAttr);
        }

        #endregion
    }
}
