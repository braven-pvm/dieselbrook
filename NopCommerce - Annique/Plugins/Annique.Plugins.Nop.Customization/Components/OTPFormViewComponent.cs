using Annique.Plugins.Nop.Customization.Models.OTP;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.OTP;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{

    /// <summary>
    /// Represents a view component that displays in OTP pop up
    /// </summary>
    [ViewComponent(Name = "OTPForm")]
    public class OTPFormViewComponent : NopViewComponent
    {
        #region Field

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IWorkContext _workContext;
        private readonly IOtpService _otpService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public OTPFormViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IWorkContext workContext,
            IOtpService otpService,
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _workContext = workContext;
            _otpService = otpService;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            if(!settings.IsOTP)
                return Content(string.Empty);

            var isConsultantUser = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();

            if (!isConsultantUser)
                return Content(string.Empty);

            var customer = await _workContext.GetCurrentCustomerAsync();

            var model = new OtpModel
            {
                IsVerified = false
            };

            //get customer otp record
            var otpRecord = await _otpService.GetCustomerOtpRecordAsync(customer.Id);

            //if otp record not null and OTP verified
            if (otpRecord != null && otpRecord.Iverified)
            {
                model.IsVerified = true;
            }
            else if (otpRecord != null && !otpRecord.Iverified)
            {
                //if record exist, but otp not verified then do not show send otp buttons
                model.ShowSendOtpButtons = false;
            }
            else
            {
                //show send button only if user has no active otp record
                model.ShowSendOtpButtons = true;
            }

            return View(model);
        }

        #endregion
    }
}