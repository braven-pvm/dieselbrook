using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class ScheduleTaskController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly IScheduleTaskRunner _taskRunner;
        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public ScheduleTaskController(IScheduleTaskRunner taskRunner,
            IScheduleTaskService scheduleTaskService)
        {
            _taskRunner = taskRunner;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Run task
        /// </summary>
        /// <param name="taskType">Task type</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RunTask([FromQuery][Required] string taskType)
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(taskType);
            if (scheduleTask == null)
                //schedule task cannot be loaded
                return NotFound($"Not found the specified task by type={taskType}");

            await _taskRunner.ExecuteAsync(scheduleTask);

            return Ok();
        }

        #endregion
    }
}
