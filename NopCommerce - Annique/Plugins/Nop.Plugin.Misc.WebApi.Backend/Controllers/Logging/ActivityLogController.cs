using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Logging;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Logging
{
    public partial class ActivityLogController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public ActivityLogController(ICustomerActivityService customerActivityService,
            ICustomerService customerService)
        {
            _customerActivityService = customerActivityService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; pass null to load all records</param>
        /// <param name="createdOnTo">Log item creation to; pass null to load all records</param>
        /// <param name="customerId">Customer identifier; pass null to load all records</param>
        /// <param name="activityLogTypeId">Activity log type identifier; pass null to load all records</param>
        /// <param name="ipAddress">IP address; pass null or empty to load all records</param>
        /// <param name="entityName">Entity name; pass null to load all records</param>
        /// <param name="entityId">Entity identifier; pass null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ActivityLog, ActivityLogDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] DateTime? createdOnFrom = null,
            [FromQuery] DateTime? createdOnTo = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? activityLogTypeId = null,
            [FromQuery] string ipAddress = null,
            [FromQuery] string entityName = null,
            [FromQuery] int? entityId = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var activityLogs = await _customerActivityService.GetAllActivitiesAsync(createdOnFrom, createdOnTo,
                customerId,
                activityLogTypeId, ipAddress, entityName, entityId, pageIndex, pageSize);

            var pagedListDto = activityLogs.ToPagedListDto<ActivityLog, ActivityLogDto>();

            return Ok(pagedListDto);
        }

        /// <summary>
        /// Gets an activity log item by identifier
        /// </summary>
        /// <param name="id">The activity log identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ActivityLogDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var activityLog = await _customerActivityService.GetActivityByIdAsync(id);

            if (activityLog == null) 
                return NotFound($"Activity log item Id={id} not found");

            return Ok(activityLog.ToDto<ActivityLogDto>());
        }

        /// <summary>
        /// Clears activity log
        /// </summary>
        [HttpDelete]
        public virtual async Task<IActionResult> ClearAllActivities()
        {
            await _customerActivityService.ClearAllActivitiesAsync();

            return Ok();
        }

        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="customerId">Customer</param>
        /// <param name="systemKeyword">System keyword</param>
        /// <param name="comment">Comment</param>
        [HttpPut("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ActivityLogDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> InsertActivity(int customerId,
            [FromQuery, Required] string systemKeyword,
            [FromQuery, Required] string comment)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var activityLog = await _customerActivityService.InsertActivityAsync(customer, systemKeyword, comment);

            return Ok(activityLog.ToDto<ActivityLogDto>());
        }

        #endregion
    }
}
