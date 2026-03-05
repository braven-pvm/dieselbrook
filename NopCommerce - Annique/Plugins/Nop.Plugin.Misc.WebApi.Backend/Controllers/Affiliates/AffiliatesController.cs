using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Affiliates;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Affiliates;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Affiliates;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Affiliates
{
    public partial class AffiliatesController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IAffiliateService _affiliateService;

        #endregion

        #region Ctor

        public AffiliatesController(IAffiliateService affiliateService)
        {
            _affiliateService = affiliateService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all affiliates
        /// </summary>
        /// <param name="friendlyUrlName">Friendly URL name; null to load all records</param>
        /// <param name="firstName">First name; null to load all records</param>
        /// <param name="lastName">Last name; null to load all records</param>
        /// <param name="loadOnlyWithOrders">Value indicating whether to load affiliates only with orders placed (by affiliated customers)</param>
        /// <param name="ordersCreatedFromUtc">Orders created date from (UTC); null to load all records. It's used only with "loadOnlyWithOrders" parameter st to "true".</param>
        /// <param name="ordersCreatedToUtc">Orders created date to (UTC); null to load all records. It's used only with "loadOnlyWithOrders" parameter st to "true".</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<AffiliateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] string friendlyUrlName = null,
            [FromQuery] string firstName = null,
            [FromQuery] string lastName = null,
            [FromQuery] bool loadOnlyWithOrders = false,
            [FromQuery] DateTime? ordersCreatedFromUtc = null,
            [FromQuery] DateTime? ordersCreatedToUtc = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false)
        {
            var affiliates = await _affiliateService.GetAllAffiliatesAsync(friendlyUrlName, firstName, lastName,
                loadOnlyWithOrders, ordersCreatedFromUtc, ordersCreatedToUtc, pageIndex, pageSize, showHidden);

            var affiliatesDto = affiliates.Select(affiliate => affiliate.ToDto<AffiliateDto>()).ToList();

            return Ok(affiliatesDto);
        }

        /// <summary>
        /// Gets an affiliate
        /// </summary>
        /// <param name="id">Affiliate identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AffiliateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var affiliate = await _affiliateService.GetAffiliateByIdAsync(id);

            if (affiliate == null)
                return NotFound($"Affiliates Id={id} not found");

            var affiliateDto = affiliate.ToDto<AffiliateDto>();

            return Ok(affiliateDto);
        }

        /// <summary>
        /// Gets an affiliate
        /// </summary>
        /// <param name="friendlyUrlName">Affiliate friendly url name</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AffiliateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByFriendlyUrlName([FromQuery][Required] string friendlyUrlName)
        {
            var affiliate = await _affiliateService.GetAffiliateByFriendlyUrlNameAsync(friendlyUrlName);

            if (affiliate == null)
                return NotFound($"Affiliates friendlyUrlName={friendlyUrlName} not found");

            var affiliateDto = affiliate.ToDto<AffiliateDto>();

            return Ok(affiliateDto);
        }

        /// <summary>
        /// Create affiliate
        /// </summary>
        /// <param name="model">Affiliate Dto</param>
        [HttpPost]
        [ProducesResponseType(typeof(AffiliateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] AffiliateDto model)
        {
            var affiliate = model.FromDto<Affiliate>();

            await _affiliateService.InsertAffiliateAsync(affiliate);

            var affiliateDto = affiliate.ToDto<AffiliateDto>();

            return Ok(affiliateDto);
        }

        /// <summary>
        /// Update affiliate by Id
        /// </summary>
        /// <param name="model">Affiliate Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] AffiliateDto model)
        {
            var affiliate = await _affiliateService.GetAffiliateByIdAsync(model.Id);

            if (affiliate == null)
                return NotFound($"Affiliate Id={model.Id} is not found");

            affiliate = model.FromDto<Affiliate>();

            await _affiliateService.UpdateAffiliateAsync(affiliate);

            return Ok();
        }

        /// <summary>
        /// Delete affiliate
        /// </summary>
        /// <param name="id">Affiliate identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var affiliate = await _affiliateService.GetAffiliateByIdAsync(id);

            if (affiliate == null)
                return NotFound($"Affiliate Id={id} not found");

            await _affiliateService.DeleteAffiliateAsync(affiliate);

            return Ok();
        }

        /// <summary>
        /// Get full name
        /// </summary>
        /// <param name="affiliateId">Affiliate Id</param>
        [HttpGet("{affiliateId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAffiliateFullName(int affiliateId)
        {
            if (affiliateId <= 0)
                return BadRequest();

            var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateId);

            if (affiliate == null)
                return NotFound($"Affiliate Id={affiliateId} not found");

            var affiliateFullName = await _affiliateService.GetAffiliateFullNameAsync(affiliate);

            return Ok(affiliateFullName);
        }

        /// <summary>
        /// Generate affiliate URL
        /// </summary>
        /// <param name="affiliateId">Affiliate Id</param>
        [HttpGet("{affiliateId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GenerateUrl(int affiliateId)
        {
            if (affiliateId <= 0)
                return BadRequest();

            var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateId);

            if (affiliate == null)
                return NotFound($"Affiliate Id={affiliateId} not found");

            var affiliateUrl = await _affiliateService.GenerateUrlAsync(affiliate);

            return Ok(affiliateUrl);
        }

        /// <summary>
        /// Validate friendly URL name
        /// </summary>
        /// <param name="affiliateId">Affiliate Id</param>
        /// <param name="friendlyUrlName">Friendly URL name</param>
        [HttpGet("{affiliateId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ValidateFriendlyUrlName(int affiliateId, [FromQuery][Required] string friendlyUrlName)
        {
            if (affiliateId <= 0)
                return BadRequest();

            var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateId);

            if (affiliate == null)
                return NotFound($"Affiliate Id={affiliateId} not found");

            var affiliateUrl = _affiliateService.ValidateFriendlyUrlNameAsync(affiliate, friendlyUrlName);

            return Ok(affiliateUrl);
        }

        #endregion
    }
}
