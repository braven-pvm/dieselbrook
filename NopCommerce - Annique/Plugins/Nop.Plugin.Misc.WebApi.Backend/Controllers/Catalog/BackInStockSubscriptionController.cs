using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class BackInStockSubscriptionController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public BackInStockSubscriptionController(IBackInStockSubscriptionService backInStockSubscriptionService,
            IProductService productService)
        {
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(PagedListDto<BackInStockSubscription, BackInStockSubscriptionDto>),
            StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllByCustomerId(int customerId,
            [FromQuery] int storeId = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var backInStockSubscriptions = await _backInStockSubscriptionService.GetAllSubscriptionsByCustomerIdAsync(
                customerId,
                storeId, pageIndex, pageSize);

            return Ok(backInStockSubscriptions.ToPagedListDto<BackInStockSubscription, BackInStockSubscriptionDto>());
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier</param>
        [HttpGet("{customerId}/{productId}/{storeId}")]
        [ProducesResponseType(typeof(BackInStockSubscriptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FindSubscription(int customerId, int productId, int storeId)
        {
            var backInStockSubscriptions = await _backInStockSubscriptionService.FindSubscriptionAsync(customerId, productId, storeId);

            return Ok(backInStockSubscriptions.ToDto<BackInStockSubscriptionDto>());
        }

        /// <summary>
        /// Send notification to subscribers
        /// </summary>
        /// <param name="productId">Product Id</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SendNotificationsToSubscribers(int productId)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);
            
            if (product == null)
                return NotFound($"Product Id={productId} not found");

            await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);

            return Ok();
        }

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(PagedListDto<BackInStockSubscription, BackInStockSubscriptionDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllByProduct(int productId,
            [FromQuery] int storeId = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var backInStockSubscriptions = await _backInStockSubscriptionService.GetAllSubscriptionsByProductIdAsync(
                productId,
                storeId, pageIndex, pageSize);

            return Ok(backInStockSubscriptions.ToPagedListDto<BackInStockSubscription, BackInStockSubscriptionDto>());
        }

        /// <summary>
        /// Delete a back in stock subscription
        /// </summary>
        /// <param name="id">Subscription identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var backInStockSubscription = await _backInStockSubscriptionService.GetSubscriptionByIdAsync(id);

            if (backInStockSubscription == null)
                return NotFound($"Back in stock subscription Id={id} not found");

            await _backInStockSubscriptionService.DeleteSubscriptionAsync(backInStockSubscription);

            return Ok();
        }

        /// <summary>
        /// Gets a subscription
        /// </summary>
        /// <param name="id">Subscription identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BackInStockSubscriptionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var backInStockSubscription = await _backInStockSubscriptionService.GetSubscriptionByIdAsync(id);

            if (backInStockSubscription == null)
                return NotFound($"Back in stock subscription Id={id} not found");

            return Ok(backInStockSubscription.ToDto<BackInStockSubscriptionDto>());
        }

        /// <summary>
        /// Inserts subscription
        /// </summary>
        /// <param name="model">Subscription model</param>
        [HttpPost]
        [ProducesResponseType(typeof(BackInStockSubscriptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] BackInStockSubscriptionDto model)
        {
            var backInStockSubscription = model.FromDto<BackInStockSubscription>();

            await _backInStockSubscriptionService.InsertSubscriptionAsync(backInStockSubscription);

            var backInStockSubscriptionDto = backInStockSubscription.ToDto<BackInStockSubscriptionDto>();

            return Ok(backInStockSubscriptionDto);
        }

        #endregion
    }
}
