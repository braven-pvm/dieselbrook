using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public partial class CampaignController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICampaignService _campaignService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;

        #endregion

        #region Ctor

        public CampaignController(ICampaignService campaignService,
            IEmailAccountService emailAccountService,
            INewsLetterSubscriptionService newsLetterSubscriptionService)
        {
            _campaignService = campaignService;
            _emailAccountService = emailAccountService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a campaign
        /// </summary>
        /// <param name="model">Campaign Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CampaignDto model)
        {
            var campaign = model.FromDto<Campaign>();

            await _campaignService.InsertCampaignAsync(campaign);

            return Ok(campaign.ToDto<CampaignDto>());
        }

        /// <summary>
        /// Update a campaign
        /// </summary>
        /// <param name="model">Campaign Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CampaignDto model)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(model.Id);

            if (campaign == null)
                return NotFound("Campaign is not found");

            campaign = model.FromDto<Campaign>();
            await _campaignService.UpdateCampaignAsync(campaign);

            return Ok();
        }

        /// <summary>
        /// Delete a campaign
        /// </summary>
        /// <param name="id">Campaign identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var campaign = await _campaignService.GetCampaignByIdAsync(id);

            if (campaign == null)
                return NotFound($"Campaign Id={id} not found");

            await _campaignService.DeleteCampaignAsync(campaign);

            return Ok();
        }

        /// <summary>
        /// Gets a campaign by identifier
        /// </summary>
        /// <param name="id">The campaign identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var campaign = await _campaignService.GetCampaignByIdAsync(id);

            if (campaign == null)
            {
                return NotFound($"Campaign Id={id} not found");
            }

            return Ok(campaign.ToDto<CampaignDto>());
        }

        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<CampaignDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int storeId = 0)
        {
            if (storeId < 0)
                return BadRequest();

            var campaigns = await _campaignService.GetAllCampaignsAsync(storeId);
            var campaignDtos = campaigns.Select(campaign => campaign.ToDto<CampaignDto>()).ToList();

            return Ok(campaignDtos);
        }

        /// <summary>
        /// Sends a campaign to specified emails
        /// </summary>
        /// <param name="id">The campaign identifier</param>
        /// <param name="emailAccountId">Email account identifier</param>
        /// <param name="subscriptionIds">Subscription identifiers (separator - ;)</param>
        [HttpGet("{id}/{emailAccountId}/{subscriptionIds}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SendCampaignToEmails(int id, int emailAccountId, string subscriptionIds)
        {
            if (id <= 0 || emailAccountId <= 0 || string.IsNullOrEmpty(subscriptionIds))
                return BadRequest();

            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
                return NotFound($"Campaign Id={id} not found");

            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId);
            if (emailAccount == null)
                return NotFound($"Email account Id={emailAccountId} not found");

            var subscriptionsIds = subscriptionIds.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var newsLetterSubscriptions = await subscriptionsIds.SelectAwait(async subscription => await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByIdAsync(subscription)).ToListAsync();

            var result = await _campaignService.SendCampaignAsync(campaign, emailAccount, newsLetterSubscriptions);

            return Ok(result);
        }

        /// <summary>
        /// Sends a campaign to specified email
        /// </summary>
        /// <param name="id">The campaign identifier</param>
        /// <param name="emailAccountId">Email account identifier</param>
        /// <param name="email">Email</param>
        [HttpGet("{id}/{emailAccountId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SendCampaignToEmail(int id, int emailAccountId, [FromQuery][Required] string email)
        {
            if (id <= 0 || emailAccountId <= 0 || string.IsNullOrEmpty(email))
                return BadRequest();

            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
                return NotFound($"Campaign Id={id} not found");

            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId);
            if (emailAccount == null)
                return NotFound($"Email account Id={emailAccountId} not found");

            await _campaignService.SendCampaignAsync(campaign, emailAccount, email);

            return Ok();
        }

        #endregion
    }
}
