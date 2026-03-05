using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Framework.Controllers;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [Area("api-frontend")]
    [Route("api-frontend/[controller]/[action]", Order = int.MaxValue)]
    [ApiExplorerSettings(GroupName = "frontend_" + WebApiCommonDefaults.API_VERSION)]
    public abstract class BaseNopWebApiFrontendController : BaseNopWebApiController
    {
    }
}
