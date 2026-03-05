using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Security;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Security
{
    public partial class PermissionRecordController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public PermissionRecordController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all permissions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<PermissionRecordDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var permissionRecords = await _permissionService.GetAllPermissionRecordsAsync();

            var permissionRecordsDto = permissionRecords.Select(permissionRecord => permissionRecord.ToDto<PermissionRecordDto>()).ToList();

            return Ok(permissionRecordsDto);
        }

        /// <summary>
        /// Gets a permission
        /// </summary>
        /// <param name="id">Permission identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PermissionRecordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var permissionRecord = await _permissionService.GetPermissionRecordByIdAsync(id);

            if (permissionRecord == null)
            {
                return NotFound($"permissionRecord Id={id} not found");
            }

            return Ok(permissionRecord.ToDto<PermissionRecordDto>());
        }

        /// <summary>
        /// Create a permission
        /// </summary>
        /// <param name="model">Permission Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(PermissionRecordDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] PermissionRecordDto model)
        {
            var permissionRecord = model.FromDto<PermissionRecord>();

            await _permissionService.InsertPermissionRecordAsync(permissionRecord);

            return Ok(permissionRecord.ToDto<PermissionRecordDto>());
        }

        /// <summary>
        /// Update a permission by Id
        /// </summary>
        /// <param name="model">Permission record Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] PermissionRecordDto model)
        {
            var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync()).Where(p => p.Id == model.Id).FirstOrDefault();

            if (permissionRecord == null)
                return NotFound("Permission record is not found");

            permissionRecord = model.FromDto<PermissionRecord>();
            await _permissionService.UpdatePermissionRecordAsync(permissionRecord);

            return Ok();
        }

        /// <summary>
        /// Delete a permission
        /// </summary>
        /// <param name="id">Permission identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var permissionRecord = await _permissionService.GetPermissionRecordByIdAsync(id);

            if (permissionRecord == null)
                return NotFound($"permissionRecord Id={id} not found");

            await _permissionService.DeletePermissionRecordAsync(permissionRecord);

            return Ok();
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="systemName">Permission record system name</param>
        /// <param name="customerRoleId">Customer role identifier</param>
        [HttpGet("{customerRoleId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetActiveSlug([FromQuery][Required] string systemName, int customerRoleId)
        {
            var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync()).Where(p => p.SystemName.Equals(systemName)).FirstOrDefault();
            
            if (permissionRecord == null)
                return NotFound($"Permission record is not found");

            var res = await _permissionService.AuthorizeAsync(systemName, customerRoleId);

            return Ok(res);
        }

        #endregion
    }
}
