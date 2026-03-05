using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Seo;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public class UrlRecordController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public UrlRecordController(IUrlRecordService urlRecordService)
        {
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a URL record by slug
        /// </summary>
        /// <param name="slug">Slug</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UrlRecordDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBySlug([FromQuery][Required] string slug)
        {
            var decodedSlug = HttpUtility.UrlDecode(slug);
            var urlRecord = await _urlRecordService.GetBySlugAsync(decodedSlug);

            if (urlRecord == null)
            {
                return NotFound($"URL record by slug={slug} not found");
            }

            return Ok(urlRecord.ToDto<UrlRecordDto>());
        }

        #endregion
    }
}
