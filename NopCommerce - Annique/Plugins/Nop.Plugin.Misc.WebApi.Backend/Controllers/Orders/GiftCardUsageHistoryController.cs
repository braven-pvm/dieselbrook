using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class GiftCardUsageHistoryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IGiftCardService _giftCardService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public GiftCardUsageHistoryController(IOrderService orderService,
            IGiftCardService giftCardService)
        {
            _orderService = orderService;
            _giftCardService = giftCardService;
        }

        #endregion

        #region Methods        

        /// <summary>
        /// Gets a gift card usage history entries by Gift card
        /// </summary>
        /// <param name="id">Gift card identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<GiftCardUsageHistoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetGiftCardUsageHistoryByGiftCard(int id)
        {
            if (id <= 0)
                return BadRequest();

            var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);

            if (giftCard == null)
                return NotFound("Gift card is not found");

            var giftCardUsageHistories = await _giftCardService.GetGiftCardUsageHistoryAsync(giftCard);

            var giftCardUsageHistoriesDto = giftCardUsageHistories.Select(gcuh => gcuh.ToDto<GiftCardUsageHistoryDto>()).ToList();

            return Ok(giftCardUsageHistoriesDto);
        }

        /// <summary>
        /// Gets a gift card usage history entries by order
        /// </summary>
        /// <param name="id">Order identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<GiftCardUsageHistoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetGiftCardUsageHistoryByOrder(int id)
        {
            if (id <= 0)
                return BadRequest();

            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound("Order is not found");

            var giftCardUsageHistories = await _giftCardService.GetGiftCardUsageHistoryAsync(order);

            var giftCardUsageHistoriesDto = giftCardUsageHistories.Select(gcuh => gcuh.ToDto<GiftCardUsageHistoryDto>()).ToList();

            return Ok(giftCardUsageHistoriesDto);
        }

        /// <summary>
        /// Create a gift card usage history entry
        /// </summary>
        /// <param name="model">Gift card usage history Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(GiftCardUsageHistoryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] GiftCardUsageHistoryDto model)
        {
            var giftCardUsageHistory = model.FromDto<GiftCardUsageHistory>();

            await _giftCardService.InsertGiftCardUsageHistoryAsync(giftCardUsageHistory);

            return Ok(giftCardUsageHistory.ToDto<GiftCardUsageHistoryDto>());
        }

        #endregion
    }
}
