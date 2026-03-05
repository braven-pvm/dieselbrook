using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Services.Security;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.WebApi.Backend.Helpers
{
    /// <summary>
    /// Represents a filter attribute that confirms access to Web API
    /// </summary>
    public sealed class CheckAccessWebApiAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        /// <param name="ignore">Whether to ignore the execution of filter actions</param>
        public CheckAccessWebApiAttribute(bool ignore = false) : base(typeof(CheckAccessWebApiFilter))
        {
            IgnoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to ignore the execution of filter actions
        /// </summary>
        public bool IgnoreFilter { get; }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter that confirms access to Web API
        /// </summary>
        private class CheckAccessWebApiFilter : IAsyncAuthorizationFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;
            private readonly WebApiCommonSettings _webApiCommonSettings;

            #endregion

            #region Ctor

            public CheckAccessWebApiFilter(bool ignoreFilter, 
                IPermissionService permissionService,
                WebApiCommonSettings webApiCommonSettings)
            {
                _ignoreFilter = ignoreFilter;
                _permissionService = permissionService;
                _webApiCommonSettings = webApiCommonSettings;
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="context">Authorization filter context</param>
            private async Task CheckAccessWebApiAsync(AuthorizationFilterContext context)
            {
                if (context == null)
                    throw new NopException(nameof(context));

                if (!DataSettingsManager.IsDatabaseInstalled())
                    return;

                if (_webApiCommonSettings.DeveloperMode)
                    return;

                //check whether this filter has been overridden for the Action
                var actionFilter = context.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter)
                    .OfType<CheckAccessWebApiAttribute>()
                    .FirstOrDefault();

                //ignore filter (the action is available even if navigation is not allowed)
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;

                //check whether current customer has access to Web API
                if (await _permissionService.AuthorizeAsync(WebApiBackendPermissionProvider.AccessWebApiBackend))
                    return;

                //customer hasn't access to Web API
                context.Result = new JsonResult(new { message = "Access denied." }) { StatusCode = StatusCodes.Status403Forbidden };
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="context">Authorization filter context</param>
            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                await CheckAccessWebApiAsync(context);
            }

            #endregion
        }

        #endregion
    }
}
