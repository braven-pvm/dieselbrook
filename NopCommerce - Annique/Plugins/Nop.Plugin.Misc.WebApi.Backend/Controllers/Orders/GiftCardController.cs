using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class GiftCardController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IGiftCardService _giftCardService;

        #endregion

        #region Ctor

        public GiftCardController(ICustomerService customerService,
            IGiftCardService giftCardService)
        {
            _customerService = customerService;
            _giftCardService = giftCardService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a gift card
        /// </summary>
        /// <param name="id">Gift card identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);

            if (giftCard == null)
                return NotFound($"Gift card Id={id} not found");

            await _giftCardService.DeleteGiftCardAsync(giftCard);

            return Ok();
        }

        /// <summary>
        /// Gets a gift card
        /// </summary>
        /// <param name="id">Gift card identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);

            if (giftCard == null)
            {
                return NotFound($"Gift card Id={id} not found");
            }

            return Ok(giftCard.ToDto<GiftCardDto>());
        }

        /// <summary>
        /// Gets all gift cards
        /// </summary>
        /// <param name="purchasedWithOrderId">Associated order ID; null to load all records</param>
        /// <param name="usedWithOrderId">The order ID in which the gift card was used; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="isGiftCardActivated">Value indicating whether gift card is activated; null to load all records</param>
        /// <param name="giftCardCouponCode">Gift card coupon code; null to load all records</param>
        /// <param name="recipientName">Recipient name; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<GiftCard, GiftCardDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int? purchasedWithOrderId = null,
            [FromQuery] int? usedWithOrderId = null,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] bool? isGiftCardActivated = null,
            [FromQuery] string giftCardCouponCode = null,
            [FromQuery] string recipientName = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var giftCards = await _giftCardService.GetAllGiftCardsAsync(purchasedWithOrderId, usedWithOrderId,
                createdFromUtc, createdToUtc,
                isGiftCardActivated, giftCardCouponCode, recipientName,
                pageIndex, pageSize);

            var giftCardsDto = giftCards.ToPagedListDto<GiftCard, GiftCardDto>();

            return Ok(giftCardsDto);
        }

        /// <summary>
        /// Create an gift card
        /// </summary>
        /// <param name="model">Gift card Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] GiftCardDto model)
        {
            var giftCard = model.FromDto<GiftCard>();

            await _giftCardService.InsertGiftCardAsync(giftCard);

            return Ok(giftCard.ToDto<GiftCardDto>());
        }

        /// <summary>
        /// Update an gift card
        /// </summary>
        /// <param name="model">Gift card Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] GiftCardDto model)
        {
            var giftCard = await _giftCardService.GetGiftCardByIdAsync(model.Id);

            if (giftCard == null)
                return NotFound("Gift card is not found");

            giftCard = model.FromDto<GiftCard>();
            await _giftCardService.UpdateGiftCardAsync(giftCard);

            return Ok();
        }

        /// <summary>
        /// Gets gift cards by 'PurchasedWithOrderItemId'
        /// </summary>
        /// <param name="id">Purchased with order item identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<GiftCardDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByPurchasedWithOrderItemId(int id)
        {
            if (id <= 0)
                return BadRequest();

            var giftCards = await _giftCardService.GetGiftCardsByPurchasedWithOrderItemIdAsync(id);

            var giftCardsDto = giftCards.Select(order => order.ToDto<GiftCardDto>()).ToList();

            return Ok(giftCardsDto);
        }

        /// <summary>
        /// Get active gift cards that are applied by a customer
        /// </summary>
        /// <param name="id">Customer identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<GiftCardDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAppliedByCustomer(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound($"Customer Id={id} not found");

            var giftCards = await _giftCardService.GetActiveGiftCardsAppliedByCustomerAsync(customer);

            var giftCardsDto = giftCards.Select(order => order.ToDto<GiftCardDto>()).ToList();

            return Ok(giftCardsDto);
        }

        /// <summary>
        /// Is gift card valid
        /// </summary>
        /// <param name="id">Gift card identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsGiftCardValid(int id)
        {
            if (id <= 0)
                return BadRequest();

            var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);

            if (giftCard == null)
                return NotFound("Gift card is not found");

            return Ok(await _giftCardService.IsGiftCardValidAsync(giftCard));
        }

        /// <summary>
        /// Gets a gift card remaining amount
        /// </summary>
        /// <param name="id">Gift card identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetGiftCardRemainingAmount(int id)
        {
            if (id <= 0)
                return BadRequest();

            var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);

            if (giftCard == null)
                return NotFound("Gift card is not found");

            return Ok(await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard));
        }

        #endregion
    }
}
