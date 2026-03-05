using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Framework;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    [ApiController]
    [Route("swagger-frontend/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "Swagger")]
    [Produces("application/json")]
    public partial class CommonInfoController : ControllerBase
    {
        /// <summary>
        /// Gets swagger doc version API
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult GetSwaggerDocVersion()
        {
            return Ok(WebApiCommonDefaults.API_VERSION);
        }
    }
}
