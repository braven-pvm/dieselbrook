using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Media;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Media;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Media
{
    public partial class DownloadController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IDownloadService _downloadService;

        #endregion

        #region Ctor

        public DownloadController(IDownloadService downloadService)
        {
            _downloadService = downloadService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a download by identifier
        /// </summary>
        /// <param name="id">The download identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DownloadDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var download = await _downloadService.GetDownloadByIdAsync(id);

            if (download == null)
            {
                return NotFound($"Download Id={id} not found");
            }

            return Ok(download.ToDto<DownloadDto>());
        }

        /// <summary>
        /// Gets a download by GUID
        /// </summary>
        /// <param name="guid">The download GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DownloadDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByGuid([FromQuery][Required] Guid guid)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(guid);
            if (download == null)
                return NotFound($"Download GUID={guid} not found");

            return Ok(download.ToDto<DownloadDto>());
        }

        /// <summary>
        /// Create a download
        /// </summary>
        /// <param name="model">Download Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(DownloadDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] DownloadDto model)
        {
            var download = model.FromDto<Download>();

            await _downloadService.InsertDownloadAsync(download);

            return Ok(download.ToDto<DownloadDto>());
        }

        /// <summary>
        /// Delete a download
        /// </summary>
        /// <param name="id">Download identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var download = await _downloadService.GetDownloadByIdAsync(id);

            if (download == null)
                return NotFound($"Download Id={id} not found");

            await _downloadService.DeleteDownloadAsync(download);

            return Ok();
        }

        #endregion
    }
}
