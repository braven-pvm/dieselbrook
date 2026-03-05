using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Media;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Media
{
    public partial class PictureBinaryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public PictureBinaryController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a product picture binary by picture identifier
        /// </summary>
        /// <param name="id">The picture identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PictureBinaryDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByPictureId(int id)
        {
            if (id <= 0)
                return BadRequest();

            var pictureBinary = await _pictureService.GetPictureBinaryByPictureIdAsync(id);

            if (pictureBinary == null)
            {
                return NotFound($"Picture binary by picture Id={id} not found");
            }

            return Ok(pictureBinary.ToDto<PictureBinaryDto>());
        }

        /// <summary>
        /// Validates input picture dimensions
        /// </summary>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="fileName">Name of file</param>
        [HttpPost]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ValidatePicture([FromBody] byte[] pictureBinary,
            [FromQuery, Required] string mimeType, [FromQuery, Required] string fileName)
        {
            var result = await _pictureService.ValidatePictureAsync(pictureBinary, mimeType, fileName);

            return Ok(result);
        }

        #endregion
    }
}
