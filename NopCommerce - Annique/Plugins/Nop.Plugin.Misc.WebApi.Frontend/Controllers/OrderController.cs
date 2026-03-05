using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class OrderController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;

        #endregion

		#region Ctor

        public OrderController(IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService, 
            IOrderService orderService, 
            IPaymentService paymentService, 
            IProductService productService,
            IShipmentService shipmentService, 
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings)
        {
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _productService = productService;
            _shipmentService = shipmentService;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
        }

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(typeof(CustomerOrderListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CustomerOrders()
        {
            var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync();
            
            return Ok(model.ToDto<CustomerOrderListModelDto>());
        }

        //My account / Orders / Cancel recurring order
        [HttpPost]
        [ProducesResponseType(typeof(CustomerOrderListModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CancelRecurringPayment([FromBody] IDictionary<string, string> form)
        {

            //get recurring payment identifier
            var recurringPaymentId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("cancelRecurringPayment", StringComparison.InvariantCultureIgnoreCase))
                    recurringPaymentId = Convert.ToInt32(formValue["cancelRecurringPayment".Length..]);

            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(recurringPaymentId);
            if (recurringPayment == null) 
                return BadRequest();

            if (await _orderProcessingService.CanCancelRecurringPaymentAsync(await _workContext.GetCurrentCustomerAsync(), recurringPayment))
            {
                var errors = await _orderProcessingService.CancelRecurringPaymentAsync(recurringPayment);

                var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync();
                model.RecurringPaymentErrors = errors;

                return Ok(model.ToDto<CustomerOrderListModelDto>());
            }

            return BadRequest();
        }

        //My account / Orders / Retry last recurring order
        [HttpPost]
        [ProducesResponseType(typeof(CustomerOrderListModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> RetryLastRecurringPayment([FromBody] IDictionary<string, string> form)
        {
            //get recurring payment identifier
            var recurringPaymentId = 0;
            if (!form.Keys.Any(formValue => formValue.StartsWith("retryLastPayment", StringComparison.InvariantCultureIgnoreCase) &&
                int.TryParse(formValue[(formValue.IndexOf('_') + 1)..], out recurringPaymentId)))
                return BadRequest();

            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(recurringPaymentId);
            if (recurringPayment == null)
                return BadRequest();

            if (!await _orderProcessingService.CanRetryLastRecurringPaymentAsync(await _workContext.GetCurrentCustomerAsync(), recurringPayment))
                return BadRequest();

            var errors = await _orderProcessingService.ProcessNextRecurringPaymentAsync(recurringPayment);
            var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync();
            model.RecurringPaymentErrors = errors.ToList();

            return Ok(model.ToDto<CustomerOrderListModelDto>());
        }

        //My account / Reward points
        [HttpGet]
        [ProducesResponseType(typeof(CustomerRewardPointsModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CustomerRewardPoints([FromQuery] int? pageNumber)
        {
            if (!_rewardPointsSettings.Enabled)
                return BadRequest();

            var model = await _orderModelFactory.PrepareCustomerRewardPointsAsync(pageNumber);

            return Ok(model.ToDto<CustomerRewardPointsModelDto>());
        }

        //My account / Order details page
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OrderDetailsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Details(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null)
                return NotFound($"Order Id = {orderId} not found");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order.Deleted || customer.Id != order.CustomerId)
                return BadRequest("The order is deleted or belongs to another customer");

            var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                foreach (var item in model.Items
                    .Where(item => orderItem.OrderItemGuid == item.OrderItemGuid)
                    .Where(item => !product.VisibleIndividually)
                    .Where(item => product.ParentGroupedProductId > 0))
                {
                    item.ProductId = product.ParentGroupedProductId;
                }
            }

                return Ok(model.ToDto<OrderDetailsModelDto>());
        }
        
        //My account / Order details page / re-order
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ReOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id = {orderId} not found");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order.Deleted || customer.Id != order.CustomerId)
                return BadRequest("The order is deleted or belongs to another customer");

            await _orderProcessingService.ReOrderAsync(order);

            return Ok();
        }

        //My account / Order details page / Complete payment
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RePostPayment(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id = {orderId} not found");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order.Deleted || customer.Id != order.CustomerId)
                return BadRequest("The order is deleted or belongs to another customer");

            if (!await _paymentService.CanRePostProcessPaymentAsync(order))
                return BadRequest();

            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = order
            };
            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
            
            return Ok();
        }

        //My account / Order details page / Shipment details page
        [HttpGet("{shipmentId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ShipmentDetailsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ShipmentDetails(int shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            
            if (shipment == null)
                return NotFound($"Shipment Id = {shipmentId} not found");

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return BadRequest("The order is deleted or belongs to another customer");

            var model = await _orderModelFactory.PrepareShipmentDetailsModelAsync(shipment);

            return Ok(model.ToDto<ShipmentDetailsModelDto>());
        }
        
        #endregion
    }
}