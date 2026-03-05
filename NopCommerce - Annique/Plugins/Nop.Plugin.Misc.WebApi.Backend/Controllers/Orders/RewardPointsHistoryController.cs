using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class RewardPointsHistoryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IRewardPointService _rewardPointService;

        #endregion

        #region Ctor

        public RewardPointsHistoryController(ICustomerService customerService,
            IOrderService orderService,
            IRewardPointService rewardPointService)
        {
            _customerService = customerService;
            _orderService = orderService;
            _rewardPointService = rewardPointService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load reward point history records
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; pass null to load all records</param>
        /// <param name="showNotActivated">A value indicating whether to show reward points that did not yet activated</param>
        /// <param name="orderGuid">Order Guid; pass null to load all record</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<RewardPointsHistory, RewardPointsHistoryDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetRewardPointsHistory([FromQuery] int customerId = 0,
            [FromQuery] int? storeId = null,
            [FromQuery] bool showNotActivated = false,
            [FromQuery] Guid? orderGuid = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            if (customerId < 0)
                return BadRequest();

            var rewardPointsHistoryList = await _rewardPointService.GetRewardPointsHistoryAsync(customerId, storeId,
                showNotActivated, orderGuid, pageIndex, pageSize);

            var rewardPointsHistoryListDto =
                rewardPointsHistoryList.ToPagedListDto<RewardPointsHistory, RewardPointsHistoryDto>();

            return Ok(rewardPointsHistoryListDto);
        }

        /// <summary>
        /// Gets reward points balance
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="storeId">Store identifier</param>
        [HttpGet("{customerId}/{storeId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetRewardPointsBalance(int customerId, int storeId)
        {
            if (customerId < 0 || storeId < 0)
                return BadRequest();

            var result = await _rewardPointService.GetRewardPointsBalanceAsync(customerId, storeId);

            return Ok(result);
        }
        
        /// <summary>
        /// Add reward points history record
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="points">Number of points to add</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="message">Message</param>
        /// <param name="usedWithOrderId">The order identifier for which points were redeemed (spent) as a payment</param>
        /// <param name="usedAmount">Used amount</param>
        /// <param name="activatingDate">Date and time of activating reward points; pass null to immediately activating</param>
        /// <param name="endDate">Date and time when the reward points will no longer be valid; pass null to add date termless points</param>
        [HttpGet("{customerId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AddRewardPointsHistoryEntry(int customerId,
            [FromQuery, Required] int points,
            int storeId,
            [FromQuery] string message = "",
            [FromQuery] int? usedWithOrderId = null,
            [FromQuery] decimal usedAmount = 0M,
            [FromQuery] DateTime? activatingDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (customerId <= 0 || storeId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer by Id={customerId} not found");

            var order = await _orderService.GetOrderByIdAsync(usedWithOrderId ?? 0);

            var result = await _rewardPointService.AddRewardPointsHistoryEntryAsync(customer, points, storeId,
                message, order, usedAmount,
                activatingDate, endDate);

            return Ok(result);
        }

        /// <summary>
        /// Gets a reward point history entry
        /// </summary>
        /// <param name="id">Reward point history entry identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RewardPointsHistoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var rewardPoint = await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(id);

            if (rewardPoint == null) 
                return NotFound($"Reward point history Id={id} not found");

            return Ok(rewardPoint.ToDto<RewardPointsHistoryDto>());
        }

        /// <summary>
        /// Update the reward point history entry
        /// </summary>
        /// <param name="model">Reward point history Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] RewardPointsHistoryDto model)
        {
            var rewardPoint = await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(model.Id);

            if (rewardPoint == null)
                return NotFound("Reward point history is not found");

            rewardPoint = model.FromDto<RewardPointsHistory>();
            await _rewardPointService.UpdateRewardPointsHistoryEntryAsync(rewardPoint);

            return Ok();
        }

        /// <summary>
        /// Delete the reward point history entry
        /// </summary>
        /// <param name="id">Reward point history identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var rewardPoint = await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(id);

            if (rewardPoint == null)
                return NotFound($"Reward point history Id={id} not found");

            await _rewardPointService.DeleteRewardPointsHistoryEntryAsync(rewardPoint);

            return Ok();
        }

        #endregion
    }
}
