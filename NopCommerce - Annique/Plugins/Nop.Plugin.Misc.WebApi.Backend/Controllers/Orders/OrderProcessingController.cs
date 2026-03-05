using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class OrderProcessingController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IShipmentService _shipmentService;

        #endregion

        #region Ctor

        public OrderProcessingController(IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IShipmentService shipmentService)
        {
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _shipmentService = shipmentService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks order status
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CheckOrderStatus(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.CheckOrderStatusAsync(order);

            return Ok();
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="orderId">The order</param>
        [HttpDelete("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> DeleteOrder(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.DeleteOrderAsync(order);

            return Ok();
        }

        /// <summary>
        /// Gets a value indicating whether cancel is allowed
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CanCancelOrder(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = _orderProcessingService.CanCancelOrder(order);

            return Ok(flag);
        }

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CancelOrder(int orderId, [FromQuery][Required] bool notifyCustomer)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.CancelOrderAsync(order, notifyCustomer);

            return Ok();
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as authorized
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CanMarkOrderAsAuthorized(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = _orderProcessingService.CanMarkOrderAsAuthorized(order);

            return Ok(flag);
        }

        /// <summary>
        /// Marks order as authorized
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> MarkAsAuthorized(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.MarkAsAuthorizedAsync(order);

            return Ok();
        }

        /// <summary>
        /// Gets a value indicating whether capture from admin panel is allowed
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CanCapture(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = _orderProcessingService.CanMarkOrderAsAuthorized(order);

            return Ok(flag);
        }

        /// <summary>
        /// Capture an order (from admin panel)
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Capture(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var errors = await _orderProcessingService.CaptureAsync(order);

            return Ok(errors);
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as paid
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CanMarkOrderAsPaid(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = _orderProcessingService.CanMarkOrderAsPaid(order);

            return Ok(flag);
        }

        /// <summary>
        /// Marks order as paid
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> MarkOrderAsPaid(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.MarkOrderAsPaidAsync(order);

            return Ok();
        }

        /// <summary>
        /// Gets a value indicating whether refund from admin panel is allowed
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CanRefund(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = await _orderProcessingService.CanRefundAsync(order);

            return Ok(flag);
        }

        /// <summary>
        /// Refunds an order (from admin panel)
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Refund(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var errors = await _orderProcessingService.RefundAsync(order);

            return Ok(errors);
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as refunded
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CanRefundOffline(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = _orderProcessingService.CanRefundOffline(order);

            return Ok(flag);
        }

        /// <summary>
        /// Refunds an order (offline)
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> RefundOffline(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.RefundOfflineAsync(order);

            return Ok();
        }

        /// <summary>
        /// Gets a value indicating whether partial refund from admin panel is allowed
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <param name="amountToRefund">Amount to refund</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CanPartiallyRefund(int orderId, [FromQuery][Required] decimal amountToRefund)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = await _orderProcessingService.CanPartiallyRefundAsync(order, amountToRefund);

            return Ok(flag);
        }

        /// <summary>
        /// Partially refunds an order (from admin panel)
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <param name="amountToRefund">Amount to refund</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> PartiallyRefund(int orderId, [FromQuery][Required] decimal amountToRefund)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var errors = await _orderProcessingService.PartiallyRefundAsync(order, amountToRefund);

            return Ok(errors);
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as partially refunded
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <param name="amountToRefund">Amount to refund</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CanPartiallyRefundOffline(int orderId, [FromQuery][Required] decimal amountToRefund)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = _orderProcessingService.CanPartiallyRefundOffline(order, amountToRefund);

            return Ok(flag);
        }

        /// <summary>
        /// Partially refunds an order (offline)
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <param name="amountToRefund">Amount to refund</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> PartiallyRefundOffline(int orderId, [FromQuery][Required] decimal amountToRefund)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.PartiallyRefundOfflineAsync(order, amountToRefund);

            return Ok();
        }

        /// <summary>
        /// Gets a value indicating whether void from admin panel is allowed
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CanVoid(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = await _orderProcessingService.CanVoidAsync(order);

            return Ok(flag);
        }

        /// <summary>
        /// Voids order (from admin panel)
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Void(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var errors = await _orderProcessingService.VoidAsync(order);

            return Ok(errors);
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as voided
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CanVoidOffline(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = _orderProcessingService.CanVoidOffline(order);

            return Ok(flag);
        }

        /// <summary>
        /// Voids order (offline)
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> VoidOffline(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.VoidOfflineAsync(order);

            return Ok();
        }

        /// <summary>
        /// Place order items in current user shopping cart.
        /// </summary>
        /// <param name="orderId">The order</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ReOrder(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            await _orderProcessingService.ReOrderAsync(order);

            return Ok();
        }

        /// <summary>
        /// Check whether return request is allowed
        /// </summary>
        /// <param name="orderId">Order Id</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsReturnRequestAllowed(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id={orderId} not found");

            var flag = await _orderProcessingService.IsReturnRequestAllowedAsync(order);

            return Ok(flag);
        }

        /// <summary>
        /// Places an order
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PlaceOrderResultDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PlaceOrder([FromBody] ProcessPaymentRequestDto model)
        {
            var result = await _orderProcessingService.PlaceOrderAsync(model.FromDto<ProcessPaymentRequest>());

            var resultDto = new PlaceOrderResultDto
            {
                Errors = result.Errors, Success = result.Success, PlacedOrder = result.PlacedOrder.ToDto<OrderDto>()
            };

            return Ok(resultDto);
        }

        /// <summary>
        /// Update order totals
        /// </summary>
        /// <param name="updateOrderParameters">Parameters for the updating order</param>
        [HttpPost]
        public virtual async Task<IActionResult> UpdateOrderTotals([FromBody]UpdateOrderParametersDto updateOrderParameters)
        {
            await _orderProcessingService.UpdateOrderTotalsAsync(updateOrderParameters.FromDto<UpdateOrderParameters>());

            return Ok();
        }

        /// <summary>
        /// Send a shipment
        /// </summary>
        /// <param name="shipmentId">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        [HttpGet("{shipmentId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Ship(int shipmentId, [FromQuery][Required] bool notifyCustomer)
        {
            if (shipmentId <= 0)
                return BadRequest();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            if (shipment == null)
                return NotFound($"Shipment Id={shipmentId} not found");

            await _orderProcessingService.ShipAsync(shipment, notifyCustomer);

            return Ok();
        }

        /// <summary>
        /// Marks a shipment as delivered
        /// </summary>
        /// <param name="shipmentId">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        [HttpGet("{shipmentId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Deliver(int shipmentId, [FromQuery][Required] bool notifyCustomer)
        {
            if (shipmentId <= 0)
                return BadRequest();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            if (shipment == null)
                return NotFound($"Shipment Id={shipmentId} not found");

            await _orderProcessingService.DeliverAsync(shipment, notifyCustomer);

            return Ok();
        }

        /// <summary>
        /// Gets a value indicating whether payment workflow is required
        /// </summary>
        /// <param name="ids">Array of Shopping cart item identifiers (separator - ;)</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsPaymentWorkflowRequired(string ids, [FromQuery]bool? useRewardPoints = null)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest();

            var cartIds = ids.ToIdArray();

            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _shoppingCartItemRepository.GetByIdsAsync(cartIds);

            var flag = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(shoppingCartItems, useRewardPoints);

            return Ok(flag);
        }

        /// <summary>
        /// Validate minimum order sub-total amount
        /// </summary>
        /// <param name="ids">Array of Shopping cart item identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ValidateMinOrderSubtotalAmount(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest();

            var cartIds = ids.ToIdArray();

            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _shoppingCartItemRepository.GetByIdsAsync(cartIds);

            var flag = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(shoppingCartItems);

            return Ok(flag);
        }

        /// <summary>
        /// Validate minimum order total amount
        /// </summary>
        /// <param name="ids">Array of Shopping cart item identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ValidateMinOrderTotalAmount(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest();

            var cartIds = ids.ToIdArray();

            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _shoppingCartItemRepository.GetByIdsAsync(cartIds);

            var flag = await _orderProcessingService.ValidateMinOrderTotalAmountAsync(shoppingCartItems);

            return Ok(flag);
        }

        #endregion
    }
}
