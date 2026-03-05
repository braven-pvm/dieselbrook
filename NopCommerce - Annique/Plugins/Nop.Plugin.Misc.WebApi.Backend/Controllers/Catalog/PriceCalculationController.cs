using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class PriceCalculationController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public PriceCalculationController(ICurrencyService currencyService,
            ICustomerService customerService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IStoreService storeService)
        {
            _currencyService = currencyService;
            _customerService = customerService;
            _priceCalculationService = priceCalculationService;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productId">Product id</param>
        /// <param name="customerId">The customer id</param>
        /// /// <param name="storeId">The store id</param>
        /// <param name="overriddenProductPrice">Overridden product price. If specified, then it'll be used instead of a product price. For example, used with product attribute combinations</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="rentalStartDate">Rental period start date (for rental products)</param>
        /// <param name="rentalEndDate">Rental period end date (for rental products)</param>
        [HttpGet("{productId}/{customerId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(FinalPriceResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetFinalPrice(int productId,
            int customerId,
            int storeId,
            [FromQuery] decimal? overriddenProductPrice,
            [FromQuery, Required] decimal additionalCharge,
            [FromQuery, Required] bool includeDiscounts,
            [FromQuery, Required] int quantity,
            [FromQuery] DateTime? rentalStartDate,
            [FromQuery] DateTime? rentalEndDate)
        {
            if (productId <= 0 || customerId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var store = await _storeService.GetStoreByIdAsync(storeId);

            if (store == null)
                return NotFound($"Store Id={storeId} not found");

            var rez = new FinalPriceResponse();

            List<Discount> appliedDiscounts;

            (rez.PriceWithoutDiscounts, rez.FinalPrice, rez.AppliedDiscountAmount, appliedDiscounts) =
                await _priceCalculationService.GetFinalPriceAsync(product, customer, store, overriddenProductPrice,
                    additionalCharge,
                    includeDiscounts, quantity, rentalStartDate, rentalEndDate);

            rez.AppliedDiscounts = appliedDiscounts.Select(d => d.ToDto<DiscountDto>()).ToList();

            return Ok(rez);
        }

        /// <summary>
        /// Gets the product cost (one item)
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="attributesXml">Shopping cart item attributes in XML</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductCost(int productId, [FromBody] string attributesXml)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            return Ok(await _priceCalculationService.GetProductCostAsync(product, attributesXml));
        }

        /// <summary>
        /// Get a price adjustment of a product attribute value
        /// </summary>
        /// <param name="productId">Product id</param>
        /// <param name="valueId">Product attribute value id</param>
        /// <param name="customerId">Customer id</param>
        /// /// <param name="storeId">Store id</param>
        /// <param name="productPrice">Product price (null for using the base product price)</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        [HttpGet("{productId}/{valueId}/{customerId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductAttributeValuePriceAdjustment(int productId, int valueId, int customerId, int storeId, [FromQuery] decimal? productPrice = null, [FromQuery] int quantity = 1)
        {
            if (customerId <= 0 || productId <= 0 || valueId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var store = await _storeService.GetStoreByIdAsync(storeId);

            if (store == null)
                return NotFound($"Store Id={storeId} not found");

            var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(valueId);

            if (attributeValue == null)
                return NotFound($"Product attribute value Id={valueId} not found");

            return Ok(await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store, productPrice, quantity));
        }

        /// <summary>
        /// Round a product or order total for the currency
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <param name="currencyId">Currency Id; pass null to use the primary store currency</param>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RoundPrice([FromQuery][Required] decimal value, [FromQuery] int? currencyId = null)
        {
            if (currencyId <= 0)
                return BadRequest();

            var currency = currencyId == null ? null : await _currencyService.GetCurrencyByIdAsync(currencyId.Value);

            if (currency == null)
                return NotFound($"Currency Id={currencyId} not found");

            return Ok(await _priceCalculationService.RoundPriceAsync(value, currency));
        }

        /// <summary>
        /// Round
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <param name="roundingType">The rounding type</param>
        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public IActionResult Round([FromQuery][Required] decimal value, [FromQuery][Required] RoundingType roundingType)
        {
            return Ok(_priceCalculationService.Round(value, roundingType));
        }

        #endregion
    }
}
