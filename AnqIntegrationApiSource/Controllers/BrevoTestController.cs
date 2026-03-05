using Microsoft.AspNetCore.Mvc;
using BrevoApiHelpers.Models;
using BrevoApiHelpers.Services;
using Microsoft.AspNetCore.Authorization;

namespace AnqIntegrationApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "internal")]
    [Route("api/[controller]")]
    public class BrevoTestController : ControllerBase
    {
        private readonly IMessagingService _messagingService;
        private readonly IContactService _contactService;

        public BrevoTestController(IMessagingService messagingService, IContactService contactService)
        {
            _messagingService = messagingService;
            _contactService = contactService;
        }

        /// <summary>
        /// Test Brevo transactional email using SendEmailRequest input.
        /// </summary>
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To) || request.TemplateId <= 0)
                return BadRequest("Recipient email and template ID are required.");

            var result = await _messagingService.SendTransactionalEmailAsync(
                request.To,
                request.TemplateId,
                request.Parameters ?? new Dictionary<string, object>());

            return StatusCode(result.StatusCode, new
            {
                result.Success,
                result.MessageId,
                result.StatusCode,
                result.Error,
                result.RawResponseBody
            });
        }

        /// <summary>
        /// Test Brevo WhatsApp message using SendWhatsappRequest input.
        /// </summary>
        [HttpPost("send-whatsapp")]
        public async Task<IActionResult> SendWhatsapp([FromBody] SendWhatsappRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Number) || request.TemplateId <= 0)
                return BadRequest("WhatsApp number and template ID are required.");

            var result = await _messagingService.SendWhatsappTemplateAsync(
                request.Number,
                request.TemplateId,
                request.Parameters ?? new Dictionary<string, object>());

            return StatusCode(result.StatusCode, new
            {
                result.Success,
                result.MessageId,
                result.StatusCode,
                result.Error,
                result.RawResponseBody
            });
        }

        /// <summary>
        /// Test Brevo AddContact using AddContactRequest input.
        /// </summary>
        [HttpPost("add-contact")]
        public async Task<IActionResult> AddContact([FromBody] AddContactRequest request)
        {
            if (request.Contact == null || string.IsNullOrWhiteSpace(request.Contact.Email))
                return BadRequest("Valid contact with email is required.");

            var result = await _contactService.AddContactAsync(
                request.Contact,
                request.ListIds ?? new List<int>());

            return StatusCode(result.StatusCode, new
            {
                result.Success,
                result.StatusCode,
                result.Error,
                result.RawResponseBody
            });
        }
    }
}
