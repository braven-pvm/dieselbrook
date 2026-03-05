using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Newsletter;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class NewsletterController : BaseNopWebApiFrontendController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INewsletterModelFactory _newsletterModelFactory;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;

        public NewsletterController(ILocalizationService localizationService,
            INewsletterModelFactory newsletterModelFactory,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService)
        {
            _localizationService = localizationService;
            _newsletterModelFactory = newsletterModelFactory;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SubscribeNewsletter([FromQuery, Required] string email,
            [FromQuery, Required] bool subscribe)
        {
            string result;

            if (!CommonHelper.IsValidEmail(email))
                return BadRequest(await _localizationService.GetResourceAsync("Newsletter.Email.Wrong"));

            email = email.Trim();

            var store = await _storeContext.GetCurrentStoreAsync();
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            var subscription = await _newsLetterSubscriptionService
                    .GetNewsLetterSubscriptionByEmailAndStoreIdAsync(email, store.Id);
            if (subscription != null)
            {
                if (subscribe)
                {
                    if (!subscription.Active)
                        await _workflowMessageService.SendNewsLetterSubscriptionActivationMessageAsync(subscription, currentLanguage.Id);
                    result = await _localizationService.GetResourceAsync("Newsletter.SubscribeEmailSent");
                }
                else
                {
                    if (subscription.Active)
                        await _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessageAsync(subscription,
                            currentLanguage.Id);
                    result = await _localizationService.GetResourceAsync("Newsletter.UnsubscribeEmailSent");
                }
            }
            else if (subscribe)
            {
                subscription = new NewsLetterSubscription
                {
                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    Email = email,
                    Active = false,
                    StoreId = store.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(subscription);
                await _workflowMessageService.SendNewsLetterSubscriptionActivationMessageAsync(subscription, currentLanguage.Id);

                result = await _localizationService.GetResourceAsync("Newsletter.SubscribeEmailSent");
            }
            else
                result = await _localizationService.GetResourceAsync("Newsletter.UnsubscribeEmailSent");

            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SubscriptionActivationModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SubscriptionActivation([FromQuery, Required] Guid token,
            [FromQuery, Required] bool active)
        {
            var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuidAsync(token);
            if (subscription == null)
                return NotFound($"Subscription token = {token} not found");

            if (active)
            {
                subscription.Active = true;
                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(subscription);
            }
            else
                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);

            var model = await _newsletterModelFactory.PrepareSubscriptionActivationModelAsync(active);

            return Ok(model.ToDto<SubscriptionActivationModelDto>());
        }
    }
}