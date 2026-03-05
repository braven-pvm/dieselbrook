using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnqIntegrationApi.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        // GET /health
        [AllowAnonymous]
        [HttpGet("/health")]
        public IActionResult Health() => Ok(new { status = "ok" });
    }
}
