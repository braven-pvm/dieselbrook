using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Payments
{
    public partial class PaymentController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IRepository<ShoppingCartItem> _repositoryShoppingCartItem;

        #endregion

        #region Ctor

        public PaymentController(IOrderService orderService,
            IPaymentService paymentService,
            IRepository<ShoppingCartItem> repositoryShoppingCartItem)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _repositoryShoppingCartItem = repositoryShoppingCartItem;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CanRePostProcessPayment(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order by Id={orderId} not found");

            var result = await _paymentService.CanRePostProcessPaymentAsync(order);

            return Ok(result);
        }

        /// <summary>
        /// Gets an additional handling fee of a payment method
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAdditionalHandlingFee(string cartItemIds,
            [FromQuery, Required] string paymentMethodSystemName)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _paymentService.GetAdditionalHandlingFeeAsync(shoppingCartItems, paymentMethodSystemName);

            return Ok(result);
        }

        /// <summary>
        /// Gets a value indicating whether capture is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SupportCapture([FromQuery][Required] string paymentMethodSystemName)
        {
            var result = await _paymentService.SupportCaptureAsync(paymentMethodSystemName);

            return Ok(result);
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SupportPartiallyRefund([FromQuery][Required] string paymentMethodSystemName)
        {
            var result = await _paymentService.SupportPartiallyRefundAsync(paymentMethodSystemName);

            return Ok(result);
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SupportRefund([FromQuery][Required] string paymentMethodSystemName)
        {
            var result = await _paymentService.SupportRefundAsync(paymentMethodSystemName);

            return Ok(result);
        }

        /// <summary>
        /// Gets a value indicating whether void is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SupportVoid([FromQuery][Required] string paymentMethodSystemName)
        {
            var result = await _paymentService.SupportVoidAsync(paymentMethodSystemName);

            return Ok(result);
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        [HttpGet]
        [ProducesResponseType(typeof(RecurringPaymentType), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRecurringPaymentType([FromQuery][Required] string paymentMethodSystemName)
        {
            var result = await _paymentService.GetRecurringPaymentTypeAsync(paymentMethodSystemName);

            return Ok(result);
        }

        #endregion
    }
}
