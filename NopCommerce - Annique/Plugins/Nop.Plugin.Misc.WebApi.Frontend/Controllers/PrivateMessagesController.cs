using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.PrivateMessages;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.ReturnRequests;
using Nop.Services.Customers;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class PrivateMessagesController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly ForumSettings _forumSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly IPrivateMessagesModelFactory _privateMessagesModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PrivateMessagesController(ForumSettings forumSettings,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IForumService forumService,
            ILocalizationService localizationService,
            IPrivateMessagesModelFactory privateMessagesModelFactory,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _forumSettings = forumSettings;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _forumService = forumService;
            _localizationService = localizationService;
            _privateMessagesModelFactory = privateMessagesModelFactory;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(typeof(PrivateMessageIndexModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Index([FromQuery, Required] string tab,
            [FromQuery] int? pageNumber)
        {
            if (!_forumSettings.AllowPrivateMessages)
                return BadRequest();

            var model = await _privateMessagesModelFactory.PreparePrivateMessageIndexModelAsync(pageNumber, tab);

            return Ok(model.ToDto<PrivateMessageIndexModelDto>());
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteInboxPM([FromBody] IDictionary<string, string> formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("pm", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("pm", "").Trim();
                    if (int.TryParse(id, out var privateMessageId))
                    {
                        var customer = await _workContext.GetCurrentCustomerAsync();

                        var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
                        if (pm != null)
                            if (pm.ToCustomerId == customer.Id)
                            {
                                pm.IsDeletedByRecipient = true;
                                await _forumService.UpdatePrivateMessageAsync(pm);
                            }
                    }
                }
            }

            return Ok();
        }

        [HttpPost]
        public virtual async Task<IActionResult> MarkUnread([FromBody] IDictionary<string, string> formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("pm", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("pm", "").Trim();
                    if (int.TryParse(id, out var privateMessageId))
                    {
                        var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
                        var customer = await _workContext.GetCurrentCustomerAsync();

                        if (pm != null)
                            if (pm.ToCustomerId == customer.Id)
                            {
                                pm.IsRead = false;
                                await _forumService.UpdatePrivateMessageAsync(pm);
                            }
                    }
                }
            }

            return Ok();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSentPM([FromBody] IDictionary<string, string> formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("si", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("si", "").Trim();
                    if (int.TryParse(id, out var privateMessageId))
                    {
                        var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
                        var customer = await _workContext.GetCurrentCustomerAsync();

                        if (pm != null)
                            if (pm.FromCustomerId == customer.Id)
                            {
                                pm.IsDeletedByAuthor = true;
                                await _forumService.UpdatePrivateMessageAsync(pm);
                            }
                    }
                }
            }

            return Ok();
        }

        [HttpGet("{toCustomerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SendPrivateMessageModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SendPM(int toCustomerId, [FromQuery] int? replyToMessageId)
        {
            if (!_forumSettings.AllowPrivateMessages)
                return NotFound($"The setting {nameof(_forumSettings.AllowPrivateMessages)} is not enabled.");
            
            var customerTo = await _customerService.GetCustomerByIdAsync(toCustomerId);
            if (customerTo == null || await _customerService.IsGuestAsync(customerTo))
                return NotFound($"Customer by id={toCustomerId} not found or customer is guest.");

            PrivateMessage replyToPM = null;
            if (replyToMessageId.HasValue)
            //reply to a previous PM
                replyToPM = await _forumService.GetPrivateMessageByIdAsync(replyToMessageId.Value);

            var model = await _privateMessagesModelFactory.PrepareSendPrivateMessageModelAsync(customerTo, replyToPM);
            
            return Ok(model.ToDto<SendPrivateMessageModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SendPrivateMessageModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SendPM([FromBody] SendPrivateMessageModelDto model)
        {
            if (!_forumSettings.AllowPrivateMessages)
                return BadRequest();

            Customer toCustomer;
            var replyToPM = await _forumService.GetPrivateMessageByIdAsync(model.ReplyToMessageId);
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (replyToPM != null)
            {
                //reply to a previous PM
                if (replyToPM.ToCustomerId == currentCustomer.Id || replyToPM.FromCustomerId == currentCustomer.Id)
                    //Reply to already sent PM (by current customer) should not be sent to yourself
                    toCustomer = await _customerService.GetCustomerByIdAsync(replyToPM.FromCustomerId == currentCustomer.Id
                        ? replyToPM.ToCustomerId
                        : replyToPM.FromCustomerId);
                else
                    return BadRequest();
            }
            else
                //first PM
                toCustomer = await _customerService.GetCustomerByIdAsync(model.ToCustomerId);

            if (toCustomer == null || await _customerService.IsGuestAsync(toCustomer))
                return NotFound($"Customer by id={model.ToCustomerId} not found or customer is guest.");
            
            try
            {
                var subject = model.Subject;
                if (_forumSettings.PMSubjectMaxLength > 0 && subject.Length > _forumSettings.PMSubjectMaxLength) 
                    subject = subject[0.._forumSettings.PMSubjectMaxLength];

                var text = model.Message;
                if (_forumSettings.PMTextMaxLength > 0 && text.Length > _forumSettings.PMTextMaxLength) 
                    text = text[0.._forumSettings.PMTextMaxLength];

                var nowUtc = DateTime.UtcNow;
                var store = await _storeContext.GetCurrentStoreAsync();

                var privateMessage = new PrivateMessage
                {
                    StoreId = store.Id,
                    ToCustomerId = toCustomer.Id,
                    FromCustomerId = currentCustomer.Id,
                    Subject = subject,
                    Text = text,
                    IsDeletedByAuthor = false,
                    IsDeletedByRecipient = false,
                    IsRead = false,
                    CreatedOnUtc = nowUtc
                };

                await _forumService.InsertPrivateMessageAsync(privateMessage);

                //activity log
                await _customerActivityService.InsertActivityAsync("PublicStore.SendPM",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.SendPM"), toCustomer.Email), toCustomer);

                var responseModel = await _privateMessagesModelFactory.PrepareSendPrivateMessageModelAsync(toCustomer, replyToPM);

                return Ok(responseModel.ToDto<SendPrivateMessageModelDto>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{privateMessageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PrivateMessageModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ViewPM(int privateMessageId)
        {
            if (!_forumSettings.AllowPrivateMessages)
                return BadRequest();
            
            var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (pm != null)
            {
                if (pm.ToCustomerId != customer.Id && pm.FromCustomerId != customer.Id) 
                    return BadRequest();

                if (!pm.IsRead && pm.ToCustomerId == customer.Id)
                {
                    pm.IsRead = true;
                    await _forumService.UpdatePrivateMessageAsync(pm);
                }
            }
            else
                return NotFound($"Private message by id={privateMessageId} not found.");

            var model = await _privateMessagesModelFactory.PreparePrivateMessageModelAsync(pm);

            return Ok(model.ToDto<PrivateMessageModelDto>());
        }

        [HttpDelete("{privateMessageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> DeletePM(int privateMessageId)
        {
            if (!_forumSettings.AllowPrivateMessages)
                return BadRequest();
            
            var pm = await _forumService.GetPrivateMessageByIdAsync(privateMessageId);
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (pm != null)
            {
                if (pm.FromCustomerId == customer.Id)
                {
                    pm.IsDeletedByAuthor = true;
                    await _forumService.UpdatePrivateMessageAsync(pm);
                }

                if (pm.ToCustomerId == customer.Id)
                {
                    pm.IsDeletedByRecipient = true;
                    await _forumService.UpdatePrivateMessageAsync(pm);
                }
            }

            return Ok();
        }

        #endregion
    }
}