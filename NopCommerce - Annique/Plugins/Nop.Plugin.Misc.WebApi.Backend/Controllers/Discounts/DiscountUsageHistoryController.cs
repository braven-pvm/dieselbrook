using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Discounts;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Discounts
{
    public partial class DiscountUsageHistoryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IDiscountService _discountService;

        #endregion

        #region Ctor

        public DiscountUsageHistoryController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        #endregion

        #region Methods

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var discountUsageHistory = await _discountService.GetDiscountUsageHistoryByIdAsync(id);

            if (discountUsageHistory == null)
                return NotFound($"Discount usage history Id={id} not found");

            await _discountService.DeleteDiscountUsageHistoryAsync(discountUsageHistory);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DiscountUsageHistoryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var discountUsageHistory = await _discountService.GetDiscountUsageHistoryByIdAsync(id);

            if (discountUsageHistory == null)
                return NotFound($"Discount usage history Id={id} not found");

            return Ok(discountUsageHistory.ToDto<DiscountUsageHistoryDto>());
        }

        /// <summary>
        /// Gets all discount usage history records
        /// </summary>
        /// <param name="discountId">Discount identifier; null to load all records</param>
        /// <param name="customerId">Customer identifier; null to load all records</param>
        /// <param name="orderId">Order identifier; null to load all records</param>
        /// <param name="includeCancelledOrders">Include cancelled orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<DiscountUsageHistory, DiscountUsageHistoryDto>),
            StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int? discountId = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? orderId = null,
            [FromQuery] bool includeCancelledOrders = false,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var discountUsageHistories = await _discountService.GetAllDiscountUsageHistoryAsync(discountId, customerId,
                orderId,
                includeCancelledOrders,
                pageIndex, pageSize);

            return Ok(new PagedListDto<DiscountUsageHistory, DiscountUsageHistoryDto>(discountUsageHistories));
        }

        [HttpPost]
        [ProducesResponseType(typeof(DiscountUsageHistoryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] DiscountUsageHistoryDto model)
        {
            var discountUsageHistory = model.FromDto<DiscountUsageHistory>();

            await _discountService.InsertDiscountUsageHistoryAsync(discountUsageHistory);

            var discountUsageHistoryDto = discountUsageHistory.ToDto<DiscountUsageHistoryDto>();

            return Ok(discountUsageHistoryDto);
        }
        
        #endregion
    }
}
