using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class OrderItemController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public OrderItemController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a order item
        /// </summary>
        /// <param name="id">Order item identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OrderItemDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var orderItem = await _orderService.GetOrderItemByIdAsync(id);

            if (orderItem == null)
            {
                return NotFound($"Order item Id={id} not found");
            }

            return Ok(orderItem.ToDto<OrderItemDto>());
        }

        /// <summary>
        /// Gets a product of specify order item
        /// </summary>
        /// <param name="id">Order item identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductByOrderItemId(int id)
        {
            if (id <= 0)
                return BadRequest();

            var product = await _orderService.GetProductByOrderItemIdAsync(id);

            if (product == null)
            {
                return NotFound($"Product by order item Id={id} not found");
            }

            return Ok(product.ToDto<ProductDto>());
        }

        /// <summary>
        /// Gets a list items of order
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <param name="isNotReturnable">Value indicating whether this product is returnable; pass null to ignore</param>
        /// <param name="isShipEnabled">Value indicating whether the entity is ship enabled; pass null to ignore</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<OrderItemDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetOrderItems(int orderId,
            [FromQuery] bool? isNotReturnable = null,
            [FromQuery] bool? isShipEnabled = null,
            [FromQuery] int vendorId = 0)
        {
            if (orderId <= 0)
                return BadRequest();

            var orderItems = await _orderService.GetOrderItemsAsync(orderId, isNotReturnable, isShipEnabled, vendorId);

            var orderItemsDto = orderItems.Select(orderItem => orderItem.ToDto<OrderItemDto>()).ToList();

            return Ok(orderItemsDto);
        }

        /// <summary>
        /// Gets an order item by GUID
        /// </summary>
        /// <param name="guid">The order GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(OrderItemDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByGuid([FromQuery][Required] Guid guid)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(guid);
            if (orderItem == null)
                return NotFound("Order item is not found");

            return Ok(orderItem.ToDto<OrderItemDto>());
        }

        /// <summary>
        /// Gets all downloadable order items
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(IList<OrderItemDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetOrderItemsByCustomer(int customerId)
        {
            var orderItems = await _orderService.GetDownloadableOrderItemsAsync(customerId);

            var orderItemsDto = orderItems.Select(orderItem => orderItem.ToDto<OrderItemDto>()).ToList();

            return Ok(orderItemsDto);
        }

        /// <summary>
        /// Delete a order item
        /// </summary>
        /// <param name="id">Order item identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var orderItem = await _orderService.GetOrderItemByIdAsync(id);

            if (orderItem == null)
                return NotFound($"Order item Id={id} not found");

            await _orderService.DeleteOrderItemAsync(orderItem);

            return Ok();
        }

        /// <summary>
        /// Gets a total number of items in all shipments
        /// </summary>
        /// <param name="orderItemId">The order item identifier</param>
        [HttpGet("{orderItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTotalNumberOfItemsInAllShipments(int orderItemId)
        {
            if (orderItemId <= 0)
                return BadRequest();

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);

            if (orderItem == null)
            {
                return NotFound("Order item is not found");
            }

            return Ok(await _orderService.GetTotalNumberOfItemsInAllShipmentsAsync(orderItem));
        }

        /// <summary>
        /// Gets a total number of already items which can be added to new shipments
        /// </summary>
        /// <param name="orderItemId">The order item identifier</param>
        [HttpGet("{orderItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTotalNumberOfItemsCanBeAddedToShipment(int orderItemId)
        {
            if (orderItemId <= 0)
                return BadRequest();

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);

            if (orderItem == null)
            {
                return NotFound("Order item is not found");
            }

            return Ok(await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem));
        }

        /// <summary>
        /// Gets a value indicating whether download is allowed
        /// </summary>
        /// <param name="orderItemId">The order item identifier</param>
        [HttpGet("{orderItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsDownloadAllowed(int orderItemId)
        {
            if (orderItemId <= 0)
                return BadRequest();

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);

            if (orderItem == null)
            {
                return NotFound("Order item is not found");
            }

            return Ok(await _orderService.IsDownloadAllowedAsync(orderItem));
        }

        /// <summary>
        /// Gets a value indicating whether license download is allowed
        /// </summary>
        /// <param name="orderItemId">The order item identifier</param>
        [HttpGet("{orderItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsLicenseDownloadAllowed(int orderItemId)
        {
            if (orderItemId <= 0)
                return BadRequest();

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);

            if (orderItem == null)
            {
                return NotFound("Order item is not found");
            }

            return Ok(await _orderService.IsLicenseDownloadAllowedAsync(orderItem));
        }

        /// <summary>
        /// Create an order item
        /// </summary>
        /// <param name="model">Order item Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(OrderItemDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] OrderItemDto model)
        {
            var orderItem = model.FromDto<OrderItem>();

            await _orderService.InsertOrderItemAsync(orderItem);

            return Ok(orderItem.ToDto<OrderItemDto>());
        }

        /// <summary>
        /// Update an order item
        /// </summary>
        /// <param name="model">Order item Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] OrderItemDto model)
        {
            var orderItem = await _orderService.GetOrderItemByIdAsync(model.Id);

            if (orderItem == null)
                return NotFound("Order item is not found");

            orderItem = model.FromDto<OrderItem>();
            await _orderService.UpdateOrderItemAsync(orderItem);

            return Ok();
        }

        #endregion
    }
}
