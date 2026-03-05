using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.WebApi.Backend.Models;
using Nop.Plugin.Misc.WebApi.Backend.Services;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Framework.Controllers;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Models;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers
{
    [Route("api-backend/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "backend_" + WebApiCommonDefaults.API_VERSION)]
    public partial class AuthenticateController : BaseNopWebApiController
    {
        #region Fields

        private readonly IAuthorizationAdminService _authorizationAdminService;

        #endregion

        #region Ctor

        public AuthenticateController(
            IAuthorizationAdminService authorizationAdminService)
        {
            _authorizationAdminService = authorizationAdminService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Authenticate admin
        /// </summary>
        [Authorize(true)]
        [HttpPost]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetToken([FromBody] AuthenticateAdminRequest request)
        {
            var response = await _authorizationAdminService.AuthenticateAsync(request);

            if (response == null)
                throw new NopException("Username or password is incorrect");

            return Ok(response);
        }

        #endregion
    }
}
