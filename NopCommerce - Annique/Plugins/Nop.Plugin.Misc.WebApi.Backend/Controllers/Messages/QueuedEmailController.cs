using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Messages;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Messages
{
    public partial class QueuedEmailController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IQueuedEmailService _queuedEmailService;

        #endregion

        #region Ctor

        public QueuedEmailController(IQueuedEmailService queuedEmailService)
        {
            _queuedEmailService = queuedEmailService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a queued email by identifier
        /// </summary>
        /// <param name="id">Queued email identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(QueuedEmailDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var queuedEmail = await _queuedEmailService.GetQueuedEmailByIdAsync(id);

            if (queuedEmail == null)
            {
                return NotFound($"Queued email Id={id} not found");
            }

            return Ok(queuedEmail.ToDto<QueuedEmailDto>());
        }

        /// <summary>
        /// Get queued email by identifiers
        /// </summary>
        /// <param name="ids">Array of queued email identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<QueuedEmailDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByIds(string ids)
        {
            var queuedEmailIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var queuedEmails = await _queuedEmailService.GetQueuedEmailsByIdsAsync(queuedEmailIds);

            var queuedEmailDtos = queuedEmails.Select(queuedEmail => queuedEmail.ToDto<QueuedEmailDto>()).ToList();

            return Ok(queuedEmailDtos);
        }

        /// <summary>
        /// Create a queued email
        /// </summary>
        /// <param name="model">Queued email Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(QueuedEmailDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] QueuedEmailDto model)
        {
            var queuedEmail = model.FromDto<QueuedEmail>();

            await _queuedEmailService.InsertQueuedEmailAsync(queuedEmail);

            return Ok(queuedEmail.ToDto<QueuedEmailDto>());
        }

        /// <summary>
        /// Update a queued email
        /// </summary>
        /// <param name="model">Queued email Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] QueuedEmailDto model)
        {
            var queuedEmail = await _queuedEmailService.GetQueuedEmailByIdAsync(model.Id);

            if (queuedEmail == null)
                return NotFound("Queued email is not found");

            queuedEmail = model.FromDto<QueuedEmail>();
            await _queuedEmailService.UpdateQueuedEmailAsync(queuedEmail);

            return Ok();
        }

        /// <summary>
        /// Delete a queued email
        /// </summary>
        /// <param name="id">Queued email identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var queuedEmail = await _queuedEmailService.GetQueuedEmailByIdAsync(id);

            if (queuedEmail == null)
                return NotFound($"Queued email Id={id} not found");

            await _queuedEmailService.DeleteQueuedEmailAsync(queuedEmail);

            return Ok();
        }

        /// <summary>
        /// Delete queued emails by identifiers
        /// </summary>
        /// <param name="ids">Array of queued email identifiers (separator - ;)</param>
        [HttpDelete("{ids}")]
        public virtual async Task<IActionResult> DeleteByIds(string ids)
        {
            var queuedEmailIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var queuedEmails = await _queuedEmailService.GetQueuedEmailsByIdsAsync(queuedEmailIds);
            
            await _queuedEmailService.DeleteQueuedEmailsAsync(queuedEmails);

            return Ok();
        }

        /// <summary>
        /// Delete all queued emails
        /// </summary>
        [HttpDelete]
        public virtual async Task<IActionResult> DeleteAll()
        {
            await _queuedEmailService.DeleteAllEmailsAsync();

            return Ok();
        }

        /// <summary>
        /// Delete all queued emails
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        [HttpDelete]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> DeleteAlreadySentEmails([FromQuery] DateTime? createdFromUtc, [FromQuery] DateTime? createdToUtc)
        {
            var result = await _queuedEmailService.DeleteAlreadySentEmailsAsync(createdFromUtc, createdToUtc);

            return Ok(result);
        }

        /// <summary>
        /// Gets all queued emails
        /// </summary>
        /// <param name="fromEmail">From Email</param>
        /// <param name="toEmail">To Email</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="loadNotSentItemsOnly">A value indicating whether to load only not sent emails</param>
        /// <param name="loadOnlyItemsToBeSent">A value indicating whether to load only emails for ready to be sent</param>
        /// <param name="maxSendTries">Maximum send tries</param>
        /// <param name="loadNewest">A value indicating whether we should sort queued email descending; otherwise, ascending.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<QueuedEmail, QueuedEmailDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SearchEmails([FromQuery, Required] string fromEmail,
            [FromQuery, Required] string toEmail,
            [FromQuery] DateTime? createdFromUtc,
            [FromQuery] DateTime? createdToUtc,
            [FromQuery, Required] bool loadNotSentItemsOnly,
            [FromQuery, Required] bool loadOnlyItemsToBeSent,
            [FromQuery, Required] int maxSendTries,
            [FromQuery, Required] bool loadNewest,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var queuedEmails = await _queuedEmailService.SearchEmailsAsync(fromEmail, toEmail,
                createdFromUtc, createdToUtc,
                loadNotSentItemsOnly, loadOnlyItemsToBeSent, maxSendTries, loadNewest,
                pageIndex, pageSize);

            var pagedListDto = queuedEmails.ToPagedListDto<QueuedEmail, QueuedEmailDto>();

            return Ok(pagedListDto);
        }

        #endregion
    }
}
