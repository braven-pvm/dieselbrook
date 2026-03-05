using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Tax;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Tax
{
    public partial class TaxController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;

        #endregion

        #region Ctor

        public TaxController(ICheckoutAttributeService checkoutAttributeService,
            ICustomerService customerService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStoreService storeService,
            ITaxService taxService)
        {
            _checkoutAttributeService = checkoutAttributeService;
            _customerService = customerService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _storeService = storeService;
            _taxService = taxService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets price
        /// </summary>
        /// /// <param name="productId">Product identifier</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="priceIncludesTax">A value indicating whether price already includes tax</param>
        [HttpGet("{productId}/{taxCategoryId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductPriceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetProductPrice(int productId,
            int taxCategoryId,
            [FromQuery, Required] decimal price,
            [FromQuery, Required] bool includingTax,
            int customerId,
            [FromQuery, Required] bool priceIncludesTax)
        {
            if (customerId <= 0 || productId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var response = await _taxService.GetProductPriceAsync(product, taxCategoryId, price, includingTax, customer,
                priceIncludesTax);

            return Ok(new ProductPriceResponse(response));
        }

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductPriceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShippingPrice([FromQuery, Required] decimal price,
            [FromQuery, Required] bool includingTax,
            int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var response = await _taxService.GetShippingPriceAsync(price, includingTax, customer);

            return Ok(new ProductPriceResponse(response));
        }

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductPriceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetPaymentMethodAdditionalFee([FromQuery, Required] decimal price,
            [FromQuery, Required] bool includingTax,
            int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var response = await _taxService.GetPaymentMethodAdditionalFeeAsync(price, includingTax, customer);

            return Ok(new ProductPriceResponse(response));
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="checkoutAttributeId">Checkout attribute identifier</param>
        /// <param name="checkoutAttributeValueId">Checkout attribute value identifier</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{checkoutAttributeId}/{checkoutAttributeValueId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductPriceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetCheckoutAttributePrice(int checkoutAttributeId,
            int checkoutAttributeValueId,
            [FromQuery, Required] bool includingTax,
            int customerId)
        {
            if (customerId <= 0 || checkoutAttributeId <= 0 || checkoutAttributeValueId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var checkoutAttribute = await _checkoutAttributeService.GetCheckoutAttributeByIdAsync(checkoutAttributeId);
            if (checkoutAttribute == null)
                return NotFound($"Checkout attribute Id={checkoutAttributeId} not found");

            var checkoutAttributeValue =
                await _checkoutAttributeService.GetCheckoutAttributeValueByIdAsync(checkoutAttributeValueId);
            if (checkoutAttributeValue == null)
                return NotFound($"Checkout attribute value Id={checkoutAttributeValueId} not found");

            var response = await _taxService.GetCheckoutAttributePriceAsync(checkoutAttribute, checkoutAttributeValue,
                includingTax, customer);

            return Ok(new ProductPriceResponse(response));
        }

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        [HttpGet]
        [ProducesResponseType(typeof(GetVatNumberStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetVatNumberStatus([FromQuery][Required] string fullVatNumber)
        {
            if (string.IsNullOrEmpty(fullVatNumber))
                return BadRequest();

            var vatNumberStatus = await _taxService.GetVatNumberStatusAsync(fullVatNumber);

            return Ok(new GetVatNumberStatusResponse(vatNumberStatus));
        }

        /// <summary>
        /// Get tax total for the passed shopping cart
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        [HttpGet("{customerId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetTaxTotalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTaxTotal(int customerId,
            int storeId,
            [FromQuery] bool usePaymentMethodAdditionalFee = true)
        {
            if (customerId <= 0 || storeId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var store = await _storeService.GetStoreByIdAsync(storeId);
            if (store == null)
                return NotFound($"Store Id={storeId} not found");

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                storeId);
            var response = await _taxService.GetTaxTotalAsync(cart, usePaymentMethodAdditionalFee);

            return Ok(new GetTaxTotalResponse(response));
        }

        /// <summary>
        /// Gets a value indicating whether a product is tax exempt
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{productId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsTaxExempt(int productId, int customerId)
        {
            if (productId <= 0 || customerId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"product Id={productId} not found");

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _taxService.IsTaxExemptAsync(product, customer));
        }

        #endregion
    }
}
