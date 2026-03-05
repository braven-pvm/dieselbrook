using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Common
{
    public partial class MaintenanceController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IMaintenanceService _maintenanceService;

        #endregion

        #region Ctor

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all backup files
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public IActionResult GetAllBackupFiles()
        {
            return Ok(_maintenanceService.GetAllBackupFiles());
        }

        /// <summary>
        /// Creates a path to a new database backup file
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult CreateNewBackupFilePath()
        {
            return Ok(_maintenanceService.CreateNewBackupFilePath());
        }

        /// <summary>
        /// Returns the path to the backup file
        /// </summary>
        /// <param name="backupFileName">The name of the backup file</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult GetBackupPath([FromQuery][Required] string backupFileName)
        {
            return Ok(_maintenanceService.GetBackupPath(backupFileName));
        }

        #endregion
    }
}
