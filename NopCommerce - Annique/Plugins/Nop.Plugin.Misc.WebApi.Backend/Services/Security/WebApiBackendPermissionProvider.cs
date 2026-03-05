using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.WebApi.Backend.Services.Security
{
    /// <summary>
    /// Web API Backend permission provider
    /// </summary>
    public partial class WebApiBackendPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord AccessWebApiBackend = new ()
        {
            Name = "Access Web API Backend", SystemName = "AccessWebApi", Category = "Standard"
        };

        /// <summary>
        /// Get permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                AccessWebApiBackend
            };
        }

        /// <summary>
        /// Get default permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new () { (NopCustomerDefaults.AdministratorsRoleName, new [] { AccessWebApiBackend }) };
        }
    }
}
