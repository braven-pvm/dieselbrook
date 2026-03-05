using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Logging;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Logging
{
    public partial class ActivityLogTypeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public ActivityLogTypeController(ICustomerActivityService customerActivityService)
        {
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Update an activity log type item
        /// </summary>
        /// <param name="model">Activity log type item Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ActivityLogTypeDto model)
        {
            var activityLogType = await _customerActivityService.GetActivityTypeByIdAsync(model.Id);

            if (activityLogType == null)
                return NotFound("Activity log type item is not found");

            activityLogType = model.FromDto<ActivityLogType>();
            await _customerActivityService.UpdateActivityTypeAsync(activityLogType);

            return Ok();
        }

        /// <summary>
        /// Gets all activity log type items
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ActivityLogTypeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var activityLogTypes = await _customerActivityService.GetAllActivityTypesAsync();
            var activityLogTypeDtos = activityLogTypes.Select(activityLogType => activityLogType.ToDto<ActivityLogTypeDto>()).ToList();

            return Ok(activityLogTypeDtos);
        }

        /// <summary>
        /// Gets an activity log type item by identifier
        /// </summary>
        /// <param name="id">The activity log type identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ActivityLogTypeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var activityLogType = await _customerActivityService.GetActivityTypeByIdAsync(id);

            if (activityLogType == null)
            {
                return NotFound($"Activity log type item Id={id} not found");
            }

            return Ok(activityLogType.ToDto<ActivityLogTypeDto>());
        }

        #endregion
    }
}
