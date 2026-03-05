using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;

namespace Nop.Plugin.Misc.WebApi.Framework.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public abstract class BaseNopWebApiController : ControllerBase
    {
    }
}
