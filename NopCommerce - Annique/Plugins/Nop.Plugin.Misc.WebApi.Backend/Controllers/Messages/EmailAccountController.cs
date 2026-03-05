using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Messages;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Messages
{
    public partial class EmailAccountController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IEmailAccountService _emailAccountService;

        #endregion

        #region Ctor

        public EmailAccountController(IEmailAccountService emailAccountService)
        {
            _emailAccountService = emailAccountService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a email account
        /// </summary>
        /// <param name="model">Email account Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(EmailAccountDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] MessageTemplateDto model)
        {
            var emailAccount = model.FromDto<EmailAccount>();

            await _emailAccountService.InsertEmailAccountAsync(emailAccount);

            return Ok(emailAccount.ToDto<EmailAccountDto>());
        }

        /// <summary>
        /// Update a email account
        /// </summary>
        /// <param name="model">Email account Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] EmailAccountDto model)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(model.Id);

            if (emailAccount == null)
                return NotFound("Email account is not found");

            emailAccount = model.FromDto<EmailAccount>();
            await _emailAccountService.UpdateEmailAccountAsync(emailAccount);

            return Ok();
        }

        /// <summary>
        /// Delete a email account
        /// </summary>
        /// <param name="id">Email account identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(id);

            if (emailAccount == null)
                return NotFound($"Email account Id={id} not found");

            await _emailAccountService.DeleteEmailAccountAsync(emailAccount);

            return Ok();
        }

        /// <summary>
        /// Gets a email account by identifier
        /// </summary>
        /// <param name="id">The email account identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EmailAccountDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(id);

            if (emailAccount == null)
            {
                return NotFound($"Email account Id={id} not found");
            }

            return Ok(emailAccount.ToDto<EmailAccountDto>());
        }

        /// <summary>
        /// Gets all email accounts
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<EmailAccountDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var emailAccounts = await _emailAccountService.GetAllEmailAccountsAsync();
            var emailAccountDtos = emailAccounts.Select(emailAccount => emailAccount.ToDto<EmailAccountDto>()).ToList();

            return Ok(emailAccountDtos);
        }

        #endregion
    }
}
