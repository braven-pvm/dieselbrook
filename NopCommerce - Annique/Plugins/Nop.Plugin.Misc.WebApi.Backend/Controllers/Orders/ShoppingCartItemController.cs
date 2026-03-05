using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class ShoppingCartItemController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IRepository<ShoppingCartItem> _repositoryShoppingCartItem;
        private readonly IShoppingCartService _shoppingCartService;

        #endregion

        #region Ctor

        public ShoppingCartItemController(ICustomerService customerService,
            IProductService productService,
            IRepository<ShoppingCartItem> repositoryShoppingCartItem,
            IShoppingCartService shoppingCartService)
        {
            _customerService = customerService;
            _productService = productService;
            _repositoryShoppingCartItem = repositoryShoppingCartItem;
            _shoppingCartService = shoppingCartService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets shopping cart
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="shoppingCartType">Shopping cart type; pass null to load all records</param>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="productId">Product identifier; pass null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); pass null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); pass null to load all records</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<ShoppingCartItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCart(int customerId,
            [FromQuery] ShoppingCartType? shoppingCartType = null,
            [FromQuery] int storeId = 0,
            [FromQuery] int? productId = null,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer by Id={customerId} not found");

            var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(customer, shoppingCartType,
                storeId, productId,
                createdFromUtc, createdToUtc);

            var shoppingCartItemsDto = shoppingCartItems.Select(item => item.ToDto<ShoppingCartItemDto>()).ToList();

            return Ok(shoppingCartItemsDto);
        }

        /// <summary>
        /// Delete shopping cart item
        /// </summary>
        /// <param name="id">Shopping cart item identifier</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <param name="ensureOnlyActiveCheckoutAttributes">A value indicating whether to ensure that only active checkout attributes are attached to the current customer</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id,
            [FromQuery] bool resetCheckoutData = true,
            [FromQuery] bool ensureOnlyActiveCheckoutAttributes = false)
        {
            if (id <= 0)
                return BadRequest();

            await _shoppingCartService.DeleteShoppingCartItemAsync(id, resetCheckoutData,
                ensureOnlyActiveCheckoutAttributes);

            return Ok();
        }

        /// <summary>
        /// Deletes expired shopping cart items
        /// </summary>
        /// <param name="olderThanUtc">Older than date and time</param>
        [HttpDelete]
        public virtual async Task<IActionResult> DeleteExpired([FromQuery][Required] DateTime olderThanUtc)
        {
            await _shoppingCartService.DeleteExpiredShoppingCartItemsAsync(olderThanUtc);

            return Ok();
        }

        /// <summary>
        /// Validates shopping cart item attributes
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <param name="ignoreConditionMet">A value indicating whether we should ignore filtering by "is condition met" property</param>
        /// <param name="ignoreBundledProducts">A value indicating whether we should ignore bundled (associated) products</param>
        /// <param name="shoppingCartItemId">Shopping cart identifier; pass 0 if it's a new item</param>
        [HttpPost("{customerId}/{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartItemAttributeWarnings(int customerId,
            [FromQuery, Required] ShoppingCartType shoppingCartType,
            int productId,
            [FromQuery] int quantity = 1,
            [FromBody] string attributesXml = "",
            [FromQuery] bool ignoreNonCombinableAttributes = false,
            [FromQuery] bool ignoreConditionMet = false,
            [FromQuery] bool ignoreBundledProducts = false,
            [FromQuery] int shoppingCartItemId = 0)
        {
            if (customerId <= 0 || productId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer by Id={customerId} not found");

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product by Id={productId} not found");

            var result = await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(customer,
                shoppingCartType,
                product,
                quantity,
                attributesXml,
                ignoreNonCombinableAttributes,
                ignoreConditionMet,
                ignoreBundledProducts,
                shoppingCartItemId);

            return Ok(result);
        }

        /// <summary>
        /// Validates shopping cart item (gift card)
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productId">Product identifier</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartItemGiftCardWarnings([FromBody] string attributesXml,
            [FromQuery, Required] ShoppingCartType shoppingCartType,
            int productId)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product by Id={productId} not found");

            var result =
                await _shoppingCartService.GetShoppingCartItemGiftCardWarningsAsync(shoppingCartType, product,
                    attributesXml);

            return Ok(result);
        }

        /// <summary>
        /// Validates shopping cart item for rental products
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetRentalProductWarnings(int productId,
            [FromQuery] DateTime? rentalStartDate = null,
            [FromQuery] DateTime? rentalEndDate = null)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product by Id={productId} not found");

            var result =
                await _shoppingCartService.GetRentalProductWarningsAsync(product, rentalStartDate, rentalEndDate);

            return Ok(result);
        }

        /// <summary>
        /// Validates shopping cart item
        /// </summary>
        /// <param name="customerId">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productId">Product</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="addRequiredProducts">Whether to add required products</param>
        /// <param name="shoppingCartItemId">Shopping cart identifier; pass 0 if it's a new item</param>
        /// <param name="getStandardWarnings">A value indicating whether we should validate a product for standard properties</param>
        /// <param name="getAttributesWarnings">A value indicating whether we should validate product attributes</param>
        /// <param name="getGiftCardWarnings">A value indicating whether we should validate gift card properties</param>
        /// <param name="getRequiredProductWarnings">A value indicating whether we should validate required products (products which require other products to be added to the cart)</param>
        /// <param name="getRentalWarnings">A value indicating whether we should validate rental properties</param>
        [HttpPost("{customerId}/{productId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartItemWarnings([FromBody] string attributesXml,
            int customerId,
            [FromQuery, Required] ShoppingCartType shoppingCartType,
            int productId,
            int storeId,
            [FromQuery, Required] decimal customerEnteredPrice,
            [FromQuery] DateTime? rentalStartDate = null,
            [FromQuery] DateTime? rentalEndDate = null,
            [FromQuery] int quantity = 1,
            [FromQuery] bool addRequiredProducts = true,
            [FromQuery] int shoppingCartItemId = 0,
            [FromQuery] bool getStandardWarnings = true,
            [FromQuery] bool getAttributesWarnings = true,
            [FromQuery] bool getGiftCardWarnings = true,
            [FromQuery] bool getRequiredProductWarnings = true,
            [FromQuery] bool getRentalWarnings = true)
        {
            if (customerId <= 0 || productId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer by Id={customerId} not found");

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product by Id={productId} not found");

            var result = await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer,
                shoppingCartType, product, storeId, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate,
                quantity, addRequiredProducts, shoppingCartItemId, getStandardWarnings, getAttributesWarnings,
                getGiftCardWarnings, getRequiredProductWarnings, getRentalWarnings);

            return Ok(result);
        }

        /// <summary>
        /// Add a product to shopping cart
        /// </summary>
        /// <param name="customerId">Customer</param>
        /// <param name="productId">Product</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="addRequiredProducts">Whether to add required products</param>
        [HttpPost("{customerId}/{productId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AddToCart([FromBody] string attributesXml,
            int customerId,
            int productId,
            [FromQuery, Required] ShoppingCartType shoppingCartType,
            int storeId,
            [FromQuery] decimal customerEnteredPrice = decimal.Zero,
            [FromQuery] DateTime? rentalStartDate = null,
            [FromQuery] DateTime? rentalEndDate = null,
            [FromQuery] int quantity = 1,
            [FromQuery] bool addRequiredProducts = true)
        {
            if (customerId <= 0 || productId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer by Id={customerId} not found");

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product by Id={productId} not found");

            var result = await _shoppingCartService.AddToCartAsync(customer, product,
                shoppingCartType, storeId, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate,
                quantity, addRequiredProducts);

            return Ok(result);
        }

        /// <summary>
        /// Updates the shopping cart item
        /// </summary>
        /// <param name="customerId">Customer</param>
        /// <param name="shoppingCartItemId">Shopping cart item identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">New customer entered price</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">New shopping cart item quantity</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        [HttpPost("{customerId}/{shoppingCartItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateShoppingCartItem([FromBody] string attributesXml,
            int customerId,
            int shoppingCartItemId,
            [FromQuery, Required] decimal customerEnteredPrice,
            [FromQuery] DateTime? rentalStartDate = null,
            [FromQuery] DateTime? rentalEndDate = null,
            [FromQuery] int quantity = 1,
            [FromQuery] bool resetCheckoutData = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer by Id={customerId} not found");

            var result = await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                shoppingCartItemId, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate,
                quantity, resetCheckoutData);

            return Ok(result);
        }

        /// <summary>
        /// Migrate shopping cart
        /// </summary>
        /// <param name="fromCustomerId">From customer</param>
        /// <param name="toCustomerId">To customer</param>
        /// <param name="includeCouponCodes">A value indicating whether to coupon codes (discount and gift card) should be also re-applied</param>
        [HttpGet("{fromCustomerId}/{toCustomerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> MigrateShoppingCart(int fromCustomerId,
            int toCustomerId,
            [FromQuery, Required] bool includeCouponCodes)
        {
            if (fromCustomerId <= 0 || toCustomerId <= 0)
                return BadRequest();

            var fromCustomer = await _customerService.GetCustomerByIdAsync(fromCustomerId);
            if (fromCustomer == null)
                return NotFound($"Customer by Id={fromCustomerId} not found");

            var toCustomer = await _customerService.GetCustomerByIdAsync(toCustomerId);
            if (toCustomer == null)
                return NotFound($"Customer by Id={toCustomerId} not found");

            await _shoppingCartService.MigrateShoppingCartAsync(fromCustomer, toCustomer, includeCouponCodes);

            return Ok();
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{cartItemIds}/{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetProductsRequiringProduct(string cartItemIds, int productId)
        {
            if (productId <= 0)
                return BadRequest();
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product by Id={productId} not found");

            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var products = await _shoppingCartService.GetProductsRequiringProductAsync(shoppingCartItems, product);
            var productDtos = products.Select(p => p.ToDto<ProductDto>()).ToList();

            return Ok(productDtos);
        }

        /// <summary>
        /// Validates whether this shopping cart is valid
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="checkoutAttributesXml">Checkout attributes in XML format</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether to validate checkout attributes</param>
        [HttpPost("{cartItemIds}")]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartWarnings(string cartItemIds,
            [FromBody] string checkoutAttributesXml,
            [FromQuery, Required] bool validateCheckoutAttributes)
        {
            var cartIds = cartItemIds.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str))
                .ToList();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _shoppingCartService.GetShoppingCartWarningsAsync(shoppingCartItems,
                checkoutAttributesXml, validateCheckoutAttributes);

            return Ok(result);
        }

        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="cartItemId">Cart item identifier</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        [HttpGet("{cartItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetSubTotalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetSubTotal(int cartItemId, [FromQuery][Required] bool includeDiscounts)
        {
            if (cartItemId <= 0)
                return BadRequest();
            var shoppingCartItem = await _repositoryShoppingCartItem.GetByIdAsync(cartItemId);

            if (shoppingCartItem == null)
                return NotFound($"Shopping cart item by Id={cartItemId} not found");

            var result = await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, includeDiscounts);

            var response = new GetSubTotalResponse
            {
                SubTotal = result.subTotal,
                DiscountAmount = result.discountAmount,
                AppliedDiscounts = result.appliedDiscounts.Select(discount => discount.ToDto<DiscountDto>()).ToList(),
                MaximumDiscountQty = result.maximumDiscountQty
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="cartItemId">Cart item identifier</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        [HttpGet("{cartItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetUnitPriceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetUnitPrice(int cartItemId, [FromQuery][Required] bool includeDiscounts)
        {
            if (cartItemId <= 0)
                return BadRequest();

            var shoppingCartItem = await _repositoryShoppingCartItem.GetByIdAsync(cartItemId);

            if (shoppingCartItem == null)
                return NotFound($"Shopping cart item by Id={cartItemId} not found");

            var result = await _shoppingCartService.GetUnitPriceAsync(shoppingCartItem, includeDiscounts);

            var response = new GetUnitPriceResponse
            {
                UnitPrice = result.unitPrice,
                DiscountAmount = result.discountAmount,
                AppliedDiscounts = result.appliedDiscounts.Select(discount => discount.ToDto<DiscountDto>()).ToList(),
            };

            return Ok(response);
        }

        /// <summary>
        /// Finds a shopping cart item in the cart
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="cartItemIds">Shopping cart ids (separator - ;)</param>
        /// <param name="shoppingCartType">Shopping cart type (Shopping cart - 1, Wishlist - 2)</param>
        /// <param name="productId">Product</param>
        /// <param name="customerEnteredPrice">Price entered by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        [HttpPost("{cartItemIds}/{productId}")]
        [ProducesResponseType(typeof(ShoppingCartItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> FindShoppingCartItemInTheCart([FromBody] string attributesXml,
            string cartItemIds,
            [FromQuery, Required] ShoppingCartType shoppingCartType,
            int productId,
            [FromQuery] decimal customerEnteredPrice = decimal.Zero,
            [FromQuery] DateTime? rentalStartDate = null,
            [FromQuery] DateTime? rentalEndDate = null)
        {
            var cartIds = cartItemIds.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str))
                .ToList();
            if (!cartIds.Any())
                return BadRequest();

            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product by Id={productId} not found");

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(shoppingCartItems,
                shoppingCartType, product, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate);

            return Ok(result.ToDto<ShoppingCartItemDto>());
        }

        /// <summary>
        /// Indicates whether the shopping cart requires shipping
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ShoppingCartRequiresShipping(string cartItemIds)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _shoppingCartService.ShoppingCartRequiresShippingAsync(shoppingCartItems);

            return Ok(result);
        }

        /// <summary>
        /// Gets a value indicating whether shopping cart is recurring
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ShoppingCartIsRecurring(string cartItemIds)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _shoppingCartService.ShoppingCartIsRecurringAsync(shoppingCartItems);

            return Ok(result);
        }

        /// <summary>
        /// Get a recurring cycle information
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(GetRecurringCycleInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetRecurringCycleInfo(string cartItemIds)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _shoppingCartService.GetRecurringCycleInfoAsync(shoppingCartItems);

            var response = new GetRecurringCycleInfoResponse
            {
                Error = result.error,
                CycleLength = result.cycleLength,
                CyclePeriod = result.cyclePeriod,
                TotalCycles = result.totalCycles
            };

            return Ok(response);
        }

        #endregion
    }
}
