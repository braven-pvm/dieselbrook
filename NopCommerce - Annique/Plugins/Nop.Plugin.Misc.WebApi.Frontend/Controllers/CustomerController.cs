using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Framework.Models;
using Nop.Plugin.Misc.WebApi.Frontend.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer;
using Nop.Plugin.Misc.WebApi.Frontend.Models;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class CustomerController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IExportManager _exportManager;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly TaxSettings _taxSettings;

        #endregion

        #region Ctor

        public CustomerController(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAuthenticationService authenticationService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            ILogger logger,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IOrderService orderService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            TaxSettings taxSettings)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _exportManager = exportManager;
            _externalAuthenticationService = externalAuthenticationService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _logger = logger;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _notificationService = notificationService;
            _orderService = orderService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _mediaSettings = mediaSettings;
            _taxSettings = taxSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get custom address attributes from the passed form
        /// </summary>
        /// <param name="form">Form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the attributes in XML format
        /// </returns>
        protected virtual async Task<string> ParseCustomAddressAttributesAsync(IDictionary<string, string> form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;

            foreach (var attribute in await _addressAttributeService.GetAllAddressAttributesAsync())
            {
                var controlId = string.Format(NopCommonDefaults.AddressAttributeControlName, attribute.Id);
                var attributeValues = form[controlId];
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        if (!StringValues.IsNullOrEmpty(attributeValues) && int.TryParse(attributeValues, out var value) && value > 0)
                            attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml, attribute, value.ToString());
                        break;

                    case AttributeControlType.Checkboxes:
                        foreach (var attributeValue in attributeValues.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            if (int.TryParse(attributeValue, out value) && value > 0)
                                attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml, attribute, value.ToString());

                        break;

                    case AttributeControlType.ReadonlyCheckboxes:
                        //load read-only (already server-side selected) values
                        var addressAttributeValues = await _addressAttributeService.GetAddressAttributeValuesAsync(attribute.Id);
                        foreach (var addressAttributeValue in addressAttributeValues)
                            if (addressAttributeValue.IsPreSelected)
                                attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml, attribute, addressAttributeValue.Id.ToString());

                        break;

                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        if (!StringValues.IsNullOrEmpty(attributeValues))
                            attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml, attribute, attributeValues.Trim());
                        break;

                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IDictionary<string, string> form)
        {
            foreach (var consent in consents)
            {
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.Equals("on")) 
                    ModelState.AddModelError(string.Empty, consent.RequiredMessage);
            }
        }
        
        protected virtual async Task<string> ParseSelectedProviderAsync(IDictionary<string, string> form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));
            var store = await _storeContext.GetCurrentStoreAsync();
            var multiFactorAuthenticationProviders = await _multiFactorAuthenticationPluginManager.LoadActivePluginsAsync(await _workContext.GetCurrentCustomerAsync(), store.Id);
            foreach (var provider in multiFactorAuthenticationProviders)
            {
                var controlId = $"provider_{provider.PluginDescriptor.SystemName}";

                var curProvider = form[controlId];
                if (!StringValues.IsNullOrEmpty(curProvider))
                {
                    var selectedProvider = curProvider;
                    if (!string.IsNullOrEmpty(selectedProvider)) return selectedProvider;
                }
            }

            return string.Empty;
        }
        
        protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IDictionary<string, string> form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
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
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
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
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.Trim();
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
            CustomerInfoModel newCustomerInfoModel, IDictionary<string, string> form)
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
                    if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.Equals("on"))
                    {
                        //agree
                        if (!previousConsentValue.HasValue || !previousConsentValue.Value) await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                    }
                    else
                    {
                        //disagree
                        if (!previousConsentValue.HasValue || previousConsentValue.Value) await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
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

        #region Methods

        #region Login / logout

        /// <summary>
        /// Login
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Login([FromBody] LoginCustomerRequest request)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var username = _customerSettings.UsernamesEnabled ? request.Username : request.Email;
            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(username, request.Password);

            if (loginResult == CustomerLoginResults.Successful)
            {
                var customer = await (_customerSettings.UsernamesEnabled
                    ? _customerService.GetCustomerByUsernameAsync(username)
                    : _customerService.GetCustomerByEmailAsync(username));


                if (currentCustomer?.Id != customer.Id)
                {
                    //migrate shopping cart
                    await _shoppingCartService.MigrateShoppingCartAsync(currentCustomer, customer, true);

                    await _workContext.SetCurrentCustomerAsync(customer);
                }

                //sign in new customer
                await _authenticationService.SignInAsync(customer, true);

                //raise event
                await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                //activity log
                await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                    await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

                return Ok(true);
            }
            
            return BadRequest("Username or password is incorrect");
        }

        /// <summary>
        /// Logout
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Logout()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //activity log
                await _customerActivityService.InsertActivityAsync(_workContext.OriginalCustomerIfImpersonated,
                    "Impersonation.Finished",
                    string.Format(
                        await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.StoreOwner"),
                        customer.Email,
                        customer.Id),
                    customer);

                await _customerActivityService.InsertActivityAsync("Impersonation.Finished",
                    string.Format(
                        await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.Customer"),
                        _workContext.OriginalCustomerIfImpersonated.Email,
                        _workContext.OriginalCustomerIfImpersonated.Id),
                    _workContext.OriginalCustomerIfImpersonated);

                //logout impersonated customer
                await _genericAttributeService
                    .SaveAttributeAsync<int?>(_workContext.OriginalCustomerIfImpersonated,
                        NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);
            }

            //activity log
            await _customerActivityService.InsertActivityAsync(customer,
                "PublicStore.Logout",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Logout"),
                customer);

            //standard logout 
            await _authenticationService.SignOutAsync();

            //raise logged out event       
            await _eventPublisher.PublishAsync(
                new CustomerLoggedOutEvent(customer));

            ////EU Cookie
            //if (_storeInformationSettings.DisplayEuCookieLawWarning)
            //{
            //    //the cookie law message should not pop up immediately after logout.
            //    //otherwise, the user will have to click it again...
            //    //and thus next visitor will not click it... so violation for that cookie law..
            //    //the only good solution in this case is to store a temporary variable
            //    //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
            //    //but it'll be displayed for further page loads
            //    TempData[$"{NopCookieDefaults.Prefix}{NopCookieDefaults.IgnoreEuCookieLawWarning}"] = true;
            //}

            return Ok();
        }

        #endregion

        #region Password recovery

        /// <summary>
        /// Prepare the password recovery model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PasswordRecoveryModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PasswordRecovery()
        {
            var model = new PasswordRecoveryModel();
            model = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);

            return Ok(model.ToDto<PasswordRecoveryModelDto>());
        }

        /// <summary>
        /// Password recovery send
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PasswordRecoveryModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PasswordRecoverySend([FromBody] PasswordRecoveryModelDto model)
        {
            var passwordRecoveryModel = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model.FromDto<PasswordRecoveryModel>());
            var rezModel = passwordRecoveryModel.ToDto<PasswordRecoveryModelDto>();

            var customer = await _customerService.GetCustomerByEmailAsync(passwordRecoveryModel.Email.Trim());
            if (customer != null && customer.Active && !customer.Deleted)
            {
                //save token and current date
                var passwordRecoveryToken = Guid.NewGuid();
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                    passwordRecoveryToken.ToString());
                DateTime? generatedDateTime = DateTime.UtcNow;
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                //send email
                await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                    (await _workContext.GetWorkingLanguageAsync()).Id);

                rezModel.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent");
            }
            else
                rezModel.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound");
            
            return Ok(rezModel);
        }

        /// <summary>
        /// Password recovery confirm
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PasswordRecoveryConfirmModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PasswordRecoveryConfirm([FromQuery, Required] string token,
            [FromQuery, Required] string email,
            [FromQuery, Required] Guid customerGuid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email)
                           ?? await _customerService.GetCustomerByGuidAsync(customerGuid);

            if (customer == null)
                return NotFound($"Customer by guid={customerGuid} or email={email} not found.");

            var model = new PasswordRecoveryConfirmModel {ReturnUrl = Url.RouteUrl("Homepage")};
            if (string.IsNullOrEmpty(await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.PasswordRecoveryTokenAttribute)))
            {
                model.DisablePasswordChanging = true;
                model.Result =
                    await _localizationService.GetResourceAsync(
                        "Account.PasswordRecovery.PasswordAlreadyHasBeenChanged");
                return Ok(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            //validate token
            if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken");
                return Ok(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            //validate token expiration date
            if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired");
                return Ok(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            return Ok(model.ToDto<PasswordRecoveryConfirmModelDto>());
        }

        /// <summary>
        /// Password recovery confirm post
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PasswordRecoveryConfirmModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PasswordRecoveryConfirmPOST(
            [FromBody] PasswordRecoveryConfirmModelDto passwordRecoveryConfirmModel,
            [FromQuery, Required] string token,
            [FromQuery, Required] string email,
            [FromQuery, Required] Guid customerGuid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email)
                           ?? await _customerService.GetCustomerByGuidAsync(customerGuid);
            if (customer == null)
                return NotFound($"Customer by guid={customerGuid} or email={email} not found.");

            var model = passwordRecoveryConfirmModel.FromDto<PasswordRecoveryConfirmModelDto>();

            model.ReturnUrl = Url.RouteUrl("Homepage");

            //validate token
            if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken");
                return Ok(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            //validate token expiration date
            if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired");
                return Ok(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            var response = await _customerRegistrationService
                .ChangePasswordAsync(new ChangePasswordRequest(customer.Email, false,
                    _customerSettings.DefaultPasswordFormat, model.NewPassword));
            if (!response.Success)
            {
                model.Result = string.Join(';', response.Errors);
                return Ok(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.PasswordRecoveryTokenAttribute, string.Empty);

            //authenticate customer after changing password
            await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

            model.DisablePasswordChanging = true;
            model.Result =
                await _localizationService.GetResourceAsync("Account.PasswordRecovery.PasswordHasBeenChanged");
            return Ok(model.ToDto<PasswordRecoveryConfirmModelDto>());
        }

        #endregion     

        #region Register

        /// <summary>
        ///  Prepare the customer register model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RegisterModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Register()
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return NotFound($"User registration type={UserRegistrationType.Disabled}");

            var model = new RegisterModel();
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);

            return Ok(model.ToDto<RegisterModelDto>());
        }

        /// <summary>
        /// Register
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RegisterModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Register([FromBody] BaseModelDtoRequest<RegisterModelDto> request)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return NotFound($"User registration type={UserRegistrationType.Disabled}");
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (await _customerService.IsRegisteredAsync(customer))
            {
                //Already registered customer. 
                await _authenticationService.SignOutAsync();

                //raise logged out event       
                await _eventPublisher.PublishAsync(
                    new CustomerLoggedOutEvent(customer));

                //Save a new record
                await _workContext.SetCurrentCustomerAsync(await _customerService.InsertGuestCustomerAsync());
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            customer.RegisteredInStoreId = store.Id;

            var errors = new List<string>();

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(request.Form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            errors.AddRange(customerAttributeWarnings);

            if (errors.Any())
                return BadRequest(errors);

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                        .GetAllConsentsAsync())
                    .Where(consent => consent.DisplayDuringRegistration && consent.IsRequired)
                    .ToList();

                ValidateRequiredConsents(consents, request.Form);
            }

            var model = request.Model.FromDto<RegisterModel>();

            var customerUserName = model.Username?.Trim();
            var customerEmail = model.Email?.Trim();

            var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
            var registrationRequest = new CustomerRegistrationRequest(customer,
                customerEmail,
                _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                model.Password,
                _customerSettings.DefaultPasswordFormat,
                store.Id,
                isApproved);
            var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
            if (registrationResult.Success)
            {
                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = model.TimeZoneId;

                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    customer.VatNumber = model.VatNumber;

                    var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                    customer.VatNumberStatusId = (int)vatNumberStatus;
                    //send VAT number admin notification
                    if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                        await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                            model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    customer.Gender = model.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = model.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = model.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = model.ParseDateOfBirth();
                if (_customerSettings.CompanyEnabled)
                    customer.Company = model.Company;
                if (_customerSettings.StreetAddressEnabled)
                    customer.StreetAddress = model.StreetAddress;
                if (_customerSettings.StreetAddress2Enabled)
                    customer.StreetAddress2 = model.StreetAddress2;
                if (_customerSettings.ZipPostalCodeEnabled)
                    customer.ZipPostalCode = model.ZipPostalCode;
                if (_customerSettings.CityEnabled)
                    customer.City = model.City;
                if (_customerSettings.CountyEnabled)
                    customer.County = model.County;
                if (_customerSettings.CountryEnabled)
                    customer.CountryId = model.CountryId;
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    customer.StateProvinceId = model.StateProvinceId;
                if (_customerSettings.PhoneEnabled)
                    customer.Phone = model.Phone;
                if (_customerSettings.FaxEnabled)
                    customer.Fax = model.Fax;

                //save customer attributes
                customer.CustomCustomerAttributesXML = customerAttributesXml;
                await _customerService.UpdateCustomerAsync(customer);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    var isNewsletterActive =
                        _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;

                    //save newsletter value
                    var newsletter =
                        await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(
                            customerEmail, store.Id);
                    if (newsletter != null)
                    {
                        if (model.Newsletter)
                        {
                            newsletter.Active = isNewsletterActive;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree,
                                    await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                        }
                        //else
                        //{
                        //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                        //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        //}
                    }
                    else
                    {
                        if (model.Newsletter)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(
                                new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customerEmail,
                                    Active = isNewsletterActive,
                                    StoreId = store.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree,
                                    await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                        }
                    }
                }

                if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    //privacy policy is required
                    //GDPR
                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree,
                            await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));

                //GDPR
                if (_gdprSettings.GdprEnabled)
                {
                    var consents = (await _gdprService.GetAllConsentsAsync())
                        .Where(consent => consent.DisplayDuringRegistration).ToList();
                    foreach (var consent in consents)
                    {
                        var controlId = $"consent{consent.Id}";
                        var cbConsent = request.Form[controlId];
                        if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.Equals("on"))
                            //agree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree,
                                consent.Message);
                        else
                            //disagree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree,
                                consent.Message);
                    }
                }

                //insert default address (if possible)
                //insert default address (if possible)
                var defaultAddress = new Address
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Company = customer.Company,
                    CountryId = customer.CountryId > 0
                        ? (int?)customer.CountryId
                        : null,
                    StateProvinceId = customer.StateProvinceId > 0
                        ? (int?)customer.StateProvinceId
                        : null,
                    County = customer.County,
                    City = customer.City,
                    Address1 = customer.StreetAddress,
                    Address2 = customer.StreetAddress2,
                    ZipPostalCode = customer.ZipPostalCode,
                    PhoneNumber = customer.Phone,
                    FaxNumber = customer.Fax,
                    CreatedOnUtc = customer.CreatedOnUtc
                };

                if (await _addressService.IsAddressValidAsync(defaultAddress))
                {
                    //some validation
                    if (defaultAddress.CountryId == 0)
                        defaultAddress.CountryId = null;
                    if (defaultAddress.StateProvinceId == 0)
                        defaultAddress.StateProvinceId = null;
                    //set default address
                    //customer.Addresses.Add(defaultAddress);

                    await _addressService.InsertAddressAsync(defaultAddress);

                    await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                    customer.BillingAddressId = defaultAddress.Id;
                    customer.ShippingAddressId = defaultAddress.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }

                //notifications
                if (_customerSettings.NotifyNewCustomerRegistration)
                    await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                        _localizationSettings.DefaultAdminLanguageId);

                //raise event       
                await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));

                switch (_customerSettings.UserRegistrationType)
                {
                    case UserRegistrationType.EmailValidation:
                        return NotFound($"This registration type '{nameof(UserRegistrationType.EmailValidation)}' is not supported for Web API.");

                    case UserRegistrationType.AdminApproval:
                        return NotFound($"This registration type '{nameof(UserRegistrationType.AdminApproval)}' is not supported for Web API.");

                    case UserRegistrationType.Standard:
                        //send customer welcome message
                        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer,
                            (await _workContext.GetWorkingLanguageAsync()).Id);

                        //raise event       
                        await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                        if (currentCustomer?.Id != customer.Id)
                        {
                            //migrate shopping cart
                            await _shoppingCartService.MigrateShoppingCartAsync(currentCustomer, customer, true);

                            await _workContext.SetCurrentCustomerAsync(customer);
                        }

                        //sign in new customer
                        await _authenticationService.SignInAsync(customer, true);

                        //raise event       
                        await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                        //activity log
                        await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

                        break;

                    default:
                        return BadRequest(new List<string> { $"Unknow {_customerSettings.UserRegistrationType}" });
                }
            }

            //errors
            if (registrationResult.Errors.Any())
                return BadRequest(registrationResult.Errors);

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, true, customerAttributesXml);

            return Ok(model.ToDto<RegisterModelDto>());
        }

        /// <summary>
        /// Prepare the register result model
        /// </summary>
        [HttpPost("{resultId}")]
        [ProducesResponseType(typeof(RegisterResultModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RegisterResult(int resultId, [FromQuery][Required] string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            var model = await _customerModelFactory.PrepareRegisterResultModelAsync(resultId, returnUrl);
            return Ok(model.ToDto<RegisterResultModelDto>());
        }

        /// <summary>
        /// Check Username availability
        /// </summary>
        /// <param name="username">Username</param>
        [HttpGet]
        [ProducesResponseType(typeof(CheckUsernameAvailabilityResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckUsernameAvailability([FromQuery][Required] string username)
        {
            var usernameAvailable = false;
            var statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.NotAvailable");

            if (!UsernamePropertyValidator<string, string>.IsValid(username, _customerSettings))
                statusText = await _localizationService.GetResourceAsync("Account.Fields.Username.NotValid");
            else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                if (customer != null &&
                    customer.Username != null &&
                    customer.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                    statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.CurrentUsername");
                else
                {
                    if (customer == null)
                    {
                        statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.Available");
                        usernameAvailable = true;
                    }
                }
            }

            return Ok(new CheckUsernameAvailabilityResponse { Available = usernameAvailable, Text = statusText });
        }

        /// <summary>
        /// Account activation
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AccountActivationModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AccountActivation([FromQuery, Required] string token,
            [FromQuery, Required] string email,
            [FromQuery, Required] Guid customerGuid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email)
                           ?? await _customerService.GetCustomerByGuidAsync(customerGuid);

            if (customer == null)
                return NotFound($"Customer by guid={customerGuid} or email={email} not found.");

            var model = new AccountActivationModel {ReturnUrl = Url.RouteUrl("Homepage")};
            var cToken = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.AccountActivationTokenAttribute);
            if (string.IsNullOrEmpty(cToken))
            {
                model.Result =
                    await _localizationService.GetResourceAsync("Account.AccountActivation.AlreadyActivated");
                return Ok(model.ToDto<AccountActivationModelDto>());
            }

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest("No match was found for the token for the specified customer.");

            //activate user account
            customer.Active = true;
            await _customerService.UpdateCustomerAsync(customer);
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.AccountActivationTokenAttribute, string.Empty);

            //send welcome message
            await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer,
                (await _workContext.GetWorkingLanguageAsync()).Id);

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

            //authenticate customer after activation
            await _customerRegistrationService.SignInCustomerAsync(customer, null, true);
            var store = await _storeContext.GetCurrentStoreAsync();

            //activating newsletter if need
            var newsletter =
                await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email,
                    store.Id);
            if (newsletter != null && !newsletter.Active)
            {
                newsletter.Active = true;
                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
            }

            model.Result = await _localizationService.GetResourceAsync("Account.AccountActivation.Activated");
            return Ok(model.ToDto<AccountActivationModelDto>());
        }

        #endregion

        #region My account / Info

        /// <summary>
        /// Prepare the customer info model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerInfoModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Info()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            var model = new CustomerInfoModel();
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, false);

            return Ok(model.ToDto<CustomerInfoModelDto>());
        }

        /// <summary>
        /// Customer info
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(InfoResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Info([FromBody] BaseModelDtoRequest<CustomerInfoModelDto> request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            var oldCustomerModel = new CustomerInfoModel();

            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(request.Form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);

            var errors = new List<string>();
            errors.AddRange(customerAttributeWarnings);
            
            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, request.Form);
            }

            var model = request.Model.FromDto<CustomerInfoModel>();

            try
            {
                //username 
                if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                {
                    var userName = model.Username.Trim();
                    if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change username
                        await _customerRegistrationService.SetUsernameAsync(customer, userName);

                        //re-authenticate
                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                            await _authenticationService.SignInAsync(customer, true);
                    }
                }
                //email
                var email = model.Email.Trim();
                if (!customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                {
                    //change email
                    var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                    await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                    //do not authenticate users in impersonation mode
                    if (_workContext.OriginalCustomerIfImpersonated == null)
                    //re-authenticate (if usernames are disabled)
                        if (!_customerSettings.UsernamesEnabled && !requireValidation)
                            await _authenticationService.SignInAsync(customer, true);
                }

                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = model.TimeZoneId;
                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    var prevVatNumber = customer.VatNumber;

                    customer.VatNumber = model.VatNumber;

                    if (prevVatNumber != model.VatNumber)
                    {
                        var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                        customer.VatNumberStatusId = (int)vatNumberStatus;
                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    customer.Gender = model.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = model.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = model.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = model.ParseDateOfBirth();
                if (_customerSettings.CompanyEnabled)
                    customer.Company = model.Company;
                if (_customerSettings.StreetAddressEnabled)
                    customer.StreetAddress = model.StreetAddress;
                if (_customerSettings.StreetAddress2Enabled)
                    customer.StreetAddress2 = model.StreetAddress2;
                if (_customerSettings.ZipPostalCodeEnabled)
                    customer.ZipPostalCode = model.ZipPostalCode;
                if (_customerSettings.CityEnabled)
                    customer.City = model.City;
                if (_customerSettings.CountyEnabled)
                    customer.County = model.County;
                if (_customerSettings.CountryEnabled)
                    customer.CountryId = model.CountryId;
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    customer.StateProvinceId = model.StateProvinceId;
                if (_customerSettings.PhoneEnabled)
                    customer.Phone = model.Phone;
                if (_customerSettings.FaxEnabled)
                    customer.Fax = model.Fax;

                customer.CustomCustomerAttributesXML = customerAttributesXml;
                await _customerService.UpdateCustomerAsync(customer);

                var store = await _storeContext.GetCurrentStoreAsync();

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    //save newsletter value
                    var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                    if (newsletter != null)
                    {
                        if (model.Newsletter)
                        {
                            newsletter.Active = true;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                        }
                        else
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                    }
                    else
                    {
                        if (model.Newsletter)
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

                if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);
                
                //GDPR
                if (_gdprSettings.GdprEnabled)
                    await LogGdprAsync(customer, oldCustomerModel, model, request.Form);

                return Ok(new InfoResponse { 
                    Errors = errors,
                    Model = model.ToDto<CustomerInfoModelDto>() 
                });
            }
            catch (Exception exc)
            {
                errors.Add(exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true, customerAttributesXml);

            return Ok(new InfoResponse
            {
                Errors = errors,
                Model = model.ToDto<CustomerInfoModelDto>()
            });
        }

        /// <summary>
        /// Delete the external authentication record
        /// </summary>
        /// <param name="id">External authentication record identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveExternalAssociation(int id)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return BadRequest("Customer is not registered.");

            //ensure it's our record
            var ear = await _externalAuthenticationService.GetExternalAuthenticationRecordByIdAsync(id);

            if (ear == null)
                return Ok(Url.Action("Info"));

            await _externalAuthenticationService.DeleteExternalAuthenticationRecordAsync(ear);

            return Ok(Url.Action("Info"));
        }

        /// <summary>
        /// Email revalidation
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EmailRevalidationModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> EmailRevalidation([FromQuery] [Required] string token,
            [FromQuery] [Required] string email,
            [FromQuery] [Required] Guid customerGuid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email)
                           ?? await _customerService.GetCustomerByGuidAsync(customerGuid);

            if (customer == null)
                return NotFound($"Customer by guid={customerGuid} or email={email} not found.");

            var model = new EmailRevalidationModel {ReturnUrl = Url.RouteUrl("Homepage")};
            var cToken = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.EmailRevalidationTokenAttribute);
            if (string.IsNullOrEmpty(cToken))
            {
                model.Result = await _localizationService.GetResourceAsync("Account.EmailRevalidation.AlreadyChanged");
                return Ok(model.ToDto<EmailRevalidationModelDto>());
            }

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest("No match was found for the token for the specified customer.");

            if (string.IsNullOrEmpty(customer.EmailToRevalidate))
                return NotFound("Email to revalidate is not found for current customer.");

            if (_customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
                return BadRequest(
                    $"The setting {nameof(_customerSettings.UserRegistrationType)} is not equal {UserRegistrationType.EmailValidation}");

            //change email
            try
            {
                await _customerRegistrationService.SetEmailAsync(customer, customer.EmailToRevalidate, false);
            }
            catch (Exception exc)
            {
                model.Result = await _localizationService.GetResourceAsync(exc.Message);
                return Ok(model.ToDto<EmailRevalidationModelDto>());
            }

            customer.EmailToRevalidate = null;
            await _customerService.UpdateCustomerAsync(customer);
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.EmailRevalidationTokenAttribute, string.Empty);

            //authenticate customer after changing email
            await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

            model.Result = await _localizationService.GetResourceAsync("Account.EmailRevalidation.Changed");
            return Ok(model.ToDto<EmailRevalidationModelDto>());
        }

        #endregion

        #region My account / Addresses

        /// <summary>
        /// Prepare the customer address list model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAddressListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Addresses()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return BadRequest("Customer is not registered.");

            var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync();

            return Ok(model.ToDto<CustomerAddressListModelDto>());
        }

        /// <summary>
        /// Address delete
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        [HttpDelete("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressDelete(int addressId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address != null)
            {
                await _customerService.RemoveCustomerAddressAsync(customer, address);
                await _customerService.UpdateCustomerAsync(customer);
                //now delete the address record
                await _addressService.DeleteAddressAsync(address);
            }

            //redirect to the address list page
            return Ok(Url.RouteUrl("CustomerAddresses"));
        }

        /// <summary>
        /// Prepare address model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAddressEditModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressAdd()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return BadRequest("Customer is not registered.");

            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            return Ok(model.ToDto<CustomerAddressEditModelDto>());
        }

        /// <summary>
        /// Address add
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddressAddResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressAdd([FromBody] BaseModelDtoRequest<CustomerAddressEditModelDto> request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            //custom address attributes
            var customAttributes = await ParseCustomAddressAttributesAsync(request.Form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);

            var model = request.Model.FromDto<CustomerAddressEditModel>();
            
            if (!customAttributeWarnings.Any())
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.InsertAddressAsync(address);

                await _customerService.InsertCustomerAddressAsync(customer, address);

                return Ok(new AddressAddResponse()
                {
                    Model = model.ToDto<CustomerAddressEditModelDto>()
                });
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            return Ok(new AddressAddResponse()
            {
                Model = model.ToDto<CustomerAddressEditModelDto>(),
                Errors = customAttributeWarnings
            });
        }

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAddressEditModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressEdit(int addressId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return NotFound($"Address by id={addressId} is not found.");

            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            return Ok(model.ToDto<CustomerAddressEditModelDto>());
        }

        /// <summary>
        /// Update address
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddressEditResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressEdit([FromBody] BaseModelDtoRequest<CustomerAddressEditModelDto> request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            var model = request.Model.FromDto<CustomerAddressEditModel>();

            var addressId = model.Address.Id;

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return NotFound($"Address by id={addressId} is not found.");

            //custom address attributes
            var customAttributes = await ParseCustomAddressAttributesAsync(request.Form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            
            if (!customAttributeWarnings.Any())
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                await _addressService.UpdateAddressAsync(address);

                return Ok(new AddressEditResponse()
                {
                    Model = model.ToDto<CustomerAddressEditModelDto>()
                });
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            return Ok(new AddressEditResponse()
            {
                Model = model.ToDto<CustomerAddressEditModelDto>(),
                Errors = customAttributeWarnings
            });
        }

        #endregion

        #region My account / Downloadable products

        /// <summary>
        /// Prepare the customer downloadable products model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerDownloadableProductsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> DownloadableProducts()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return BadRequest("Customer is not registered.");

            if (_customerSettings.HideDownloadableProductsTab)
                return NotFound($"The setting {nameof(_customerSettings.HideDownloadableProductsTab)} is true.");

            var model = await _customerModelFactory.PrepareCustomerDownloadableProductsModelAsync();

            return Ok(model.ToDto<CustomerDownloadableProductsModelDto>());
        }

        /// <summary>
        /// Prepare the user agreement model
        /// </summary>
        /// <param name="orderItemId">Order item guid identifier</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UserAgreementModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UserAgreement([FromQuery][Required] Guid orderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemId);
            if (orderItem == null)
                return NotFound($"Order item by guid={orderItemId} is not found.");

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            if (product == null || !product.HasUserAgreement)
                return NotFound($"Produc by id={orderItem.ProductId} is not found.");

            var model = await _customerModelFactory.PrepareUserAgreementModelAsync(orderItem, product);

            return Ok(model.ToDto<UserAgreementModelDto>());
        }

        #endregion

        #region My account / Change password

        /// <summary>
        /// Prepare the change password model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ChangePasswordModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ChangePassword()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            var model = await _customerModelFactory.PrepareChangePasswordModelAsync();

            //display the cause of the change password 
            if (await _customerService.IsPasswordExpiredAsync(customer))
                return BadRequest(await _localizationService.GetResourceAsync("Account.ChangePassword.PasswordIsExpired"));

            return Ok(model.ToDto<ChangePasswordModelDto>());
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ChangePasswordModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModelDto model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest(new List<string> { "Customer is not registered." });
            
            var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
            var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
            if (changePasswordResult.Success)
            {
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.ChangePassword.Success"));
                return Ok(model);
            }

            //errors
            var errors = changePasswordResult.Errors;
            if (errors.Any())
                return BadRequest(errors);

            //If we got this far, something failed, redisplay form
            return Ok(model);
        }

        #endregion

        #region My account / Avatar

        /// <summary>
        /// Prepare the customer avatar model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAvatarModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Avatar()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return BadRequest("Customer is not registered.");

            var model = new CustomerAvatarModel();

            if (_customerSettings.AllowCustomersToUploadAvatars)            
                model = await _customerModelFactory.PrepareCustomerAvatarModelAsync(model);

            return Ok(model.ToDto<CustomerAvatarModelDto>());
        }

        /// <summary>
        /// Upload avatar
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAvatarModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UploadAvatar(IFormFile fileBinary,
            [FromQuery, Required] string fileName,
            [FromQuery, Required] string contentType)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return NotFound($"The setting {nameof(_customerSettings.AllowCustomersToUploadAvatars)} = false");

            var customerAvatar = await _pictureService.GetPictureByIdAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute));
            if (fileBinary != null && !string.IsNullOrEmpty(fileName))
            {
                var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                if (fileBinary.Length > avatarMaxSize)
                    throw new NopException(string.Format(
                        await _localizationService.GetResourceAsync("Account.Avatar.MaximumUploadedFileSize"),
                        avatarMaxSize));

                var customerPictureBinary = new byte[fileBinary.Length]; 
                using var reader = fileBinary.OpenReadStream();
                await reader.ReadAsync(customerPictureBinary, 0, customerPictureBinary.Length);
                reader.Close();
                if (customerAvatar != null)
                    customerAvatar = await _pictureService.UpdatePictureAsync(customerAvatar.Id, customerPictureBinary,
                        contentType, null);
                else
                    customerAvatar = await _pictureService.InsertPictureAsync(customerPictureBinary, contentType, null);
            }

            var customerAvatarId = 0;
            if (customerAvatar != null)
                customerAvatarId = customerAvatar.Id;

            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute,
                customerAvatarId);

            var avatarUrl = await _pictureService.GetPictureUrlAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute),
                _mediaSettings.AvatarPictureSize,
                false);

            return Ok(new CustomerAvatarModelDto {AvatarUrl = avatarUrl});
        }

        /// <summary>
        /// Remove avatar
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveAvatar()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return NotFound($"The setting {nameof(_customerSettings.AllowCustomersToUploadAvatars)} = false");

            var customerAvatar = await _pictureService.GetPictureByIdAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
            if (customerAvatar != null)
                await _pictureService.DeletePictureAsync(customerAvatar);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute, 0);

            return Ok();
        }

        #endregion

        #region GDPR tools

        /// <summary>
        /// Prepare the GDPR tools model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GdprToolsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GdprTools()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return BadRequest("Customer is not registered.");

            if (!_gdprSettings.GdprEnabled)
                return NotFound($"The setting {nameof(_gdprSettings.GdprEnabled)} = false");

            var model = await _customerModelFactory.PrepareGdprToolsModelAsync();

            return Ok(model.ToDto<GdprToolsModelDto>());
        }

        /// <summary>
        /// Export customer info (GDPR request) to XLSX
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GdprToolsExport()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            if (!_gdprSettings.GdprEnabled)
                return NotFound($"The setting {nameof(_gdprSettings.GdprEnabled)} = false");

            //log
            await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ExportData, await _localizationService.GetResourceAsync("Gdpr.Exported"));
            var store = await _storeContext.GetCurrentStoreAsync();

            //export
            var bytes = await _exportManager.ExportCustomerGdprInfoToXlsxAsync(await _workContext.GetCurrentCustomerAsync(), store.Id);

            return File(bytes, MimeTypes.TextXlsx, "customerdata.xlsx");
        }

        /// <summary>
        /// Gdpr tools delete
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GdprToolsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GdprToolsDelete()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest("Customer is not registered.");

            if (!_gdprSettings.GdprEnabled)
                return NotFound($"The setting {nameof(_gdprSettings.GdprEnabled)} = false");

            //log
            await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.DeleteCustomer, await _localizationService.GetResourceAsync("Gdpr.DeleteRequested"));

            var model = await _customerModelFactory.PrepareGdprToolsModelAsync();
            model.Result = await _localizationService.GetResourceAsync("Gdpr.DeleteRequested.Success");

            return Ok(model.ToDto<GdprToolsModelDto>());
        }

        #endregion

        #region Check gift card balance

        /// <summary>
        /// Prepare the check gift card balance madel
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CheckGiftCardBalanceModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckGiftCardBalance()
        {
            if (!(_captchaSettings.Enabled && _customerSettings.AllowCustomersToCheckGiftCardBalance))
                return NotFound($"The setting {nameof(_captchaSettings.Enabled)} and setting {nameof(_customerSettings.AllowCustomersToCheckGiftCardBalance)} is false");

            var model = await _customerModelFactory.PrepareCheckGiftCardBalanceModelAsync();

            return Ok(model.ToDto<CheckGiftCardBalanceModelDto>());
        }

        /// <summary>
        /// Check GiftCard balance
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CheckGiftCardBalanceModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckBalance([FromBody] CheckGiftCardBalanceModelDto model)
        {
            var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: model.GiftCardCode)).FirstOrDefault();
            if (giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard))
            {
                var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard), await _workContext.GetWorkingCurrencyAsync());
                model.Result = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);
            }
            else
                model.Message = await _localizationService.GetResourceAsync("CheckGiftCardBalance.GiftCardCouponCode.Invalid");

            return Ok(model);
        }

        #endregion

        #endregion
    }
}
