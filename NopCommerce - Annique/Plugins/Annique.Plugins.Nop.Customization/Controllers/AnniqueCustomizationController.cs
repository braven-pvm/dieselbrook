using Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Factories.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Factories.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Models;
using Annique.Plugins.Nop.Customization.Models.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Admin;
using Annique.Plugins.Nop.Customization.Services.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Services.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class AnniqueCustomizationController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ICustomerService _customerService;
        private readonly ICategoryService _categoryService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IWorkContext _workContext;
        private readonly ICountryService _countryService;
        private readonly IChatFeedbackModelFactory _chatFeedbackModelFactory;
        private readonly IChatbotService _chatbotService;
        private readonly IConsultantNewRegistrationService _consultantNewRegistrationService;
        private readonly IConsultantRegistrationModelFactory _consultantRegistrationModelFactory;

        #endregion

        #region Ctor

        public AnniqueCustomizationController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IAddressAttributeService addressAttributeService,
            ICustomerService customerService,
            ICategoryService categoryService,
            IStaticCacheManager staticCacheManager,
            IMessageTemplateService messageTemplateService,
            IWorkContext workContext,
            ICountryService countryService,
            IChatFeedbackModelFactory chatFeedbackModelFactory,
            IChatbotService chatbotService,
            IConsultantNewRegistrationService consultantNewRegistrationService,
            IConsultantRegistrationModelFactory consultantRegistrationModelFactory)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _addressAttributeService = addressAttributeService;
            _customerService = customerService;
            _categoryService = categoryService;
            _staticCacheManager = staticCacheManager;
            _messageTemplateService = messageTemplateService;
            _workContext = workContext;
            _countryService = countryService;
            _chatFeedbackModelFactory = chatFeedbackModelFactory;
            _chatbotService = chatbotService;
            _consultantNewRegistrationService = consultantNewRegistrationService;
            _consultantRegistrationModelFactory = consultantRegistrationModelFactory;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get category list
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category list
        /// </returns>
        protected virtual async Task<List<SelectListItem>> GetCategoryListAsync()
        {
            var listItems = await _staticCacheManager.GetAsync(NopModelCacheDefaults.CategoriesListKey, async () =>
            {
                var categories = await _categoryService.GetAllCategoriesAsync(showHidden: true);
                return await categories.SelectAwait(async c => new SelectListItem
                {
                    Text = await _categoryService.GetFormattedBreadCrumbAsync(c, categories),
                    Value = c.Id.ToString()
                }).ToListAsync();
            });

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope);
            var model = new ConfigurationModel
            {
                IsEnablePlugin = settings.IsEnablePlugin,
                ActiveStoreScopeConfiguration = storeScope,
                IsPickUpCollection = settings.IsPickUpCollection,
                PickUpStoreRadius = settings.PickUpStoreRadius,
                GeoLocationApiUsername = settings.GeoLocationApiUsername,
                PickupCustomAttributeId = settings.PickupCustomAttributeId,
                TotalOrderNo = settings.TotalOrderNo,
                OrderAmountLimit = settings.OrderAmountLimit,
                CustomerRoleId = settings.CustomerRoleId,
                ShippingAddressValidationApi = settings.ShippingAddressValidationApi,
                ExclusiveItemsCategoryId = settings.ExclusiveItemsCategoryId,
                ConsultantRoleId = settings.ConsultantRoleId,
                ReportScripts = settings.ReportScripts,
                ReportCommonJs = settings.ReportCommonJs,
                LoginTimeLimit = settings.LoginTimeLimit,
                IsOTP = settings.IsOTP,
                OTPApiUrl = settings.OTPApiUrl,
                IsStageEnvType = settings.IsStageEnvType,
                AdminCustomerId = settings.AdminCustomerId,
                ExcludedCategoryIds = settings.ExcludedCategoryIds,
                CustomCacheExpireTime = settings.CustomCacheExpireTime,
                EmailableApi = settings.EmailableApi,
                IsEmailVerification = settings.IsEmailVerification,
                EmailableApiKey = settings.EmailableApiKey,
                IsPasswordResetEnabled = settings.IsPasswordResetEnabled,
                PasswordResetApi = settings.PasswordResetApi,
                PasswordResetApiKey = settings.PasswordResetApiKey,
                PasswordResetSmsMessageTemplateId = settings.PasswordResetSmsMessageTemplateId,
                IsStagingModeForPasswordReset = settings.IsStagingModeForPasswordReset,
                IsDefaultCountryIdEnabled = settings.IsDefaultCountryIdEnabled,
                DefaultCountryId = settings.DefaultCountryId,
                IsCustomShippingRule = settings.IsCustomShippingRule,
                IsFullTextSearchEnabled = settings.IsFullTextSearchEnabled,
                IsTripEnable = settings.IsTripEnable,
                TripStartDate = settings.TripStartDate,
                TripEndDate = settings.TripEndDate,
                TripMessageTemplate = settings.TripMessageTemplate,
                QualifyingAmount = settings.QualifyingAmount,
                IsChatbotEnable = settings.IsChatbotEnable,
                OpenAIApiKey = settings.OpenAIApiKey,
                PromptTemplate = settings.PromptTemplate,
                RegistrationValidationApiEndPoint = settings.RegistrationValidationApiEndPoint,
                RegistrationValidationApiKey = settings.RegistrationValidationApiKey,
                PostRedirectUrl = settings.PostRedirectUrl,
                IsNopConsultantRegistration = settings.IsNopConsultantRegistration,
                IsAdminAccessUrl = settings.IsAdminAccessUrl,
            };

            if (!string.IsNullOrEmpty(settings.ExcludedCategoryIds))
                model.SelectedExcludeCategoryIds = settings.ExcludedCategoryIds.Split(',').Select(int.Parse).ToList();

            if (!string.IsNullOrEmpty(settings.CustomerRoleIdsForPickup))
                model.SelectedCustomerRoleIdsForPickup = settings.CustomerRoleIdsForPickup.Split(',').Select(int.Parse).ToList();

            if (!string.IsNullOrEmpty(settings.ChatbotAccessRoles))
                model.SelectedChatbotAccessRoleIds = settings.ChatbotAccessRoles.Split(',').Select(int.Parse).ToList();

            model.AvailableAddressCustomAttributes = (await _addressAttributeService.GetAllAddressAttributesAsync()).Select(addressAttribute => new SelectListItem
            {
                Text = addressAttribute.Name,
                Value = addressAttribute.Id.ToString()
            }).ToList();

            model.AvailableCustomerRoles = (await _customerService.GetAllCustomerRolesAsync()).Select(customerRole => new SelectListItem
            {
                Text = customerRole.Name,
                Value = customerRole.Id.ToString()
            }).ToList();

            var availableCategoryItems = await GetCategoryListAsync();
            foreach (var categoryItem in availableCategoryItems)
            {
                model.AvailableCategories.Add(categoryItem);
            }

            model.AvailableMessageTemplates = (await _messageTemplateService.GetAllMessageTemplatesAsync(storeScope)).Select(messageTemplate => new SelectListItem
            {
                Text = messageTemplate.Name,
                Value = messageTemplate.Id.ToString()
            }).ToList();

            //get customer where role is administator 
            var adminCustomers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });
            model.AvailableAdminCustomers = adminCustomers.Select(adminCustomer => new SelectListItem
            {
                Text = adminCustomer.Username,
                Value = adminCustomer.Id.ToString()
            }).ToList();

            model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            foreach (var c in await _countryService.GetAllCountriesAsync(currentLanguage.Id))
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.DefaultCountryId,
                });
            }

            if (storeScope > 0)
            {
                model.IsEnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsEnablePlugin, storeScope);
                model.IsPickUpCollection_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsPickUpCollection, storeScope);
                model.PickUpStoreRadius_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PickUpStoreRadius, storeScope);
                model.GeoLocationApiUsername_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.GeoLocationApiUsername, storeScope);
                model.PickupCustomAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PickupCustomAttributeId, storeScope);
                model.CustomerRoleIdsForPickup_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CustomerRoleIdsForPickup, storeScope);
                model.TotalOrderNo_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TotalOrderNo, storeScope);
                model.OrderAmountLimit_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OrderAmountLimit, storeScope);
                model.CustomerRoleId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CustomerRoleId, storeScope);
                model.ShippingAddressValidationApi_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShippingAddressValidationApi, storeScope);
                model.ExclusiveItemsCategoryId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ExclusiveItemsCategoryId, storeScope);
                model.ConsultantRoleId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ConsultantRoleId, storeScope);
                model.ReportScripts_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ReportScripts, storeScope);
                model.ReportCommonJs_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ReportCommonJs, storeScope);
                model.LoginTimeLimit_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.LoginTimeLimit, storeScope);
                model.IsOTP_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsOTP, storeScope);
                model.OTPApiUrl_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OTPApiUrl, storeScope);
                model.IsStageEnvType_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsStageEnvType, storeScope);
                model.AdminCustomerId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AdminCustomerId, storeScope);
                model.ExcludedCategoryIds_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ExcludedCategoryIds, storeScope);
                model.CustomCacheExpireTime_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CustomCacheExpireTime, storeScope);
                model.IsEmailVerification_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsEmailVerification, storeScope);
                model.EmailableApi_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EmailableApi, storeScope);
                model.EmailableApiKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EmailableApiKey, storeScope);
                model.IsPasswordResetEnabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsPasswordResetEnabled, storeScope);
                model.PasswordResetApi_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PasswordResetApi, storeScope);
                model.PasswordResetApiKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PasswordResetApiKey, storeScope);
                model.PasswordResetSmsMessageTemplateId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PasswordResetSmsMessageTemplateId, storeScope);
                model.IsStagingModeForPasswordReset_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsStagingModeForPasswordReset, storeScope);
                model.IsDefaultCountryIdEnabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsDefaultCountryIdEnabled, storeScope);
                model.DefaultCountryId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultCountryId, storeScope);
                model.IsCustomShippingRule_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsCustomShippingRule, storeScope);
                model.IsFullTextSearchEnabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsFullTextSearchEnabled, storeScope);
                model.IsTripEnable_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsTripEnable, storeScope);
                model.TripStartDate_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TripStartDate, storeScope);
                model.TripEndDate_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TripEndDate, storeScope);
                model.TripMessageTemplate_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TripMessageTemplate, storeScope);
                model.QualifyingAmount_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.QualifyingAmount, storeScope);
                model.IsChatbotEnable_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsChatbotEnable, storeScope);
                model.OpenAIApiKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OpenAIApiKey, storeScope);
                model.PromptTemplate_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PromptTemplate, storeScope);
                model.ChatbotAccessRoles_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ChatbotAccessRoles, storeScope);
                model.RegistrationValidationApiEndPoint_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.RegistrationValidationApiEndPoint, storeScope);
                model.RegistrationValidationApiKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.RegistrationValidationApiKey, storeScope);
                model.PostRedirectUrl_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PostRedirectUrl, storeScope);
                model.IsNopConsultantRegistration_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsNopConsultantRegistration, storeScope);
                model.IsAdminAccessUrl_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsAdminAccessUrl, storeScope);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope);

            if (ModelState.IsValid)
            {
                bool appRestart = false;

                if (settings.IsEnablePlugin != model.IsEnablePlugin || settings.IsPickUpCollection != model.IsPickUpCollection)
                    appRestart = true;

                settings.IsEnablePlugin = model.IsEnablePlugin;
                settings.IsPickUpCollection = model.IsPickUpCollection;
                settings.PickUpStoreRadius = model.PickUpStoreRadius;
                settings.GeoLocationApiUsername = model.GeoLocationApiUsername;
                settings.PickupCustomAttributeId = model.PickupCustomAttributeId;
                settings.TotalOrderNo = model.TotalOrderNo;
                settings.OrderAmountLimit = model.OrderAmountLimit;
                settings.CustomerRoleId = model.CustomerRoleId;
                settings.ShippingAddressValidationApi = model.ShippingAddressValidationApi;
                settings.ExclusiveItemsCategoryId = model.ExclusiveItemsCategoryId;
                settings.ConsultantRoleId = model.ConsultantRoleId;
                settings.ReportScripts = model.ReportScripts;
                settings.ReportCommonJs = model.ReportCommonJs;
                settings.OTPApiUrl = model.OTPApiUrl;
                settings.IsStageEnvType = model.IsStageEnvType;
                settings.IsOTP = model.IsOTP;
                settings.AdminCustomerId = model.AdminCustomerId;
                settings.CustomCacheExpireTime = model.CustomCacheExpireTime;
                settings.IsEmailVerification = model.IsEmailVerification;
                settings.EmailableApi = model.EmailableApi;
                settings.EmailableApiKey = model.EmailableApiKey;
                settings.IsPasswordResetEnabled = model.IsPasswordResetEnabled;
                settings.PasswordResetApi = model.PasswordResetApi;
                settings.PasswordResetApiKey = model.PasswordResetApiKey;
                settings.PasswordResetSmsMessageTemplateId = model.PasswordResetSmsMessageTemplateId;
                settings.IsStagingModeForPasswordReset = model.IsStagingModeForPasswordReset;
                settings.IsDefaultCountryIdEnabled = model.IsDefaultCountryIdEnabled;
                settings.DefaultCountryId = model.DefaultCountryId;
                settings.IsCustomShippingRule = model.IsCustomShippingRule;
                settings.IsFullTextSearchEnabled = model.IsFullTextSearchEnabled;
                settings.IsTripEnable = model.IsTripEnable;
                settings.TripStartDate = model.TripStartDate;
                settings.TripEndDate = model.TripEndDate;
                settings.TripMessageTemplate = model.TripMessageTemplate;
                settings.QualifyingAmount = model.QualifyingAmount;
                settings.IsChatbotEnable = model.IsChatbotEnable;
                settings.OpenAIApiKey = model.OpenAIApiKey;
                settings.PromptTemplate = model.PromptTemplate;
               

                settings.ExcludedCategoryIds = model.SelectedExcludeCategoryIds.ToCommaSeparatedString();
                settings.CustomerRoleIdsForPickup = model.SelectedCustomerRoleIdsForPickup.ToCommaSeparatedString();
                settings.ChatbotAccessRoles = model.SelectedChatbotAccessRoleIds.ToCommaSeparatedString();

                settings.PostRedirectUrl = model.PostRedirectUrl;
                settings.RegistrationValidationApiEndPoint = model.RegistrationValidationApiEndPoint;
                settings.RegistrationValidationApiKey = model.RegistrationValidationApiKey;
                settings.IsNopConsultantRegistration = model.IsNopConsultantRegistration;
                settings.IsAdminAccessUrl = model.IsAdminAccessUrl;

                //if no time limit set take 5 as fall back time out minute 
                settings.LoginTimeLimit = (model.LoginTimeLimit <= 0) ? 5 : model.LoginTimeLimit;

                //if no cache time set take 3 min as custom cache 
                settings.CustomCacheExpireTime = (model.CustomCacheExpireTime <= 0) ? 3 : model.CustomCacheExpireTime;

                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsEnablePlugin, model.IsEnablePlugin_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsPickUpCollection, model.IsPickUpCollection_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PickUpStoreRadius, model.PickUpStoreRadius_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.GeoLocationApiUsername, model.GeoLocationApiUsername_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PickupCustomAttributeId, model.PickupCustomAttributeId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CustomerRoleIdsForPickup, model.CustomerRoleIdsForPickup_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TotalOrderNo, model.TotalOrderNo_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OrderAmountLimit, model.OrderAmountLimit_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CustomerRoleId, model.CustomerRoleId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShippingAddressValidationApi, model.ShippingAddressValidationApi_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ExclusiveItemsCategoryId, model.ExclusiveItemsCategoryId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ConsultantRoleId, model.ConsultantRoleId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ReportCommonJs, model.ReportCommonJs_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ReportScripts, model.ReportScripts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.LoginTimeLimit, model.LoginTimeLimit_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsOTP, model.IsOTP_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OTPApiUrl, model.OTPApiUrl_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsStageEnvType, model.IsStageEnvType_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AdminCustomerId, model.AdminCustomerId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ExcludedCategoryIds, model.ExcludedCategoryIds_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CustomCacheExpireTime, model.CustomCacheExpireTime_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsEmailVerification, model.IsEmailVerification_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EmailableApi, model.EmailableApi_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EmailableApiKey, model.EmailableApiKey_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsPasswordResetEnabled, model.IsPasswordResetEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PasswordResetApi, model.PasswordResetApi_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PasswordResetApiKey, model.PasswordResetApiKey_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PasswordResetSmsMessageTemplateId, model.PasswordResetSmsMessageTemplateId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsStagingModeForPasswordReset, model.IsStagingModeForPasswordReset_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsDefaultCountryIdEnabled, model.IsDefaultCountryIdEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultCountryId, model.DefaultCountryId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsCustomShippingRule, model.IsCustomShippingRule_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsFullTextSearchEnabled, model.IsFullTextSearchEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsTripEnable, model.IsTripEnable_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TripStartDate, model.TripStartDate_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TripEndDate, model.TripEndDate_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TripMessageTemplate, model.TripMessageTemplate_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.QualifyingAmount, model.QualifyingAmount_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsChatbotEnable, model.IsChatbotEnable_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OpenAIApiKey, model.OpenAIApiKey_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PromptTemplate, model.PromptTemplate_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ChatbotAccessRoles, model.ChatbotAccessRoles_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.RegistrationValidationApiEndPoint, model.RegistrationValidationApiEndPoint_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.RegistrationValidationApiKey, model.RegistrationValidationApiKey_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PostRedirectUrl, model.PostRedirectUrl_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsNopConsultantRegistration, model.IsNopConsultantRegistration_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsAdminAccessUrl, model.IsAdminAccessUrl_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                if (appRestart)
                {
                    _webHelper.RestartAppDomain();
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

                return await Configure();
            }

            model.AvailableAddressCustomAttributes = (await _addressAttributeService.GetAllAddressAttributesAsync()).Select(addressAttribute => new SelectListItem
            {
                Text = addressAttribute.Name,
                Value = addressAttribute.Id.ToString()
            }).ToList();

            model.AvailableCustomerRoles = (await _customerService.GetAllCustomerRolesAsync()).Select(customerRole => new SelectListItem
            {
                Text = customerRole.Name,
                Value = customerRole.Id.ToString()
            }).ToList();

            var availableCategoryItems = await GetCategoryListAsync();
            foreach (var categoryItem in availableCategoryItems)
            {
                model.AvailableCategories.Add(categoryItem);
            }

            //get customer where role is administator 
            var adminCustomers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });
            model.AvailableAdminCustomers = adminCustomers.Select(adminCustomer => new SelectListItem
            {
                Text = adminCustomer.Username,
                Value = adminCustomer.Id.ToString()
            }).ToList();

            model.AvailableMessageTemplates = (await _messageTemplateService.GetAllMessageTemplatesAsync(storeScope)).Select(messageTemplate => new SelectListItem
            {
                Text = messageTemplate.Name,
                Value = messageTemplate.Id.ToString()
            }).ToList();

            model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            foreach (var c in await _countryService.GetAllCountriesAsync(currentLanguage.Id))
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString()
                });
            }
            return View(model);
        }

        #endregion

        #region chatbot Feedback method

        public virtual async Task<IActionResult> FeedbackList()
        {
            //prepare model
            var model = await _chatFeedbackModelFactory.PrepareFeedbackSearchModelAsync(new ChatFeedbackSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> FeedbackList(ChatFeedbackSearchModel searchModel)
        {
            //prepare model
            var model = await _chatFeedbackModelFactory.PrepareFeedbackListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> FeedbackView(int id)
        {
            //try to get a feedback with the specified id
            var feedback = await _chatbotService.GetFeedbackByIdAsync(id);
            if (feedback == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _chatFeedbackModelFactory.PrepareFeedbackModelAsync(null, feedback);

            return View(model);
        }

        #endregion

        #region New consultant page settings

        [HttpGet]
        public async Task<IActionResult> RegistrationPageSetting()
        {
            var entity = await _consultantNewRegistrationService.GetPageSettings();

            var model = new RegistrationPageSettingsModel
            {
                Id = entity.Id,
                CustomCSS = entity.CustomCSS,
                CustomJS = entity.CustomJS,
                TopSectionPublished = entity.TopSectionPublished,
                TopSectionBody = entity.TopSectionBody,
                LeftSectionPublished = entity.LeftSectionPublished,
                LeftSectionBody = entity.LeftSectionBody,
                BottomSectionPublished = entity.BottomSectionPublished,
                BottomSectionBody = entity.BottomSectionBody
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationPageSetting(RegistrationPageSettingsModel model, bool continueEditing)
        {
            if (!ModelState.IsValid)
                return await RegistrationPageSetting();

            var entity = await _consultantNewRegistrationService.GetPageSettings();

            entity.CustomCSS = model.CustomCSS;
            entity.CustomJS = model.CustomJS;
            entity.TopSectionPublished = model.TopSectionPublished;
            entity.TopSectionBody = model.TopSectionBody;
            entity.LeftSectionPublished = model.LeftSectionPublished;
            entity.LeftSectionBody = model.LeftSectionBody;
            entity.BottomSectionPublished = model.BottomSectionPublished;
            entity.BottomSectionBody = model.BottomSectionBody;

            if (entity.Id > 0)
                await _consultantNewRegistrationService.UpdatePageSettings(entity);
            else
                await _consultantNewRegistrationService.InsertPageSettings(entity);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.PageSetting.Saved"));

             return RedirectToAction("RegistrationPageSetting");
        }

        #endregion

        #region New consultant overview

        public virtual async Task<IActionResult> RegistrationList()
        {
            //prepare model
            var model = await _consultantRegistrationModelFactory.PrepareConsultantRegistrationSearchModelAsync(new ConsultantRegistrationSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> RegistrationList(ConsultantRegistrationSearchModel searchModel)
        {
            //prepare model
            var model = await _consultantRegistrationModelFactory.PrepareConsultantRegistrationListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> ViewNewRegistration(int id)
        {
            //try to get a feedback with the specified id
            var registartion = await _consultantNewRegistrationService.GetRegistrationById(id);
            if (registartion == null)
                return RedirectToAction("RegistrationList");

            //prepare model
            var model = await _consultantRegistrationModelFactory.PrepareConsultantRegistrationOverviewModelAsync(null, registartion);

            return View(model);
        }

        #endregion
    }
}
