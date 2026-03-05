
using AnqIntegrationApi.Services.Outbox;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnqIntegrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    [Authorize(Roles = "Admin")]
    public class OutboxDebugController : ControllerBase
    {
        private readonly IOutboxProcessor _processor;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<OutboxDebugController> _logger;

        public OutboxDebugController(IOutboxProcessor processor, IWebHostEnvironment env, ILogger<OutboxDebugController> logger)
        {
            _processor = processor;
            _env = env;
            _logger = logger;
        }

        /// <summary>Process the outbox once (manual trigger). Disabled outside Development by default.</summary>
        [HttpPost("process")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Process([FromQuery] int max = 50, CancellationToken ct = default)
        {
            if (!_env.IsDevelopment())
                return Forbid();

            max = Math.Clamp(max, 1, 500);
            var count = await _processor.ProcessBatchAsync(max, ct);
            _logger.LogInformation("Manual outbox process: {Count} messages processed.", count);
            return Ok(new { processed = count, max });
        }
    }
}
