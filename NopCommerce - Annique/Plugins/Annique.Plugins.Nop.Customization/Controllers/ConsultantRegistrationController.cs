using Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Factories.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public;
using Annique.Plugins.Nop.Customization.Services.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Services.NewActivityLogs;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Services.Affiliates;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class ConsultantRegistrationController : BasePublicController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IAffiliateService _affiliateService;
        private readonly IWebHelper _webHelper;
        private readonly IConsultantNewRegistrationService _consultantNewRegistrationService;
        private readonly ISettingService _settingService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ITokenizer _tokenizer;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IConsultantRegistrationModelFactory _consultantRegistrationModelFactory;
        private readonly IAdditionalActivityLogService _additionalActivityLogService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor
        public ConsultantRegistrationController(
            IStoreContext storeContext,
            CaptchaSettings captchaSettings,
            ILocalizationService localizationService,
            IAffiliateService affiliateService,
            IWebHelper webHelper,
            IConsultantNewRegistrationService consultantNewRegistrationService,
             ISettingService settingService,
             IMessageTemplateService messageTemplateService,
            ITokenizer tokenizer,
            LocalizationSettings localizationSettings,
            IConsultantRegistrationModelFactory consultantRegistrationModelFactory,
            IAdditionalActivityLogService additionalActivityLogService,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            _storeContext = storeContext;
            _captchaSettings = captchaSettings;
            _localizationService = localizationService;
            _affiliateService = affiliateService;
            _webHelper = webHelper;
            _consultantNewRegistrationService = consultantNewRegistrationService;
            _settingService = settingService;
            _messageTemplateService = messageTemplateService;
            _tokenizer = tokenizer;
            _localizationSettings = localizationSettings;
            _consultantRegistrationModelFactory = consultantRegistrationModelFactory;
            _additionalActivityLogService = additionalActivityLogService;
            _workContext = workContext;
            _customerService = customerService;
        }

        #endregion

        #region Utilities 

        /// <summary>
        /// prepare tokens
        /// </summary>
        /// <param name="model">Consultant registration model</param>
        /// <param name="validationResponse">validation reposne</param>
        /// <param name="store">Store Entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains tokens for message template
        /// </returns>
        private async Task<List<Token>> PrepareTokensAsync(ConsultantRegistrationModel model, ValidationResponse validationResponse, Store store)
        {
            var tokens = new List<Token>
            {
                new("NewConsultant.Name", $"{model.FirstName} {model.LastName}"),
                new("Store.Name", await _localizationService.GetLocalizedAsync(store, x => x.Name))
            };
            if (validationResponse.ccustno != null)
            {
                tokens.Add(new Token("ccustno", validationResponse.ccustno));
            }
            if (validationResponse.Sponsor != null)
            {
                tokens.Add(new Token("Sponsor.Name", validationResponse.Sponsor.Name));
                tokens.Add(new Token("Sponsor.Email", validationResponse.Sponsor.Email));
                tokens.Add(new Token("Sponsor.Cell", validationResponse.Sponsor.Cell));
            }

            return tokens;
        }

        /// <summary>
        /// Fetch and replace message template
        /// </summary>
        /// <param name="templateName">Message template name</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="fallbackMessage">Fallback message</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains message to show in popup after registration
        /// </returns>
        private async Task<string> GetMessageBodyAsync(string templateName, List<Token> tokens, string fallbackMessage)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplatesByNameAsync(templateName);

            if (messageTemplate.Any())
            {
                var body = await _localizationService.GetLocalizedAsync(messageTemplate.FirstOrDefault(), mt => mt.Body, _localizationSettings.DefaultAdminLanguageId);
                return _tokenizer.Replace(body, tokens, true);
            }
            else
            {
                return fallbackMessage;
            }
        }

        /// <summary>
        /// Manage Pop up after registration
        /// </summary>
        /// <param name="model">Registration model</param>
        /// <param name="newRegistrations">New registation</param>
        /// <param name="settings">Annique setting</param>
        /// <param name="validationResponse">Validation response</param>
        /// <param name="store">Store</param>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="defaultMessage">Default message</param>
        /// <param name="popupType">Pop up type</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains pop up based on type, welcome pop up or exising user warning pop up
        /// </returns>
        private async Task HandlePopupAsync(
            ConsultantRegistrationModel model,
            NewRegistrations newRegistrations,
            AnniqueCustomizationSettings settings,
            ValidationResponse validationResponse,
            Store store,
            string messageTemplate,
            string defaultMessage,
            string popupType)
        {
            // Prepare tokens
            var tokens = await PrepareTokensAsync(model, validationResponse, store);

            // Get localized message
            var messageBody = await GetMessageBodyAsync(messageTemplate, tokens, defaultMessage);

            // Set popup type and message
            var postModel = model.ConsultantPostResgistrationModel;

            switch (popupType.ToLowerInvariant())
            {
                case "welcome":
                    newRegistrations.Status = "New";
                    newRegistrations.UpdatedOnUtc = DateTime.UtcNow;
                    await _consultantNewRegistrationService.UpdateAsyc(newRegistrations);
                    postModel.ShowWelcomePopup = true;
                    postModel.WelcomeMessage = messageBody;
                    break;

                case "existing":
                    postModel.ShowCustomerExistPopup = true;
                    postModel.ExistingCustomerMessage = messageBody;
                    break;

                default:
                    break;
            }

            // Set redirect URL
            postModel.RedirectUrl = _webHelper.GetStoreLocation() + settings.PostRedirectUrl.TrimStart('/');
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> RegisterConsultant()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            // Step 1: If IsAdminAccessUrl is enabled, check if the customer is an admin
            if (settings.IsAdminAccessUrl)
            {
                // Check if the customer is in the "Administrators" role
                if (await _customerService.IsAdminAsync(customer))
                    return await PrepareConsultantRegistrationPage(customer);
            }

            // Step 2: If admin access is not enabled or customer is not an admin, check if Nop-based registration is enabled
            var isNopConsultantRegistrationEnabled = await _consultantNewRegistrationService.IsNopBasedConsultantRegistrationEnabledAsync();

            // If Nop-based registration is disabled, redirect to the old registration page
            if (!isNopConsultantRegistrationEnabled)
                return Redirect(_webHelper.GetStoreLocation() + "new-consultant-registration");

            // Step 3: Proceed with the normal consultant registration flow
            return await PrepareConsultantRegistrationPage(customer);
        }

        /// <summary>
        /// Prepares the consultant registration page, handling the referral code and other settings.
        /// </summary>
        private async Task<IActionResult> PrepareConsultantRegistrationPage(Customer customer)
        {
            #region Task 650 Consultant Register Activity

            await _additionalActivityLogService.InsertActivityTrackingLogAsync("PublicStore.ConsultantRegistrationPageVisit", "Consultant Registration Page Visit", customer);

            #endregion

            var model = await _consultantRegistrationModelFactory.PrepareConsultantRegistrationModelAsync();
            model.Browser = _webHelper.GetThisPageUrl(true);
            model.Csponser = string.Empty;

            // Check for referral code in query string
            if (Request.Query.ContainsKey("ref"))
            {
                var refCode = Request.Query["ref"].ToString();
                model.Csponser = refCode;

                // Assuming this method exists to fetch affiliate by code
                var affiliate = await _affiliateService.GetAffiliateByFriendlyUrlNameAsync(refCode);

                if (affiliate != null && !affiliate.Deleted)
                {
                    var affiliateName = await _affiliateService.GetAffiliateFullNameAsync(affiliate);
                    model.AffiliateName = $"{affiliateName}".Trim();
                }
            }
            model.ConsultantPostResgistrationModel = new ConsultantPostResgistrationModel();
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        public async Task<IActionResult> RegisterConsultant(ConsultantRegistrationModel model, bool captchaValid)
        {
            model = await _consultantRegistrationModelFactory.PrepareConsultantRegistrationModelAsync(model);
            model.ConsultantPostResgistrationModel = new ConsultantPostResgistrationModel();

            // validate CAPTCHA
            if (_captchaSettings.Enabled && !captchaValid)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));

            if (!ModelState.IsValid)
                return View(model);

            // prepare entity from model
            var newRegistration = await _consultantNewRegistrationService.PrepareNewRegistrationsFromModel(model);

            if (model.Id == 0)
                await _consultantNewRegistrationService.InsertAsync(newRegistration);
            else
                await _consultantNewRegistrationService.UpdateAsyc(newRegistration);

            // Validation API
            var validationResponse = await _consultantNewRegistrationService.ValidateConsultantAsync(newRegistration.Id);

            var updatedConsultantRegisteration = await _consultantNewRegistrationService.GetRegistrationById(newRegistration.Id);
            model.Id = updatedConsultantRegisteration.Id;

            var store = await _storeContext.GetCurrentStoreAsync();
           
            //load settings
            var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            if (validationResponse?.Status == "VALID")
            {
                // Handle valid registration
                await HandlePopupAsync(
                     model,
                     updatedConsultantRegisteration,
                     anniqueSettings,
                     validationResponse,
                     store,
                     AnniqueCustomizationDefaults.NewConsultantWelcomeMeessageTemplate,
                     await _localizationService.GetResourceAsync("Consultant.Registration.Welcome.FallbackMessage"),
                     "welcome");

                var customer = await _workContext.GetCurrentCustomerAsync();

                #region Task 650 Consultant Register Activity

                await _additionalActivityLogService.InsertActivityTrackingLogAsync("PublicStore.ConsultantRegistrationSuccessful", "Consultant Registration Successful", customer);

                #endregion

                return View(model);
            }
            else if (validationResponse?.Status == "INVALID" && validationResponse.Errors?.Any() == true)
            {
                // Check if any error rule is "Already Registered"
                if (validationResponse.Errors.Any(e => e.Rule.Equals("Already Registered", StringComparison.OrdinalIgnoreCase)))
                {
                    await HandlePopupAsync(
                                        model,
                                        updatedConsultantRegisteration,
                                        anniqueSettings,
                                        validationResponse,
                                        store,
                                        AnniqueCustomizationDefaults.ConsultantWarningMeessageTemplate,
                                        await _localizationService.GetResourceAsync("Consultant.Registration.Warning.FallbackMessage"),
                                        "existing");
                }
                else
                {
                    // Add all API errors to ModelState
                    foreach (var error in validationResponse.Errors)
                    {
                        ModelState.AddModelError("", error.Message);
                    }
                }
                return View(model);
            }

            // Fallback error
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Consultant.Registration.FallbackError"));
            return View(model);
        }
    }
}
