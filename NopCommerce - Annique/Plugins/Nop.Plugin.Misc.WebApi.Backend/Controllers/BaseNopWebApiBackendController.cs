using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Framework.Controllers;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers
{
    [Authorize]
    [CheckAccessWebApi]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [Area("api-backend")]
    [Route("api-backend/[controller]/[action]", Order = int.MaxValue)]
    [ApiExplorerSettings(GroupName = "backend_" + WebApiCommonDefaults.API_VERSION)]
    public abstract class BaseNopWebApiBackendController : BaseNopWebApiController
    {
    }
}
