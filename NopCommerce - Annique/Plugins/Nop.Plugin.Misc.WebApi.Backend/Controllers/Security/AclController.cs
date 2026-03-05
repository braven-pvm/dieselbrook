using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Security;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Security
{
    public partial class AclController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICustomerService _customerService;
        private readonly IRepository<AclRecord> _aclRecordRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public AclController(IAclService aclService,
            CatalogSettings catalogSettings,
            ICustomerService customerService,
            IRepository<AclRecord> aclRecordRepository,
            IStaticCacheManager staticCacheManager)
        {
            _aclService = aclService;
            _catalogSettings = catalogSettings;
            _customerService = customerService;
            _aclRecordRepository = aclRecordRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        protected async Task<IList<int>> GetRoleIdsWithAccess(int entityId, string entityTypeName)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopSecurityDefaults.AclRecordCacheKey, entityId, entityTypeName);

            var query = from ur in _aclRecordRepository.Table
                where ur.EntityId == entityId &&
                      ur.EntityName == entityTypeName
                select ur.CustomerRoleId;

            var ids = await _staticCacheManager.GetAsync(key, () => query.ToArray());

            return ids.ToList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes an ACL record
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var aclRecord = await _aclRecordRepository.GetByIdAsync(id);

            if (aclRecord == null)
                return NotFound($"Acl record Id={id} not found");

            await _aclService.DeleteAclRecordAsync(aclRecord);

            return Ok();
        }

        // TODO: move logic to service
        /// <summary>
        /// Gets ACL records
        /// </summary>
        [HttpGet("{entityId}")]
        [ProducesResponseType(typeof(IList<AclRecordDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAclRecords(int entityId, [FromQuery][Required] string entityTypeName)
        {
            var entityName = entityTypeName;

            var query = from ur in _aclRecordRepository.Table
                where ur.EntityId == entityId &&
                      ur.EntityName == entityName
                select ur;
            var aclRecords = await query.ToListAsync();
            var aclRecordsDto = aclRecords.Select(r => r.ToDto<AclRecordDto>()).ToList();

            return Ok(aclRecordsDto);
        }

        // TODO: move logic to service
        /// <summary>
        /// Inserts an ACL record
        /// </summary>
        /// <param name="customerRoleId">Customer role Id</param>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityTypeName">Entity type name</param>
        [HttpPost("{customerRoleId}/{entityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> InsertAclRecord(int customerRoleId,
            int entityId,
            [FromQuery, Required] string entityTypeName)
        {
            if (customerRoleId == 0)
                return BadRequest();

            var aclRecord = new AclRecord
            {
                EntityId = entityId, EntityName = entityTypeName, CustomerRoleId = customerRoleId
            };

            await _aclRecordRepository.InsertAsync(aclRecord);

            return Ok();
        }

        // TODO: move logic to service
        /// <summary>
        /// Find customer role identifiers with granted access
        /// </summary>
        [HttpGet("{entityId}")]
        [ProducesResponseType(typeof(IList<int>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerRoleIdsWithAccess(int entityId, [FromQuery][Required] string entityTypeName)
        {
            return Ok(await GetRoleIdsWithAccess(entityId, entityTypeName));
        }

        // TODO: move logic to service
        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityTypeName">Entity type name</param>
        /// <param name="subjectToAcl">Is entity subject to ACL</param>
        [HttpPost("{customerId}/{entityId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Authorize(int customerId,
            int entityId,
            [FromQuery, Required] string entityTypeName,
            [FromQuery, Required] bool subjectToAcl)
        {
            if (entityId <= 0 || customerId <= 0)
                return BadRequest();

            if (_catalogSettings.IgnoreAcl)
                return Ok(true);

            if (!subjectToAcl)
                return Ok(true);

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return BadRequest();

            foreach (var role1 in await _customerService.GetCustomerRolesAsync(customer))
            foreach (var role2Id in await GetRoleIdsWithAccess(entityId, entityTypeName))
                if (role1.Id == role2Id)
                    //yes, we have such permission
                    return Ok(true);

            //no permission found
            return Ok(false);
        }
    }

    #endregion
}
