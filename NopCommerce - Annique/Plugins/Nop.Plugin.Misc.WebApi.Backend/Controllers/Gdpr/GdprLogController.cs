using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Gdpr;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Gdpr;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Services.Gdpr;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Gdpr
{
    public partial class GdprLogController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IGdprService _gdprService;

        #endregion

        #region Ctor

        public GdprLogController(IGdprService gdprService)
        {
            _gdprService = gdprService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all GDPR log records
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="consentId">Consent identifier</param>
        /// <param name="customerInfo">Customer info (Exact match)</param>
        /// <param name="requestType">GDPR request type</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<GdprLog, GdprLogDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int customerId = 0,
            [FromQuery] int consentId = 0,
            [FromQuery] string customerInfo = "",
            [FromQuery] GdprRequestType? requestType = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var gdprLogs = await _gdprService.GetAllLogAsync(customerId, consentId, customerInfo, requestType,
                pageIndex, pageSize);

            return Ok(gdprLogs.ToPagedListDto<GdprLog, GdprLogDto>());
        }

        #endregion
    }
}
