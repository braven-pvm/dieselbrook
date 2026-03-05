using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Gdpr;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Gdpr;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Gdpr;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Gdpr
{
    public partial class GdprConsentController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IGdprService _gdprService;

        #endregion

        #region Ctor

        public GdprConsentController(IGdprService gdprService)
        {
            _gdprService = gdprService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a GDPR consent by identifier
        /// </summary>
        /// <param name="id">The GDPR consent identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GdprConsentDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var gdprConsent = await _gdprService.GetConsentByIdAsync(id);

            if (gdprConsent == null)
            {
                return NotFound($"GDPR consent Id={id} not found");
            }

            return Ok(gdprConsent.ToDto<GdprConsentDto>());
        }

        /// <summary>
        /// Gets all GDPR consents
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<GdprConsentDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var gdprConsents = await _gdprService.GetAllConsentsAsync();
            var gdprConsentDtos = gdprConsents.Select(gdprConsent => gdprConsent.ToDto<GdprConsentDto>()).ToList();

            return Ok(gdprConsentDtos);
        }

        /// <summary>
        /// Create a GDPR consent
        /// </summary>
        /// <param name="model">GDPR consent Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(GdprConsentDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] GdprConsentDto model)
        {
            var gdprConsent = model.FromDto<GdprConsent>();

            await _gdprService.InsertConsentAsync(gdprConsent);

            return Ok(gdprConsent.ToDto<GdprConsentDto>());
        }

        /// <summary>
        /// Update a GDPR consent
        /// </summary>
        /// <param name="model">GDPR consent Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] GdprConsentDto model)
        {
            var gdprConsent = await _gdprService.GetConsentByIdAsync(model.Id);

            if (gdprConsent == null)
                return NotFound("GDPR consent is not found");

            gdprConsent = model.FromDto<GdprConsent>();
            await _gdprService.UpdateConsentAsync(gdprConsent);

            return Ok();
        }

        /// <summary>
        /// Delete a GDPR consent
        /// </summary>
        /// <param name="id">GDPR consent identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var gdprConsent = await _gdprService.GetConsentByIdAsync(id);

            if (gdprConsent == null)
                return NotFound($"GDPR consent Id={id} not found");

            await _gdprService.DeleteConsentAsync(gdprConsent);

            return Ok();
        }

        /// <summary>
        /// Gets the latest selected value (a consent is accepted or not by a customer)
        /// </summary>
        /// <param name="consentId">Consent identifier</param>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{consentId}/{customerId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool?), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsConsentAccepted(int consentId, int customerId)
        {
            if (consentId <= 0 || customerId <= 0)
                return BadRequest();

            var result = await _gdprService.IsConsentAcceptedAsync(consentId, customerId);

            return Ok(result);
        }

        #endregion
    }
}
