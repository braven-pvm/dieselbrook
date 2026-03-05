using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Media;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Media;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Media
{
    public partial class PictureController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public PictureController(IPictureService pictureService,
            IProductService productService)
        {
            _pictureService = pictureService;
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a picture by identifier
        /// </summary>
        /// <param name="id">The picture identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PictureDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var picture = await _pictureService.GetPictureByIdAsync(id);

            if (picture == null)
            {
                return NotFound($"Picture Id={id} not found");
            }

            return Ok(picture.ToDto<PictureDto>());
        }

        /// <summary>
        /// Delete a picture
        /// </summary>
        /// <param name="id">Picture identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var picture = await _pictureService.GetPictureByIdAsync(id);

            if (picture == null)
                return NotFound($"Picture Id={id} not found");

            await _pictureService.DeletePictureAsync(picture);

            return Ok();
        }

        /// <summary>
        /// Gets a collection of pictures
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="pageIndex">Current page</param>
        /// <param name="pageSize">Items on each page</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Picture, PictureDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPictures([FromQuery] string virtualPath = "",
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var pictures = await _pictureService.GetPicturesAsync(virtualPath, pageIndex, pageSize);

            var pagedListDto = pictures.ToPagedListDto<Picture, PictureDto>();

            return Ok(pagedListDto);
        }

        /// <summary>
        /// Gets a picture by identifier
        /// </summary>
        /// <param name="id">The product identifier</param>
        /// <param name="recordsToReturn">Number of records to return. 0 if you want to get all items</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<PictureDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByProductId(int id, [FromQuery] int recordsToReturn = 0)
        {
            if (id <= 0)
                return BadRequest();

            var pictures = await _pictureService.GetPicturesByProductIdAsync(id, recordsToReturn);
            var pictureDtos = pictures.Select(picture => picture.ToDto<PictureDto>()).ToList();

            return Ok(pictureDtos);
        }

        /// <summary>
        /// Inserts a picture
        /// </summary>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <param name="altAttribute">"alt" attribute for "img" HTML element</param>
        /// <param name="titleAttribute">"title" attribute for "img" HTML element</param>
        /// <param name="isNew">A value indicating whether the picture is new</param>
        /// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
        [HttpPut]
        [ProducesResponseType(typeof(PictureDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> InsertPicture([FromBody] byte[] pictureBinary,
            [FromQuery, Required] string mimeType,
            [FromQuery, Required] string seoFilename,
            [FromQuery] string altAttribute = null,
            [FromQuery] string titleAttribute = null,
            [FromQuery] bool isNew = true,
            [FromQuery] bool validateBinary = true)
        {
            var picture = await _pictureService.InsertPictureAsync(pictureBinary, mimeType, seoFilename,
                altAttribute, titleAttribute, isNew, validateBinary);

            return Ok(picture.ToDto<PictureDto>());
        }

        /// <summary>
        /// Update a picture
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <param name="altAttribute">"alt" attribute for "img" HTML element</param>
        /// <param name="titleAttribute">"title" attribute for "img" HTML element</param>
        /// <param name="isNew">A value indicating whether the picture is new</param>
        /// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
        [HttpPut("{pictureId}")]
        [ProducesResponseType(typeof(PictureDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UpdatePicture(int pictureId,
            [FromBody] byte[] pictureBinary,
            [FromQuery, Required] string mimeType,
            [FromQuery, Required] string seoFilename,
            [FromQuery] string altAttribute = null,
            [FromQuery] string titleAttribute = null,
            [FromQuery] bool isNew = true,
            [FromQuery] bool validateBinary = true)
        {
            var picture = await _pictureService.UpdatePictureAsync(pictureId, pictureBinary, mimeType, seoFilename,
                altAttribute, titleAttribute, isNew, validateBinary);

            return Ok(picture.ToDto<PictureDto>());
        }

        /// <summary>
        /// Update a picture
        /// </summary>
        /// <param name="model">Picture Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] PictureDto model)
        {
            var picture = await _pictureService.GetPictureByIdAsync(model.Id);

            if (picture == null)
                return NotFound("Picture is not found");

            picture = model.FromDto<Picture>();
            await _pictureService.UpdatePictureAsync(picture);

            return Ok();
        }

        /// <summary>
        /// Updates a SEO filename of a picture
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="seoFilename">The SEO filename</param>
        [HttpGet("{pictureId}")]
        [ProducesResponseType(typeof(PictureDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SetSeoFilename(int pictureId, [FromQuery][Required] string seoFilename)
        {
            var result = await _pictureService.SetSeoFilenameAsync(pictureId, seoFilename);

            return Ok(result.ToDto<PictureDto>());
        }
        
        /// <summary>
        /// Get product picture (for shopping cart and order details pages)
        /// </summary>
        /// <param name="productId">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PictureDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductPicture([FromBody] string attributesXml, int productId)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var result = await _pictureService.GetProductPictureAsync(product, attributesXml);

            return Ok(result.ToDto<PictureDto>());
        }

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        [HttpGet("{pictureId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> LoadPictureBinary(int pictureId)
        {
            if (pictureId <= 0)
                return BadRequest();

            var picture = await _pictureService.GetPictureByIdAsync(pictureId);

            if (picture == null)
                return NotFound("Picture is not found");

            var result = await _pictureService.LoadPictureBinaryAsync(picture);

            return Ok(result);
        }

        /// <summary>
        /// Get picture SEO friendly name
        /// </summary>
        /// <param name="name">Name</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPictureSeName([FromQuery][Required] string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var result = await _pictureService.GetPictureSeNameAsync(name);

            return Ok(result);
        }

        /// <summary>
        /// Gets the default picture URL
        /// </summary>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetDefaultPictureUrl([FromQuery] int targetSize = 0,
            [FromQuery] PictureType defaultPictureType = PictureType.Entity,
            [FromQuery] string storeLocation = null)
        {
            var result = await _pictureService.GetDefaultPictureUrlAsync(targetSize, defaultPictureType, storeLocation);

            return Ok(result);
        }

        /// <summary>
        /// Get a picture URL
        /// </summary>
        /// <param name="pictureId">Reference instance of Picture</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <param name="defaultPictureType">Default picture type</param>
        [HttpGet("{pictureId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GetPictureUrlResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPictureUrl(int pictureId,
            [FromQuery] int targetSize = 0,
            [FromQuery] bool showDefaultPicture = true,
            [FromQuery] string storeLocation = null,
            [FromQuery] PictureType defaultPictureType = PictureType.Entity)
        {
            if (pictureId <= 0)
                return BadRequest();

            var picture = await _pictureService.GetPictureByIdAsync(pictureId);

            if (picture == null)
                return NotFound($"Picture by Id={pictureId}is not found");

            var result = await _pictureService.GetPictureUrlAsync(picture, targetSize, showDefaultPicture,
                storeLocation, defaultPictureType);

            var response = new GetPictureUrlResponse {Url = result.Url, Picture = result.Picture.ToDto<PictureDto>()};

            return Ok(response);
        }

        /// <summary>
        /// Get a picture local path
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        [HttpGet("{pictureId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetThumbLocalPath(int pictureId,
            [FromQuery] int targetSize = 0,
            [FromQuery] bool showDefaultPicture = true)
        {
            if (pictureId <= 0)
                return BadRequest();

            var picture = await _pictureService.GetPictureByIdAsync(pictureId);

            if (picture == null)
                return NotFound($"Picture by Id={pictureId}is not found");

            var result = await _pictureService.GetThumbLocalPathAsync(picture, targetSize, showDefaultPicture);

            return Ok(result);
        }

        #endregion
    }
}
