using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Directory;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class PriceFormatterController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public PriceFormatterController(ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IProductService productService)
        {
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats the price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="targetCurrency">Target currency code</param>
        /// <param name="languageId">Language</param>
        /// <param name="priceIncludesTax">A value indicating whether price includes tax</param>
        /// <param name="showTax">A value indicating whether to show tax suffix</param>
        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatPrice(int languageId,
            [FromQuery, Required] decimal price,
            [FromQuery, Required] bool showCurrency,
            [FromQuery, Required] string targetCurrency,
            [FromQuery, Required] bool priceIncludesTax,
            [FromQuery, Required] bool showTax)
        {
            if (string.IsNullOrEmpty(targetCurrency))
                return BadRequest();

            var currency = await _currencyService.GetCurrencyByCodeAsync(targetCurrency);

            if (currency == null)
                return NotFound($"Currency code={targetCurrency} not found");

            return Ok(await _priceFormatter.FormatPriceAsync(price, showCurrency, currency, languageId,
                priceIncludesTax, showTax));
        }

        /// <summary>
        /// Formats the order price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="currencyRate">Currency rate</param>
        /// <param name="customerCurrencyCode">Customer currency code</param>
        /// <param name="displayCustomerCurrency">A value indicating whether to display price on customer currency</param>
        /// <param name="primaryStoreCurrencyId">Primary store currency id</param>
        /// <param name="languageId">Language</param>
        /// <param name="priceIncludesTax">A value indicating whether price includes tax</param>
        /// <param name="showTax">A value indicating whether to show tax suffix</param>
        [HttpGet("{languageId}/{primaryStoreCurrencyId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatOrderPrice(int languageId, int primaryStoreCurrencyId,
            [FromQuery, Required] decimal price,
            [FromQuery, Required] decimal currencyRate,
            [FromQuery, Required] string customerCurrencyCode,
            [FromQuery, Required] bool displayCustomerCurrency,
            [FromQuery] bool? priceIncludesTax = null,
            [FromQuery] bool? showTax = null)
        {
            if (primaryStoreCurrencyId <= 0)
                return BadRequest();

            var currency = await _currencyService.GetCurrencyByIdAsync(primaryStoreCurrencyId);

            if (currency == null)
                return NotFound($"Currency Id={primaryStoreCurrencyId} not found");

            return Ok(await _priceFormatter.FormatOrderPriceAsync(price, currencyRate, customerCurrencyCode,
                displayCustomerCurrency, currency, languageId, priceIncludesTax, showTax));
        }

        /// <summary>
        /// Formats the price of rental product (with rental period)
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="price">Price</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatRentalProductPeriod(int productId, [FromQuery][Required] string price)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            return Ok(await _priceFormatter.FormatRentalProductPeriodAsync(product, price));
        }

        /// <summary>
        /// Formats the shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="currencyCode">Currency code</param>
        /// <param name="languageId">Language</param>
        /// <param name="priceIncludesTax">A value indicating whether price includes tax</param>
        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatShippingPrice(int languageId,
            [FromQuery, Required] decimal price,
            [FromQuery, Required] bool showCurrency,
            [FromQuery, Required] string currencyCode,
            [FromQuery, Required] bool priceIncludesTax)
        {
            return Ok(await _priceFormatter.FormatShippingPriceAsync(price, showCurrency, currencyCode, languageId,
                priceIncludesTax));
        }

        /// <summary>
        /// Formats the payment method additional fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="currencyCode">Currency code</param>
        /// <param name="languageId">Language</param>
        /// <param name="priceIncludesTax">A value indicating whether price includes tax</param>
        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatPaymentMethodAdditionalFee(int languageId,
            [FromQuery, Required] decimal price,
            [FromQuery, Required] bool showCurrency,
            [FromQuery, Required] string currencyCode,
            [FromQuery, Required] bool priceIncludesTax)
        {
            return Ok(await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(price, showCurrency,
                currencyCode, languageId, priceIncludesTax));
        }

        /// <summary>
        /// Formats a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult FormatTaxRate([FromQuery][Required] decimal taxRate)
        {
            return Ok(_priceFormatter.FormatTaxRate(taxRate));
        }

        /// <summary>
        /// Format base price (PAngV)
        /// </summary>
        /// <param name="productId">Product id</param>
        /// <param name="productPrice">Product price (in primary currency). Pass null if you want to use a default produce price</param>
        /// <param name="totalWeight">Total weight of product (with attribute weight adjustment). Pass null if you want to use a default produce weight</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatBasePrice(int productId,
            [FromQuery] decimal? productPrice,
            [FromQuery] decimal? totalWeight = null)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            return Ok(await _priceFormatter.FormatBasePriceAsync(product, productPrice, totalWeight));
        }

        #endregion
    }
}
