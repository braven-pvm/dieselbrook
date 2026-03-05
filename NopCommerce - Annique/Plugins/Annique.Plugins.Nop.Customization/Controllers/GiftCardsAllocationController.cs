using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.GiftCardAllocation;
using Annique.Plugins.Nop.Customization.Services.GiftCardAllocation;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Controllers;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class GiftCardsAllocationController : BaseAdminController
    {
        #region Fields

        private readonly IGiftCardAdditionalInfoService _giftCardAdditionalInfoService;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public GiftCardsAllocationController(IGiftCardAdditionalInfoService giftCardAdditionalInfoService,
            ILogger logger,
            IWorkContext workContext)
        {
            _giftCardAdditionalInfoService = giftCardAdditionalInfoService;
            _logger = logger;
            _workContext = workContext;
        }

        #endregion

        #region Method

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateGiftCardAdditionalInfo(GiftCardAdditionalInfoModel model)
        {
            try
            {
                var giftCardAdditionalInfo = _giftCardAdditionalInfoService.GetGiftCardAdditionalInfoByGiftcardId(model.GiftCardId);
                if (giftCardAdditionalInfo != null)
                {
                    giftCardAdditionalInfo.Username = model.Username;
                    await _giftCardAdditionalInfoService.UpdateGiftCardAdditionalInfoAsync(giftCardAdditionalInfo);
                }
                else
                {
                    var newGiftCardAdditionalInfo = new GiftCardAdditionalInfo()
                    {
                        GiftCardId = model.GiftCardId,
                        Username = model.Username
                    };
                    await _giftCardAdditionalInfoService.InsertGiftCardAdditionalInfoAsync(newGiftCardAdditionalInfo);
                }

                return Json(new
                {
                    Username = model.Username,
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }

        }
        #endregion
    }
}
