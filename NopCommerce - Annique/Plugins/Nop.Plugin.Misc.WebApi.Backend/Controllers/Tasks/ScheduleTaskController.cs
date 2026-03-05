using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Tasks;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Tasks
{
    public partial class ScheduleTaskController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public ScheduleTaskController(IScheduleTaskService scheduleTaskService)
        {
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all schedule tasks
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ScheduleTaskDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery]bool showHidden = false)
        {
            var scheduleTasks = await _scheduleTaskService.GetAllTasksAsync(showHidden);

            var scheduleTasksDto = scheduleTasks.Select(scheduleTask => scheduleTask.ToDto<ScheduleTaskDto>()).ToList();

            return Ok(scheduleTasksDto);
        }

        /// <summary>
        /// Gets a task by identifier
        /// </summary>
        /// <param name="id">Task identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ScheduleTaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var scheduleTask = await _scheduleTaskService.GetTaskByIdAsync(id);

            if (scheduleTask == null)
            {
                return NotFound($"Schedule task Id={id} not found");
            }

            var taxCategoryDto = scheduleTask.ToDto<ScheduleTaskDto>();

            return Ok(taxCategoryDto);
        }

        /// <summary>
        /// Gets a schedule task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ScheduleTaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetByType([FromQuery][Required] string type)
        {
            if (string.IsNullOrEmpty(type))
                return BadRequest();

            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(type);

            if (scheduleTask == null)
            {
                return NotFound($"Schedule task by Type={type} not found");
            }

            var scheduleTaskDto = scheduleTask.ToDto<ScheduleTaskDto>();

            return Ok(scheduleTaskDto);
        }

        /// <summary>
        /// Create a task
        /// </summary>
        /// <param name="model">Schedule task Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ScheduleTaskDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ScheduleTaskDto model)
        {
            var task = model.FromDto<ScheduleTask>();

            await _scheduleTaskService.InsertTaskAsync(task);

            var scheduleTaskDto = task.ToDto<ScheduleTaskDto>();

            return Ok(scheduleTaskDto);
        }

        /// <summary>
        /// Update a task
        /// </summary>
        /// <param name="model">Schedule task Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ScheduleTaskDto model)
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByIdAsync(model.Id);

            if (scheduleTask == null)
                return NotFound("Schedule task is not found");

            scheduleTask = model.FromDto<ScheduleTask>();
            await _scheduleTaskService.UpdateTaskAsync(scheduleTask);

            return Ok();
        }

        /// <summary>
        /// Delete a task
        /// </summary>
        /// <param name="id">Task identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var scheduleTask = await _scheduleTaskService.GetTaskByIdAsync(id);

            if (scheduleTask == null)
                return NotFound($"Schedule task Id={id} not found");

            await _scheduleTaskService.DeleteTaskAsync(scheduleTask);

            return Ok();
        }

        #endregion
    }
}
