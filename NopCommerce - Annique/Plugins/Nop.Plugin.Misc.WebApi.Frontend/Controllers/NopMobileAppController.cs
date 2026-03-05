using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Media;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public class NopMobileAppController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly WebApiMobileSettings _mobileSettings;

        #endregion

        #region Ctor

        public NopMobileAppController(IPictureService pictureService, ISettingService settingService,
            WebApiMobileSettings mobileSettings)
        {
            _pictureService = pictureService;
            _settingService = settingService;
            _mobileSettings = mobileSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all settings
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Settings()
        {
            var settings = await _settingService.GetAllSettingsAsync();

            var dictionary = settings
                .Where(setting =>
                    _mobileSettings.AllowedSettings.Contains(setting.Name, StringComparer.InvariantCultureIgnoreCase))
                .ToDictionary(setting => setting.Name, setting => setting.Value);

            return Ok(dictionary);
        }

        [HttpGet]
        [ProducesResponseType(typeof(SliderDataDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SliderData()
        {
            var rez = new SliderDataDto();

            var productImages = _mobileSettings.ProductsSliderImages.SelectAwait(async pi =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(pi.Key);

                if (picture == null)
                    return null;

                var (fullSizeImageUrl, _) = await _pictureService.GetPictureUrlAsync(picture);
                var (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, _mobileSettings.SliderPictureSize);
                var (thumbImageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, _mobileSettings.SliderPictureSize);

                return new SliderDataDto.SliderProductDto
                {
                    ProductId = pi.Value,
                    Picture = new PictureModelDto
                    {
                        FullSizeImageUrl = fullSizeImageUrl, ImageUrl = imageUrl, ThumbImageUrl = thumbImageUrl,
                    }
                };
            });

            rez.Products.AddRange(await productImages.Where(p => p != null).ToArrayAsync());

            return Ok(rez);
        }

        #endregion
    }
}
