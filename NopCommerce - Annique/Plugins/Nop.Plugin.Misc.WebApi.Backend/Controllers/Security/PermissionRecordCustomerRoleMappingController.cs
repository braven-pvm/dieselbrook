using System.Collections.Generic;
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
    public partial class PermissionRecordCustomerRoleMappingController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public PermissionRecordCustomerRoleMappingController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a permission record-customer role mapping
        /// </summary>
        /// <param name="id">Permission identifier</param>        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IList<PermissionRecordCustomerRoleMappingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var mappings = await _permissionService.GetMappingByPermissionRecordIdAsync(id);
            var mappingsDto = mappings.Select(map => map.ToDto<PermissionRecordCustomerRoleMappingDto>()).ToList();

            return Ok(mappingsDto);
        }

        /// <summary>
        /// Create a permission record-customer role mapping
        /// </summary>
        /// <param name="model">Permission record-customer role mapping Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(PermissionRecordCustomerRoleMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] PermissionRecordCustomerRoleMappingDto model)
        {
            var mapping = model.FromDto<PermissionRecordCustomerRoleMapping>();

            await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(mapping);

            return Ok(mapping.ToDto<PermissionRecordCustomerRoleMappingDto>());
        }

        /// <summary>
        /// Delete a permission record-customer role mapping
        /// </summary>
        /// <param name="permissionId">Permission identifier</param>
        /// <param name="customerRoleId">Customer role identifier</param>
        [HttpDelete("{permissionId}/{customerRoleId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int permissionId, int customerRoleId)
        {
            if (permissionId <= 0 || customerRoleId <= 0)
                return BadRequest();

            await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(permissionId, customerRoleId);

            return Ok();
        }

        #endregion
    }
}
