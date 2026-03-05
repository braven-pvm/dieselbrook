using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Shipping
{
    public partial class ShippingWorkflowController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IRepository<ShoppingCartItem> _repositoryShoppingCartItem;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;

        #endregion

        #region Ctor

        public ShippingWorkflowController(IAddressService addressService,
            ICustomerService customerService,
            IProductService productService,
            IRepository<ShoppingCartItem> repositoryShoppingCartItem,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService)
        {
            _addressService = addressService;
            _customerService = customerService;
            _productService = productService;
            _repositoryShoppingCartItem = repositoryShoppingCartItem;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets shopping cart item weight (of one item)
        /// </summary>
        /// <param name="cartItemId">Cart item identifier</param>
        /// <param name="ignoreFreeShippedItems">Whether to ignore the weight of the products marked as "Free shipping"</param>
        [HttpGet("{cartItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartItemWeight(int cartItemId, [FromQuery] bool ignoreFreeShippedItems = false)
        {
            if (cartItemId <= 0)
                return BadRequest();
            var shoppingCartItem = await _repositoryShoppingCartItem.GetByIdAsync(cartItemId);

            if (shoppingCartItem == null)
                return NotFound($"Shopping cart item by Id={cartItemId} not found");

            var result = await _shippingService.GetShoppingCartItemWeightAsync(shoppingCartItem, ignoreFreeShippedItems);

            return Ok(result);
        }

        /// <summary>
        /// Gets product item weight (of one item)
        /// </summary>
        /// <param name="productId">Product</param>
        /// <param name="attributesXml">Selected product attributes in XML</param>
        /// <param name="ignoreFreeShippedItems">Whether to ignore the weight of the products marked as "Free shipping"</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartItemWeight([FromBody] string attributesXml,
            int productId,
            [FromQuery] bool ignoreFreeShippedItems = false)
        {
            if (productId <= 0)
                return BadRequest();
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product by Id={productId} not found");

            var result =
                await _shippingService.GetShoppingCartItemWeightAsync(product, attributesXml, ignoreFreeShippedItems);

            return Ok(result);
        }

        /// <summary>
        /// Gets available shipping options
        /// </summary>
        /// <param name="shoppingCartIds">Shopping cart identifiers (separator - ;)</param>
        /// <param name="shippingAddressId">Shipping address</param>
        /// <param name="customerId">Load records allowed only to a specified customer; pass null to ignore ACL permissions</param>
        /// <param name="allowedShippingRateComputationMethodSystemName">Filter by shipping rate computation method identifier; null to load shipping options of all shipping rate computation methods</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        [HttpGet("{cartIds}/{shippingAddressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Dto.Shipping.GetShippingOptionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShippingOptions(string shoppingCartIds,
            int shippingAddressId,
            [FromQuery] int? customerId = null,
            [FromQuery] string allowedShippingRateComputationMethodSystemName = "",
            [FromQuery] int storeId = 0)
        {
            var cartIds = shoppingCartIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            if (shippingAddressId <= 0)
                return BadRequest();

            var shippingAddress = await _addressService.GetAddressByIdAsync(shippingAddressId);
            if (shippingAddress == null)
                return NotFound($"Shipping address by Id={shippingAddressId} not found");

            var customer = await _customerService.GetCustomerByIdAsync(customerId ?? 0);
            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);
            var result = await _shippingService.GetShippingOptionsAsync(shoppingCartItems, shippingAddress, customer,
                allowedShippingRateComputationMethodSystemName, storeId);

            var response = result.ToDto<Dto.Shipping.GetShippingOptionResponseDto>();

            return Ok(response);
        }

        /// <summary>
        /// Gets available pickup points
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        /// <param name="customerId">Load records allowed only to a specified customer; pass null to ignore ACL permissions</param>
        /// <param name="providerSystemName">Filter by provider identifier; null to load pickup points of all providers</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(Dto.Shipping.GetPickupPointsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetPickupPoints(int addressId,
            [FromQuery] int? customerId = null,
            [FromQuery] string providerSystemName = null,
            [FromQuery] int storeId = 0)
        {
            if (addressId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId ?? 0);

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);

            var address = await _addressService.GetAddressByIdAsync(addressId);

            var result = await _shippingService.GetPickupPointsAsync(cart, address, customer, providerSystemName, storeId);

            var response = result.ToDto<Dto.Shipping.GetPickupPointsResponseDto>();

            return Ok(response);
        }

        /// <summary>
        /// Whether the shopping cart item is ship enabled
        /// </summary>
        /// <param name="cartItemId">Cart item identifier</param>
        [HttpGet("{cartItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsShipEnabled(int cartItemId)
        {
            if (cartItemId <= 0)
                return BadRequest();
            var shoppingCartItem = await _repositoryShoppingCartItem.GetByIdAsync(cartItemId);

            if (shoppingCartItem == null)
                return NotFound($"Shopping cart item by Id={cartItemId} not found");

            var result = await _shippingService.IsShipEnabledAsync(shoppingCartItem);

            return Ok(result);
        }

        /// <summary>
        /// Whether the shopping cart item is free shipping
        /// </summary>
        /// <param name="cartItemId">Cart item identifier</param>
        [HttpGet("{cartItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsFreeShipping(int cartItemId)
        {
            if (cartItemId <= 0)
                return BadRequest();
            var shoppingCartItem = await _repositoryShoppingCartItem.GetByIdAsync(cartItemId);

            if (shoppingCartItem == null)
                return NotFound($"Shopping cart item by Id={cartItemId} not found");

            var result = await _shippingService.IsFreeShippingAsync(shoppingCartItem);

            return Ok(result);
        }

        /// <summary>
        /// Get the additional shipping charge
        /// </summary>
        /// <param name="cartItemId">Cart item identifier</param>
        [HttpGet("{cartItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAdditionalShippingCharge(int cartItemId)
        {
            if (cartItemId <= 0)
                return BadRequest();
            var shoppingCartItem = await _repositoryShoppingCartItem.GetByIdAsync(cartItemId);

            if (shoppingCartItem == null)
                return NotFound($"Shopping cart item by Id={cartItemId} not found");

            var result = await _shippingService.GetAdditionalShippingChargeAsync(shoppingCartItem);

            return Ok(result);
        }

        /// <summary>
        /// Gets a warehouse by identifier
        /// </summary>
        /// <param name="id">Warehouse identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetWarehousesById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var warehouse = await _shippingService.GetWarehouseByIdAsync(id);

            if (warehouse == null)
                return NotFound($"Warehouse Id={id} not found");

            return Ok(warehouse.ToDto<WarehouseDto>());
        }

        #endregion
    }
}
