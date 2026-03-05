using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Framework.Controllers;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Models;
using Nop.Plugin.Misc.WebApi.Frontend.Models;
using Nop.Plugin.Misc.WebApi.Frontend.Services;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    [Route("api-frontend/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "frontend_" + WebApiCommonDefaults.API_VERSION)]
    public partial class AuthenticateController : BaseNopWebApiController
    {
        #region Fields

        private readonly IAuthorizationUserService _authorizationUserService;

        #endregion

        #region Ctor

        public AuthenticateController(
            IAuthorizationUserService authorizationUserService)
        {
            _authorizationUserService = authorizationUserService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Authenticate user
        /// </summary>
        /// <param name="request"></param>
        [Authorize(true)]
        [HttpPost]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetToken([FromBody]AuthenticateCustomerRequest request)
        {
            var response = await _authorizationUserService.AuthenticateAsync(request);

            if (response == null)
                throw new NopException("Username or password is incorrect");

            return Ok(response);
        }

        #endregion
    }
}
