using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class TierPricesController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public TierPricesController(ICustomerService customerService, 
            IProductService productService,
            IStoreService storeService)
        {
            _customerService = customerService;
            _productService = productService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a product tier prices for customer
        /// </summary>
        /// <param name="productId">Product</param>
        /// <param name="customerId">Customer</param>
        /// <param name="storeId">Store identifier</param>
        [HttpGet("{productId}/{customerId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<TierPriceDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetTierPrices(int productId, int customerId, int storeId)
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

            var tierPrices = await _productService.GetTierPricesAsync(product, customer, store);

            var tierPricesDto = tierPrices.Select(tp => tp.ToDto<TierPriceDto>());

            return Ok(tierPricesDto);
        }

        /// <summary>
        /// Gets a tier prices by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<TierPriceDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetTierPricesByProduct(int productId)
        {
            var tierPrices = await _productService.GetTierPricesByProductAsync(productId);
            var tierPricesDto = tierPrices.Select(tp => tp.ToDto<TierPriceDto>());

            return Ok(tierPricesDto);
        }

        /// <summary>
        /// Gets a preferred tier price
        /// </summary>
        /// <param name="productId">Product</param>
        /// <param name="customerId">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="quantity">Quantity</param>
        [HttpGet("{productId}/{customerId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TierPriceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPreferredTierPrice(int productId, int customerId, int storeId, [FromQuery][Required] int quantity)
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

            var tierPrice = await _productService.GetPreferredTierPriceAsync(product, customer, store, quantity);

            return Ok(tierPrice.ToDto<TierPriceDto>());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var tierPrice = await _productService.GetTierPriceByIdAsync(id);

            if (tierPrice == null)
                return NotFound($"Tier price Id={id} not found");

            await _productService.DeleteTierPriceAsync(tierPrice);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TierPriceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var tierPrice = await _productService.GetTierPriceByIdAsync(id);

            if (tierPrice == null)
                return NotFound($"Tier price Id={id} not found");

            return Ok(tierPrice.ToDto<TierPriceDto>());
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(TierPriceDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] TierPriceDto model)
        {
            var tierPrice = model.FromDto<TierPrice>();

            await _productService.InsertTierPriceAsync(tierPrice);

            var tierPriceDto = tierPrice.ToDto<TierPriceDto>();

            return Ok(tierPriceDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] TierPriceDto model)
        {
            var tierPrice = await _productService.GetTierPriceByIdAsync(model.Id);

            if (tierPrice == null)
                return NotFound($"Tier price Id={model.Id} is not found");

            tierPrice = model.FromDto<TierPrice>();

            await _productService.UpdateTierPriceAsync(tierPrice);

            return Ok();
        }

        #endregion
    }
}
