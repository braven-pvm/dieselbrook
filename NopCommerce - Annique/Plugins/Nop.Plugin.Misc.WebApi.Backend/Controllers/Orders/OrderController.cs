using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class OrderController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a order
        /// </summary>
        /// <param name="id">Order identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound($"Order Id={id} not found");
            }

            return Ok(order.ToDto<OrderDto>());
        }

        /// <summary>
        /// Gets a order by custome order number
        /// </summary>
        /// <param name="customOrderNumber">The custom order number</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByCustomOrderNumber([FromQuery][Required] string customOrderNumber)
        {
            if (string.IsNullOrEmpty(customOrderNumber))
                return BadRequest();

            var order = await _orderService.GetOrderByCustomOrderNumberAsync(customOrderNumber);

            if (order == null)
            {
                return NotFound("Order is not found");
            }

            return Ok(order.ToDto<OrderDto>());
        }

        /// <summary>
        /// Gets a order by order item identifier
        /// </summary>
        /// <param name="orderItemId">The order item identifier</param>
        [HttpGet("{orderItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByOrderItem(int orderItemId)
        {
            if (orderItemId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByOrderItemAsync(orderItemId);

            if (order == null)
            {
                return NotFound("Order is not found");
            }

            return Ok(order.ToDto<OrderDto>());
        }

        /// <summary>
        /// Get orders by identifiers
        /// </summary>
        /// <param name="ids">Array of order identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<OrderDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByIds(string ids)
        {
            var orderIds = ids.ToIdArray();
            var orders = await _orderService.GetOrdersByIdsAsync(orderIds);

            var ordersDto = orders.Select(order => order.ToDto<OrderDto>()).ToList();

            return Ok(ordersDto);
        }

        /// <summary>
        /// Get orders by guids
        /// </summary>
        /// <param name="orderGuids">Order guids</param>
        [HttpGet("{orderGuids}")]
        [ProducesResponseType(typeof(IList<OrderDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetOrdersByGuids(string orderGuids)
        {
            var orderGuid = orderGuids.ToGuidArray();
            var orders = await _orderService.GetOrdersByGuidsAsync(orderGuid);

            var ordersDto = orders.Select(order => order.ToDto<OrderDto>()).ToList();

            return Ok(ordersDto);
        }

        /// <summary>
        /// Gets an order by GUID
        /// </summary>
        /// <param name="guid">The order GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByGuid([FromQuery][Required] Guid guid)
        {
            var order = await _orderService.GetOrderByGuidAsync(guid);
            if (order == null)
                return NotFound("Order is not found");

            return Ok(order.ToDto<OrderDto>());
        }

        /// <summary>
        /// Delete a order
        /// </summary>
        /// <param name="id">Order identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound($"Order Id={id} not found");

            await _orderService.DeleteOrderAsync(order);

            return Ok();
        }

        /// <summary>
        /// Create an order
        /// </summary>
        /// <param name="model">Order Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] OrderDto model)
        {
            var order = model.FromDto<Order>();

            await _orderService.InsertOrderAsync(order);

            return Ok(order.ToDto<OrderDto>());
        }

        /// <summary>
        /// Update an order
        /// </summary>
        /// <param name="model">Order Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] OrderDto model)
        {
            var order = await _orderService.GetOrderByIdAsync(model.Id);

            if (order == null)
                return NotFound("Order is not found");

            order = model.FromDto<Order>();
            await _orderService.UpdateOrderAsync(order);

            return Ok();
        }

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; 0 to load all orders</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier, only orders with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="osIds">Order status identifiers; null to load all orders</param>
        /// <param name="psIds">Payment status identifiers; null to load all orders</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all orders</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Order, OrderDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Search([FromQuery] int storeId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] int customerId = 0,
            [FromQuery] int productId = 0,
            [FromQuery] int affiliateId = 0,
            [FromQuery] int warehouseId = 0,
            [FromQuery] int billingCountryId = 0,
            [FromQuery] string paymentMethodSystemName = null,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] string osIds = null,
            [FromQuery] string psIds = null,
            [FromQuery] string ssIds = null,
            [FromQuery] string billingPhone = null,
            [FromQuery] string billingEmail = null,
            [FromQuery] string billingLastName = "",
            [FromQuery] string orderNotes = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool getOnlyTotalCount = false)
        {
            var orderStatusIds = osIds.ToIdArray().ToList();
            var paymentStatusIds = psIds.ToIdArray().ToList();
            var shippingStatusIds = ssIds.ToIdArray().ToList();

            var orders = await _orderService.SearchOrdersAsync(storeId, vendorId, customerId, productId,
                affiliateId, warehouseId, billingCountryId, paymentMethodSystemName,
                createdFromUtc, createdToUtc,
                orderStatusIds, paymentStatusIds, shippingStatusIds,
                billingPhone, billingEmail, billingLastName, orderNotes,
                pageIndex, pageSize, getOnlyTotalCount);

            var ordersDto = orders.ToPagedListDto<Order, OrderDto>();

            return Ok(ordersDto);
        }

        /// <summary>
        /// Parse tax rates
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <param name="taxRates">Tax rates</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SortedDictionary<decimal, decimal>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseTaxRates(int orderId, [FromQuery][Required] string taxRates)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return NotFound("Order is not found");
            }

            var result = _orderService.ParseTaxRates(order, taxRates);

            return Ok(result);
        }

        /// <summary>
        /// Gets a value indicating whether an order has items to be added to a shipment
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HasItemsToAddToShipment(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return NotFound("Order is not found");
            }

            return Ok(await _orderService.HasItemsToAddToShipmentAsync(order));
        }

        /// <summary>
        /// Gets a value indicating whether an order has items to ship
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HasItemsToShip(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return NotFound("Order is not found");
            }

            return Ok(await _orderService.HasItemsToShipAsync(order));
        }

        /// <summary>
        /// Gets a value indicating whether an order has items to deliver
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HasItemsToDeliver(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return NotFound("Order is not found");
            }

            return Ok(await _orderService.HasItemsToDeliverAsync(order));
        }

        #endregion
    }
}
