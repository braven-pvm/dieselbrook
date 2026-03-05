using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class OrderTotalCalculationController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IRepository<ShoppingCartItem> _repositoryShoppingCartItem;

        #endregion

        #region Ctor

        public OrderTotalCalculationController(ICustomerService customerService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IRepository<ShoppingCartItem> repositoryShoppingCartItem)
        {
            _customerService = customerService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _repositoryShoppingCartItem = repositoryShoppingCartItem;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(GetShoppingCartSubTotalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartSubTotal(string cartItemIds, [FromQuery][Required] bool includingTax)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(shoppingCartItems, includingTax);

            var response = new GetShoppingCartSubTotalResponse
            {
                DiscountAmount = result.discountAmount,
                AppliedDiscounts = result.appliedDiscounts.Select(discount => discount.ToDto<DiscountDto>()).ToList(),
                SubTotalWithoutDiscount = result.subTotalWithoutDiscount,
                SubTotalWithDiscount = result.subTotalWithDiscount,
                TaxRates = result.taxRates
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets a value indicating whether shipping is free
        /// </summary>
        /// <param name="ids">Cart ids (Separator - ;)</param>
        /// <param name="subTotal">Subtotal amount; pass null to calculate subtotal</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsFreeShipping(string ids, [FromQuery] decimal? subTotal = null)
        {
            var cartIds = ids.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _orderTotalCalculationService.IsFreeShippingAsync(shoppingCartItems, subTotal);

            return Ok(result);
        }

        /// <summary>
        /// Adjust shipping rate (free shipping, additional charges, discounts)
        /// </summary>
        /// <param name="shippingRate">Shipping rate to adjust</param>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="applyToPickupInStore">Adjust shipping rate to pickup in store shipping option rate</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(AdjustShippingRateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AdjustShippingRate(string cartItemIds,
            [FromQuery, Required] decimal shippingRate,
            [FromQuery] bool applyToPickupInStore = false)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result =
                await _orderTotalCalculationService.AdjustShippingRateAsync(shippingRate, shoppingCartItems,
                    applyToPickupInStore);

            var response = new AdjustShippingRateResponse
            {
                AdjustedShippingRate = result.adjustedShippingRate,
                AppliedDiscounts = result.appliedDiscounts.Select(discount => discount.ToDto<DiscountDto>())
                    .ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(GetShoppingCartShippingTotalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartShippingTotal(string cartItemIds, [FromQuery][Required] bool includingTax)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCartItems, includingTax);

            var response = new GetShoppingCartShippingTotalResponse
            {
                ShippingTotal = result.shippingTotal,
                TaxRate = result.taxRate,
                AppliedDiscounts = result.appliedDiscounts.Select(discount => discount.ToDto<DiscountDto>()).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets tax
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(GetTaxTotalTaxRateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTaxTotal(string cartItemIds, [FromQuery] bool usePaymentMethodAdditionalFee = true)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _orderTotalCalculationService.GetTaxTotalAsync(shoppingCartItems, usePaymentMethodAdditionalFee);

            var response = new GetTaxTotalTaxRateResponse
            {
                TaxTotal = result.taxTotal,
                TaxRates = result.taxRates
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets tax
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(GetShoppingCartTotalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetShoppingCartTotal(string cartItemIds,
            [FromQuery] bool? useRewardPoints = null,
            [FromQuery] bool usePaymentMethodAdditionalFee = true)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _orderTotalCalculationService.GetShoppingCartTotalAsync(shoppingCartItems,
                useRewardPoints, usePaymentMethodAdditionalFee);

            var response = new GetShoppingCartTotalResponse
            {
                ShoppingCartTotal = result.shoppingCartTotal,
                DiscountAmount = result.discountAmount,
                AppliedDiscounts =
                    result.appliedDiscounts.Select(discount => discount.ToDto<DiscountDto>()).ToList(),
                AppliedGiftCards =
                    result.appliedGiftCards.Select(giftCard => giftCard.ToDto<AppliedGiftCardResponseDto>())
                        .ToList(),
                RedeemedRewardPoints = result.redeemedRewardPoints,
                RedeemedRewardPointsAmount = result.redeemedRewardPointsAmount
            };

            return Ok(response);
        }

        /// <summary>
        /// Converts existing reward points to amount
        /// </summary>
        /// <param name="rewardPoints">Reward points</param>
        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertRewardPointsToAmount([FromQuery][Required] int rewardPoints)
        {
            var result = await _orderTotalCalculationService.ConvertRewardPointsToAmountAsync(rewardPoints);

            return Ok(result);
        }

        /// <summary>
        /// Gets a value indicating whether a customer has minimum amount of reward points to use (if enabled)
        /// </summary>
        /// <param name="rewardPoints">Reward points to check</param>
        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public IActionResult CheckMinimumRewardPointsToUseRequirement([FromQuery][Required] int rewardPoints)
        {
            var result = _orderTotalCalculationService.CheckMinimumRewardPointsToUseRequirement(rewardPoints);

            return Ok(result);
        }

        /// <summary>
        /// Calculate how order total (maximum amount) for which reward points could be earned/reduced
        /// </summary>
        /// <param name="orderShippingInclTax">Order shipping (including tax)</param>
        /// <param name="orderTotal">Order total</param>
        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public IActionResult CalculateApplicableOrderTotalForRewardPoints(
            [FromQuery, Required] decimal orderShippingInclTax,
            [FromQuery, Required] decimal orderTotal)
        {
            var result = _orderTotalCalculationService.CalculateApplicableOrderTotalForRewardPoints(orderShippingInclTax, orderTotal);

            return Ok(result);
        }

        /// <summary>
        /// Calculate how much reward points will be earned/reduced based on certain amount spent
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="amount">Amount (in primary store currency)</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CalculateRewardPoints(int customerId, [FromQuery][Required] decimal amount)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer by Id={customerId} not found");

            var result = await _orderTotalCalculationService.CalculateRewardPointsAsync(customer, amount);

            return Ok(result);
        }

        /// <summary>
        /// Calculate payment method fee
        /// </summary>
        /// <param name="cartItemIds">Cart item identifiers (separator - ;)</param>
        /// <param name="fee">Fee value</param>
        /// <param name="usePercentage">Is fee amount specified as percentage or fixed value?</param>
        [HttpGet("{cartItemIds}")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CalculatePaymentAdditionalFee(string cartItemIds,
            [FromQuery, Required] decimal fee,
            [FromQuery, Required] bool usePercentage)
        {
            var cartIds = cartItemIds.ToIdArray();
            if (!cartIds.Any())
                return BadRequest();

            var shoppingCartItems = await _repositoryShoppingCartItem.GetByIdsAsync(cartIds);

            var result = await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(shoppingCartItems, fee, usePercentage);

            return Ok(result);
        }

        #endregion
    }
}
