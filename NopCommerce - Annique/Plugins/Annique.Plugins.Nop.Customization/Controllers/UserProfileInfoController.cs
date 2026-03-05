using Annique.Plugins.Nop.Customization.Factories.UserProfile;
using Annique.Plugins.Nop.Customization.Models.UserProfile;
using Annique.Plugins.Nop.Customization.Services.OTP;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Models.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class UserProfileInfoController : BasePublicController
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICountryService _countryService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly TaxSettings _taxSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ITaxService _taxService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly IUserProfileAdditionalInfoModelFactory _userProfileAdditionalInfoModelFactory;
        private readonly ISettingService _settingService;
        private readonly IOtpService _otpService;

        #endregion

        #region Ctor

        public UserProfileInfoController(CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAuthenticationService authenticationService,
            ICountryService countryService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreContext storeContext,
            IWorkContext workContext,
            INotificationService notificationService,
            ILogger logger,
            IStateProvinceService stateProvinceService,
            TaxSettings taxSettings,
            IWorkflowMessageService workflowMessageService,
            ITaxService taxService,
            LocalizationSettings localizationSettings,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            IUserProfileAdditionalInfoModelFactory userProfileAdditionalInfoModelFactory,
            ISettingService settingService,
            IOtpService otpService)
        {
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _storeContext = storeContext;
            _workContext = workContext;
            _notificationService = notificationService;
            _logger = logger;
            _stateProvinceService = stateProvinceService;
            _taxSettings = taxSettings;
            _workflowMessageService = workflowMessageService;
            _taxService = taxService;
            _localizationSettings = localizationSettings;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _userProfileAdditionalInfoModelFactory = userProfileAdditionalInfoModelFactory;
            _settingService = settingService;
            _otpService = otpService;
        }

        #endregion

        #region Utilities

        //update changes into customer change table
        protected async void UpdateCustomerChanges(Customer customer, string tableName, string fieldName, string oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(oldValue) && !string.IsNullOrWhiteSpace(newValue))
            {
                newValue = newValue.Trim();
                oldValue = oldValue.Trim();
                if (!oldValue.Equals(newValue))
                {
                    //Update changes in customer changes table
                    await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, tableName, customer.Id, fieldName, oldValue, newValue);
                }
            }
            else
            {
                if (string.Compare(oldValue, newValue) != 0)
                    //Update changes in customer changes table
                    await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, tableName, customer.Id, fieldName, oldValue, newValue);
            }
        }

        protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
        {
            foreach (var consent in consents)
            {
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
                {
                    ModelState.AddModelError("", consent.RequiredMessage);
                }
            }
        }

        protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = await _customerAttributeService.GetAllCustomerAttributesAsync();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _customerAttributeService.GetCustomerAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected virtual async Task LogGdprAsync(Customer customer, CustomerInfoModel oldCustomerInfoModel,
            CustomerInfoModel newCustomerInfoModel, IFormCollection form)
        {
            try
            {
                //consents
                var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id, customer.Id);
                    var controlId = $"consent{consent.Id}";
                    var cbConsent = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                    {
                        //agree
                        if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                        {
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        }
                    }
                    else
                    {
                        //disagree
                        if (!previousConsentValue.HasValue || previousConsentValue.Value)
                        {
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                        }
                    }
                }

                //newsletter subscriptions
                if (_gdprSettings.LogNewsletterConsent)
                {
                    if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                    if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                }

                //user profile changes
                if (!_gdprSettings.LogUserProfileChanges)
                    return;

                if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

                if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

                if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

                if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");

                if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

                if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

                if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

                if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

                if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

                if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

                if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

                if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
                {
                    var countryName = (await _countryService.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
                }

                if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
                {
                    var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
                }
            }
            catch (Exception exception)
            {
                await _logger.ErrorAsync(exception.Message, exception, customer);
            }
        }

        #endregion

        #region Method

        public IActionResult Info()
        {
            return RedirectToAction("Info", "Customer");
        }

        [HttpPost]
        public virtual async Task<IActionResult> Info(CustomerInfoModel model, UserProfileAdditionalInfoModel userProfileAdditionalInfoModel, IFormCollection form)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var oldCustomerModel = new CustomerInfoModel();
            oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

            var oldCustomerModelForGdpr = new CustomerInfoModel();
            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModelForGdpr = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                .GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();
                ValidateRequiredConsents(consents, form);
            }

            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(_storeContext.GetCurrentStoreAsync().Id);
            try
            {
                if (ModelState.IsValid)
                {
                    //prepare user profile fields
                    var userProfileInfo = _userProfileAdditionalInfoModelFactory.PrepareUserProfileAdditionalInfoFields(userProfileAdditionalInfoModel);

                    if(customerRoleIds.Contains(settings.ConsultantRoleId))
                        //validation for unique Id number
                        await _userProfileAdditionalInfoService.ValidateUserIdNumberAsync(userProfileInfo, userProfileAdditionalInfoModel.IdNumber);

                    //if new entry for user profile info
                    if (userProfileInfo.Id == 0)
                    {
                        await _userProfileAdditionalInfoService.ValidatePhoneNumberAsync(userProfileAdditionalInfoModel.WhatsappNumber, customer.Id, "Annique.Plugin.WhatsAppNumber.Validation.Message");
                        //Insert new info
                        await _userProfileAdditionalInfoService.InsertUserAdditionalInfoAsync(userProfileInfo);
                    }
                    else
                    {
                        //if user profile already exist get orignal record
                        var existUserProfile = await _userProfileAdditionalInfoService.GetUserProfileAdditionalInfoByCustomerIdAsync(customer.Id);

                        //compare exist record with newly updated 
                        if (existUserProfile.Title != userProfileAdditionalInfoModel.Title)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.Title"), existUserProfile.Title, userProfileAdditionalInfoModel.Title);

                        if (existUserProfile.Nationality != userProfileAdditionalInfoModel.Nationality)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.Nationality"), existUserProfile.Nationality, userProfileAdditionalInfoModel.Nationality);

                        if (existUserProfile.IdNumber != userProfileAdditionalInfoModel.IdNumber)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.IdNumber"), existUserProfile.IdNumber, userProfileAdditionalInfoModel.IdNumber);

                        if (existUserProfile.Language != userProfileAdditionalInfoModel.Language)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.Language"), existUserProfile.Language, userProfileAdditionalInfoModel.Language);

                        if (existUserProfile.Ethnicity != userProfileAdditionalInfoModel.Ethnicity)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.Ethnicity"), existUserProfile.Ethnicity, userProfileAdditionalInfoModel.Ethnicity);

                        if (existUserProfile.BankName != userProfileAdditionalInfoModel.BankName)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.BankName"), existUserProfile.BankName, userProfileAdditionalInfoModel.BankName);

                        if (existUserProfile.AccountHolder != userProfileAdditionalInfoModel.AccountHolder)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.AccountHolder"), existUserProfile.AccountHolder, userProfileAdditionalInfoModel.AccountHolder);

                        if (existUserProfile.AccountType != userProfileAdditionalInfoModel.AccountType)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.AccountType"), existUserProfile.AccountType, userProfileAdditionalInfoModel.AccountType);

                        if (existUserProfile.WhatsappNumber != userProfileAdditionalInfoModel.WhatsappNumber)
                        {
                            await _userProfileAdditionalInfoService.ValidatePhoneNumberAsync(userProfileAdditionalInfoModel.WhatsappNumber, customer.Id, "Annique.Plugin.WhatsAppNumber.Validation.Message");

                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.WhatsappNumber"), existUserProfile.WhatsappNumber, userProfileAdditionalInfoModel.WhatsappNumber);
                        }

                        if (existUserProfile.Accept != userProfileAdditionalInfoModel.Accept)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.Accept"), existUserProfile.Accept.ToString(), userProfileAdditionalInfoModel.Accept.ToString());

                        if (existUserProfile.AccountNumber != userProfileInfo.AccountNumber)
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.UserProfileTable, existUserProfile.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.AccountNumber"), existUserProfile.AccountNumber, userProfileInfo.AccountNumber);

                        await _userProfileAdditionalInfoService.UpdateUserAdditionalInfoAsync(userProfileInfo);
                    }

                    //username 
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        var userName = model.Username.Trim();
                        if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var oldUsername = customer.Username;

                            //change username
                            await _customerRegistrationService.SetUsernameAsync(customer, userName);

                            //Update changes in customer changes table
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.CustomerTable, customer.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.Username"), oldUsername, userName);

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }
                    //email
                    var email = model.Email.Trim();
                    // Modified string comparison to handle potential null values.
                    // In default nopCommerce, comparing customer.Email with email using
                    // Equals() method can result in a NullReferenceException if customer.Email is null.
                    // The updated code uses string.Equals() which safely handles null values and
                    // performs a case-insensitive comparison using StringComparison.InvariantCultureIgnoreCase.
                    if (!string.Equals(customer.Email, email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var oldEmail = customer.Email;

                        #region Task 600 email verification 

                        var emailValid = await _userProfileAdditionalInfoService.VerifyEmailByApiAsync(email);
                        if (!emailValid)
                        {
                            throw new NopException(await _localizationService.GetResourceAsync("Annique.Plugin.EmailableApi.Validation.Message"));
                        }

                        #endregion

                        //change email
                        var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                        await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                        //Update changes in customer changes table
                        await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.CustomerTable, customer.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.Email"), oldEmail, email);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        //update time zone id
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.TimeZoneId"), customer.TimeZoneId.ToString(), model.TimeZoneId.ToString());

                        customer.TimeZoneId = model.TimeZoneId;
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = customer.VatNumber;
                        customer.VatNumber = model.VatNumber;

                        if (prevVatNumber != model.VatNumber)
                        {
                            //update vat number
                            UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.VatNumber"), prevVatNumber, model.VatNumber);

                            var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                            //update vatNumberStatus
                            UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.VatNumberStatusId"), customer.VatNumberStatusId.ToString(), vatNumberStatus.ToString());

                            customer.VatNumberStatusId = (int)vatNumberStatus;

                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                    model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                    {
                        var oldGender = customer.Gender;
                        if (oldGender != model.Gender)
                            //Update changes in customer changes table
                            await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.CustomerTable, customer.Id, await _localizationService.GetResourceAsync("CustomerChanges.Field.Gender"), oldGender, model.Gender);

                        customer.Gender = model.Gender;
                    }

                    if (_customerSettings.FirstNameEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.FirstName"), customer.FirstName, model.FirstName);
                        customer.FirstName = model.FirstName.Trim();
                    }

                    if (_customerSettings.LastNameEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.LastName"), customer.LastName, model.LastName);
                        customer.LastName = model.LastName;
                    }
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.DateOfBirth"), customer.DateOfBirth.ToString(), model.ParseDateOfBirth().ToString());
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    }
                    if (_customerSettings.CompanyEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.Company"), customer.Company, model.Company);
                        customer.Company = model.Company;
                    }
                    if (_customerSettings.StreetAddressEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.StreetAddress"), customer.StreetAddress, model.StreetAddress);
                        customer.StreetAddress = model.StreetAddress;
                    }

                    if (_customerSettings.StreetAddress2Enabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.StreetAddress2"), customer.StreetAddress2, model.StreetAddress2);
                        customer.StreetAddress2 = model.StreetAddress2;
                    }

                    if (_customerSettings.ZipPostalCodeEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.ZipPostalCode"), customer.ZipPostalCode, model.ZipPostalCode);
                        customer.ZipPostalCode = model.ZipPostalCode;
                    }

                    if (_customerSettings.CityEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.City"), customer.City, model.City);
                        customer.City = model.City;
                    }
                    if (_customerSettings.CountyEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.County"), customer.County, model.County);
                        customer.County = model.County;
                    }
                    if (_customerSettings.CountryEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.CountryId"), customer.CountryId.ToString(), model.CountryId.ToString());
                        customer.CountryId = model.CountryId;
                    }
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.StateProvinceId"), customer.StateProvinceId.ToString(), model.StateProvinceId.ToString());
                        customer.StateProvinceId = model.StateProvinceId;
                    }
                    if (_customerSettings.PhoneEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.Phone"), customer.Phone, model.Phone);

                        if (!string.IsNullOrWhiteSpace(model.Phone))
                        {
                            var trimmedModelPhone = model.Phone.Trim();
                            var trimmedCustomerPhone = customer.Phone?.Trim();

                            // If phone has changed or customer had no phone before
                            if (!string.Equals(trimmedModelPhone, trimmedCustomerPhone, StringComparison.Ordinal))
                            {
                                await _userProfileAdditionalInfoService.ValidatePhoneNumberAsync(
                                    trimmedModelPhone,
                                    customer.Id,
                                    "Annique.Plugin.PhoneNumber.Validation.Message"
                                );
                            }
                        }
                        customer.Phone = model.Phone?.Trim();
                    }
                    if (_customerSettings.FaxEnabled)
                    {
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.Fax"), customer.Fax, model.Fax);
                        customer.Fax = model.Fax;
                    }

                    customer.CustomCustomerAttributesXML = customerAttributesXml;

                    #region task 610 Format phone number for sendinblue plugin

                    //to format phone number country id is required so setting country id for customer
                    await _userProfileAdditionalInfoService.SetDefaultCountryAsync(customer);

                    #endregion

                    await _customerService.UpdateCustomerAsync(customer);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //Update changes in customer changes table
                        if (oldCustomerModel.Newsletter != model.Newsletter)
                            UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, await _localizationService.GetResourceAsync("CustomerChanges.Field.Newsletter"), oldCustomerModel.Newsletter.ToString(), model.Newsletter.ToString());

                        //save newsletter value
                        var store = await _storeContext.GetCurrentStoreAsync();
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                            }
                            else
                            {
                                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = store.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                    {
                        var oldSignature = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SignatureAttribute);
                        //Update changes in customer changes table
                        UpdateCustomerChanges(customer, AnniqueCustomizationDefaults.CustomerTable, NopCustomerDefaults.SignatureAttribute, oldSignature, model.Signature);

                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);
                    }

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                        await LogGdprAsync(customer, oldCustomerModel, model, form);

                    //if customer come here so far without any errors and user profile terms and condition also accepted then update flag profile updated
                    if (userProfileInfo.Accept)
                        userProfileInfo.ProfileUpdated = true;
                    else
                        userProfileInfo.ProfileUpdated = false;

                    await _userProfileAdditionalInfoService.UpdateUserAdditionalInfoAsync(userProfileInfo);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.CustomerInfo.Updated"));

                    return RedirectToRoute("CustomerInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true, customerAttributesXml);

            return View(model);
        }

        //method to redirect on public store and signIn with provied customer
        public virtual async Task<IActionResult> PublicSiteLogin(string username)
        {
            var returnUrl = Url.RouteUrl("Homepage");
            if (!string.IsNullOrEmpty(username))
            {
                //get customer by username
                var customer = await _customerService.GetCustomerByUsernameAsync(username);
                //customer exist
                if (customer != null)
                {
                    //sign in with customer
                    await _userProfileAdditionalInfoService.SignInCustomerAsync(customer, returnUrl, false);
                }
                else
                {
                    //if no customer found with provided username then return back with error message 
                    return Json(new
                    {
                        success = false,
                        message = await _localizationService.GetResourceAsync("Public.Username.NotExist")
                    });
                }
            }

            //return to public site
            return Json(new
            {
                success = true,
                url = returnUrl
            });
        }

        public async Task<IActionResult> CustomMyAccountPage()
        {
            var storeScope = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

            //Get current customer and customer roles
            var customer = await _workContext.GetCurrentCustomerAsync();

            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            //if not conusultant role then return to 404 page 
            if (!customerRoleIds.Contains(settings.ConsultantRoleId))
                return InvokeHttp404();

            //show custom myaccount view to consultant users
            return View();
        }

        #endregion

        #region OTP methods

        [HttpPost]
        [ValidateAntiForgeryToken]
        //send OTP method
        public async Task<IActionResult> SendOtp(string sendVia)
        {
            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();

                //get current user
                var customer = await _workContext.GetCurrentCustomerAsync();

                //check current customer is registered user or not
                var isCurrentUserRegistered = await _customerService.IsRegisteredAsync(customer);
                if (!isCurrentUserRegistered)
                {
                    return Json(new
                    {
                        success = false,
                        message = "User is not registered.",
                    });
                }

                var (success, message) = await _otpService.SendOTPAsync(store.Id, customer.Id, sendVia);
                if (success)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message });
                }
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpPost]
        //Verify OTP method
        public async Task<IActionResult> VerifyOTP(string enteredOTP)
        {
            try
            {
                //get current customer
                var customer = await _workContext.GetCurrentCustomerAsync();

                //check current customer is registered user or not
                var isCurrentUserRegistered = await _customerService.IsRegisteredAsync(customer);
                if (!isCurrentUserRegistered)
                {
                    return Json(new { success = false });
                }

                // Check if the entered OTP matches the stored OTP for the customer
                bool isOTPValid = _otpService.VerifyOTP(customer.Id, enteredOTP);

                if (isOTPValid)
                {
                    // Load the OTP record for the customer
                    var otpRecord = await _otpService.GetCustomerOtpRecordAsync(customer.Id);

                    //OTP verified so update verified flag
                    otpRecord.Iverified = true;

                    //update in table
                    await _otpService.UpdateOtpRecordAsync(otpRecord);

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        #endregion

        #region Password reset method

        [HttpPost]
        public virtual async Task<IActionResult> SendPasswordResetLinkViaSms(string Email)
        {
            try
            {
                //load settings
                var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(_storeContext.GetCurrentStoreAsync().Id);
                if (!anniqueSettings.IsPasswordResetEnabled)
                    return Json(new { success = false, message = await _localizationService.GetResourceAsync("Account.PasswordRecovery.SmsError") });

                var customer = await _customerService.GetCustomerByEmailAsync(Email?.Trim());
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    //send email
                    var smsSent = await _userProfileAdditionalInfoService.SendCustomerPasswordRecoverySmsAsync(customer,
                        (await _workContext.GetWorkingLanguageAsync()).Id);

                    if (smsSent)
                        return Json(new { success = true, message = await _localizationService.GetResourceAsync("Account.PasswordRecovery.SmsHasBeenSent") });
                    else
                        return Json(new { success = false, message = await _localizationService.GetResourceAsync("Account.PasswordRecovery.SmsError") });
                }
                else
                {
                    return Json(new { success = false, message = await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound") });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("Account.PasswordRecovery.SmsError") });
            }
        }

        #endregion
    }
}
