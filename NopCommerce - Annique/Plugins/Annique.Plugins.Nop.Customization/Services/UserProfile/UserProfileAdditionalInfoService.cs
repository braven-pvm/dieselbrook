using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.OTP;
using Annique.Plugins.Nop.Customization.Models.UserProfile;
using Annique.Plugins.Nop.Customization.Services.ApiServices;
using Annique.Plugins.Nop.Customization.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.UserProfile
{
    public class UserProfileAdditionalInfoService : IUserProfileAdditionalInfoService
    {
        #region Fields

        private readonly IRepository<UserProfileAdditionalInfo> _UserProfileAdditionalInfoRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IRepository<CustomerChanges> _customerChangesRepository;
        private readonly IRepository<Lookups> _lookupsRepository;
        private readonly IApiService _apiService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ITokenizer _tokenizer;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public UserProfileAdditionalInfoService(IRepository<UserProfileAdditionalInfo> UserProfileAdditionalInfoRepository,
            IRepository<Customer> CustomerRepository,
            IRepository<Order> orderRepository,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IActionContextAccessor actionContextAccessor,
            IAuthenticationService authenticationService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            IUrlHelperFactory urlHelperFactory,
            IRepository<CustomerChanges> customerChangesRepository,
            IRepository<Lookups> lookupsRepository,
            IApiService apiService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILogger logger,
            IMessageTemplateService messageTemplateService,
            ITokenizer tokenizer,
            IGenericAttributeService genericAttributeService,
            IStoreService storeService,
            ICustomerService customerService,
            IOrderService orderService)
        {
            _UserProfileAdditionalInfoRepository = UserProfileAdditionalInfoRepository;
            _customerRepository = CustomerRepository;
            _orderRepository = orderRepository;
            _localizationService = localizationService;
            _workContext = workContext;
            _actionContextAccessor = actionContextAccessor;
            _authenticationService = authenticationService;
            _customerActivityService = customerActivityService;
            _eventPublisher = eventPublisher;
            _urlHelperFactory = urlHelperFactory;
            _customerChangesRepository = customerChangesRepository;
            _lookupsRepository = lookupsRepository;
            _apiService = apiService;
            _settingService = settingService;
            _storeContext = storeContext;
            _logger = logger;
            _storeContext = storeContext;
            _settingService = settingService;
            _messageTemplateService = messageTemplateService;
            _apiService = apiService;
            _genericAttributeService = genericAttributeService;
            _storeService = storeService;
            _customerService = customerService;
            _tokenizer = tokenizer;
            _orderService = orderService;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Generates an absolute URL for the specified store, routeName and route values
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="routeName">The name of the route that is used to generate URL</param>
        /// <param name="routeValues">An object that contains route values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the generated URL
        /// </returns>
        protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync()
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate the relative URL
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = urlHelper.RouteUrl(routeName, routeValues);

            //compose the result
            return new Uri(new Uri(store.Url), url).AbsoluteUri;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Insert user profile Info
        /// </summary>
        /// <param name="userProfileAdditionalInfo">User Profile info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertUserAdditionalInfoAsync(UserProfileAdditionalInfo userProfileAdditionalInfo)
        {
            await _UserProfileAdditionalInfoRepository.InsertAsync(userProfileAdditionalInfo);
        }

        /// <summary>
        /// Update user profile Info
        /// </summary>
        /// <param name="userProfileAdditionalInfo">User Profile info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateUserAdditionalInfoAsync(UserProfileAdditionalInfo userProfileAdditionalInfo)
        {
            await _UserProfileAdditionalInfoRepository.UpdateAsync(userProfileAdditionalInfo);
        }

        /// <summary>
        /// Get user profile info by customer id
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the User profile Info
        /// </returns>
        public async Task<UserProfileAdditionalInfo> GetUserProfileAdditionalInfoByCustomerIdAsync(int customerId)
        {
            return await _UserProfileAdditionalInfoRepository.Table
                .Where(i => i.CustomerId == customerId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Encrypt text in RC4
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <param name="encryptionPrivateKey">Encryption private key</param>
        /// <returns>Encrypted text</returns>
        public string EncryptTextRC4(string data, string encryptionPrivateKey)
        {
            //convert key into byte
            byte[] key = Encoding.UTF8.GetBytes(encryptionPrivateKey);

            //convert account number into byte
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            //Encode into rc4
            byte[] rc4_encrypted_data = RC4.Apply(byteData, key);

            //convert to base 64
            var encrypt_data_to_base64string = Convert.ToBase64String(rc4_encrypted_data);

            return encrypt_data_to_base64string;
        }

        /// <summary>
        /// Decrypt text in RC4
        /// </summary>
        /// <param name="cipherText">Text to decrypt</param>
        /// <param name="encryptionPrivateKey">Encryption private key</param>
        /// <returns>Decrypted text</returns>
        public string DecryptTextRC4(string data, string encryptionPrivateKey)
        {
            // byte data of the key
            byte[] key = Encoding.UTF8.GetBytes(encryptionPrivateKey);

            //Decode from base 64
            byte[] byteData = Convert.FromBase64String(data);

            //Decode From RC4
            var rc4_decrypted_data = RC4.Apply(byteData, key);

            //Decode byte to string
            var decrypted_data_from_base64string = Encoding.UTF8.GetString(rc4_decrypted_data);

            //return actual string 
            return decrypted_data_from_base64string;
        }

        /// <summary>
        /// Get additional info by IdNumber
        /// </summary>
        /// <param name="idNumber">IdNumber</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the User profile Addiitonal Info
        /// </returns>
        public async Task<UserProfileAdditionalInfo> GetUserProfileAdditionalInfoByIdNumberAsync(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
                return null;

            var query = from u in _UserProfileAdditionalInfoRepository.Table
                        orderby u.Id
                        where u.IdNumber == idNumber
                        select u;

            var userProfileAdditionalInfo = await query.FirstOrDefaultAsync();

            return userProfileAdditionalInfo;
        }

        /// <summary>
        /// Validate User Profile IdNumber
        /// </summary>
        /// <param name="userProfileAdditionalInfo">UserProfileAdditionalInfo</param>
        /// <param name="newIdNumber">New IdNumber</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ValidateUserIdNumberAsync(UserProfileAdditionalInfo userProfileAdditionalInfo, string newIdNumber)
        {
            newIdNumber = newIdNumber.Trim();

            var user2 = await GetUserProfileAdditionalInfoByIdNumberAsync(newIdNumber);
            if (user2 != null && userProfileAdditionalInfo.Id != user2.Id)
                throw new NopException(await _localizationService.GetResourceAsync("Account.IdNumberErrors.IdNumberAlreadyExists"));
        }

        /// <summary>
        /// Login passed user
        /// </summary>
        /// <param name="customer">User to login</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <param name="isPersist">Is remember me</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result of an authentication
        /// </returns>
        public virtual async Task<IActionResult> SignInCustomerAsync(Customer customer, string returnUrl, bool isPersist = false)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (currentCustomer?.Id != customer.Id)
            {
                await _workContext.SetCurrentCustomerAsync(customer);
            }

            //sign in new customer
            await _authenticationService.SignInAsync(customer, isPersist);

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

            //activity log
            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            //redirect to the return URL if it's specified
            if (!string.IsNullOrEmpty(returnUrl) && urlHelper.IsLocalUrl(returnUrl))
                return new RedirectResult(returnUrl);

            return new RedirectToRouteResult("Homepage", null);
        }
        #endregion

        #region Customer Changes method

        /// <summary>
        /// Insert customer changes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertCustomerChangesAsync(Customer customer, string tableName, int changeId, string fieldName, string oldValue, string newValue)
        {
            var customerChanges = new CustomerChanges
            {
                ChangeId = changeId,
                cTableName = tableName,
                CustomerId = customer.Id,
                cCustno = customer.Username,
                cFieldname = fieldName,
                cOldvalue = oldValue,
                cNewvalue = newValue,
                InsUpdDate = DateTime.UtcNow
            };
            await _customerChangesRepository.InsertAsync(customerChanges);
        }

        #endregion

        #region Lookups Method

        /// <summary>
        /// Gets Lookups by ctype
        /// </summary>
        /// <param name="ctype">ctype</param>
        /// <param name="storeId">customer registered storeId</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Lookups
        /// </returns>
        public async Task<IList<Lookups>> GetLookupsByCtypeAsync(string ctype, int storeId)
        {
            //trim access spaces 
            ctype = ctype.Trim();
            var query = _lookupsRepository.Table;

            //get look up by ctype and is active
            query = query.Where(q => q.ctype.Equals(ctype) && q.Iactive);

            //if store id is not 0 then get lookups which matches with customer's registed store id
            if (query.Any(q => q.StoreId != 0))
            {
                query = query.Where(q => q.StoreId == storeId);
                return await query.ToListAsync();
            }
            return await query.ToListAsync();
        }

        public async Task<IList<SelectListItem>> GetSelectListAsync(string ctype, int storeId)
        {
            ctype = ctype.Trim();
            var query = _lookupsRepository.Table
                .Where(q => q.ctype == ctype && q.Iactive);

            // Check if any lookups are store-specific (StoreId != 0)
            bool hasStoreSpecific = await query.AnyAsync(q => q.StoreId != 0);

            if (hasStoreSpecific)
            {
                query = query.Where(q => q.StoreId == storeId);
            }

            var list = await query
                .Select(x => new SelectListItem
                {
                    Text = x.description,
                    Value = x.code
                })
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// validate user profile 
        /// </summary>
        /// <param name="customer">customer</param>
        /// <param name="customerRoleIds">customer role ids</param>
        /// <param name="model">SHopping cart model</param>
        /// <param name="settings">Annique customization settings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task validates user profiles and email validation
        /// </returns>
        public async Task<string> ValidateUserProfileAsync(Customer customer, int[] customerRoleIds, ShoppingCartModel model, AnniqueCustomizationSettings settings)
        {
            if (await _customerService.IsGuestAsync(customer))
                return string.Empty;

            // If the customer's email is null or empty, hide the checkout button and set the validation message
            if (string.IsNullOrEmpty(customer.Email))
            {
                model.HideCheckoutButton = true;
                return await _localizationService.GetResourceAsync("Account.UserProfileAdditionalInfo.ProfileValidation.Message");
            }

            if (customerRoleIds.Contains(settings.ConsultantRoleId))
            {
                var userProfileInfo = await GetUserProfileAdditionalInfoByCustomerIdAsync(customer.Id);
                if (userProfileInfo == null || !userProfileInfo.ProfileUpdated)
                {
                    model.HideCheckoutButton = true;
                    return await _localizationService.GetResourceAsync("Account.UserProfileAdditionalInfo.ProfileValidation.Message");
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// validate email by emailable API
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task validates email by emailable api returs true if email is deliverable else return false
        /// </returns>
        public async Task<bool> VerifyEmailByApiAsync(string email)
        {
            //load settings
            var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(_storeContext.GetCurrentStoreAsync().Id);

            #region Task 627 added enable disable flag for email verification

            if (!anniqueSettings.IsEmailVerification)
                return true;

            if (string.IsNullOrEmpty(anniqueSettings.EmailableApi) || string.IsNullOrEmpty(anniqueSettings.EmailableApiKey))
            {
                await _logger.InformationAsync("Emailable API settings are missing.");
                return true;
            }

            #endregion

            //Get emailable api Url
            var hostUrl = new Uri(anniqueSettings.EmailableApi);
            string relativePath = $"?email={email}&api_key={anniqueSettings.EmailableApiKey}";

            string url = hostUrl + relativePath;

            var response = await _apiService.GetAPIResponseAsync(url);
            if (response.StatusCode == 249)
            {
                await _logger.WarningAsync("Email verification request is taking too long or failed (status 249).");
                return false;
            }

            var result = JsonConvert.DeserializeObject<EmailVerificationResponseModel>(response.Content, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            // Handle email verification status
            if (string.IsNullOrEmpty(result.State))
            {
                return false;
            }
            // Reject if email is undeliverable or unknown
            else if (result.State == "undeliverable" || result.State == "unknown")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Password recovery SMS methods

        /// <summary>
        ///Return password recovery by sms is enable or disable
        /// </summary>
        public async Task<bool> IsPasswordResetViaSmsEnabledAsync()
        {
            var storeScope = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

            if (settings.IsEnablePlugin && settings.IsPasswordResetEnabled)
                return true;

            return false;
        }

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains password recovery sms sent or not
        /// </returns>
        public virtual async Task<bool> SendCustomerPasswordRecoverySmsAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //save token and current date
            var passwordRecoveryToken = Guid.NewGuid();
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                passwordRecoveryToken.ToString());
            DateTime? generatedDateTime = DateTime.UtcNow;
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

            var store = await _storeContext.GetCurrentStoreAsync();

            //load settings
            var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(anniqueSettings.PasswordResetSmsMessageTemplateId);
            if (messageTemplate == null)
                return false;

            //tokens
            var commonTokens = new List<Token>();
            await AddCustomTokensAsync(commonTokens, customer, store);

            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

            //Replace body tokens 
            var bodyReplaced = _tokenizer.Replace(body, commonTokens, true);

            // send the SMS notification
            return await SendSmsNotificationAsync(anniqueSettings, customer.Id, bodyReplaced);
        }

        /// <summary>
        /// Sends sms notification
        /// </summary>
        /// <param name="settings">Annique customization setting</param>
        /// <param name="customerId">Customer Id</param>
        /// <param name="message">Message</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result returns true if sms sent else return false
        /// </returns>
        public virtual async Task<bool> SendSmsNotificationAsync(AnniqueCustomizationSettings settings, int customerId, string message)
        {
            if (settings == null || string.IsNullOrEmpty(settings.PasswordResetApi))
                throw new ArgumentNullException(nameof(settings.PasswordResetApi));

            // dynamic object for the payload
            dynamic payload = new ExpandoObject();
            payload.customerid = customerId;
            payload.message = message;

            // Conditionally adding the 'staging' property if env is stage 
            if (settings.IsStagingModeForPasswordReset)
            {
                payload.staging = true;
            }

            // API service method to send the POST request
            ApiResponse apiResponse = await _apiService.PostAPIMethodAsync(settings.PasswordResetApi, payload, settings.PasswordResetApiKey);

            if (apiResponse.StatusCode == 200)
            {
                return true;
            }
            else
            {
                // If status code is other than 200 returning false
                return false;
            }
        }

        /// <summary>
        /// Add custom tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customer">Customer</param>
        /// <param name="store">STore</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task AddCustomTokensAsync(IList<Token> tokens, Customer customer, Store store)
        {
            tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));

            var passwordRecoveryUrl = await RouteUrlAsync(routeName: "PasswordRecoveryConfirm", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute), guid = customer.CustomerGuid });
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
        }

        #endregion

        #region Default Country

        /// <summary>
        /// Set customer default country
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SetDefaultCountryAsync(Customer customer)
        {
            var storeScope = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);
            if (settings.IsDefaultCountryIdEnabled && customer.CountryId == 0)
                customer.CountryId = settings.DefaultCountryId;
        }

        #endregion

        #region Activation date

        /// <summary>
        /// Set customer activation date on first order
        /// </summary>
        /// <param name="order">order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateActivationDateOnFirstOrderAsync(Order order)
        {
            //get user profile additional info
            var userProfileInfo = await GetUserProfileAdditionalInfoByCustomerIdAsync(order.CustomerId);

            //activation date also null then only update activation date from null to order createdUTC
            if (userProfileInfo.ActivationDate.HasValue == false)
            { 
                //get first order of user
                var firstOrder = _orderRepository.Table.Where(o => o.CustomerId == order.CustomerId && o.PaymentStatusId == (int)PaymentStatus.Paid  && (o.OrderStatusId == (int)OrderStatus.Processing || o.OrderStatusId == (int)OrderStatus.Complete)).FirstOrDefault();
                if (firstOrder != null)
                {
                    //if current order is first order of user 
                    if (firstOrder.Id == order.Id)
                    {
                        var previousActivationDate = userProfileInfo.ActivationDate;
                        userProfileInfo.ActivationDate = order.CreatedOnUtc;
                        await UpdateUserAdditionalInfoAsync(userProfileInfo);

                        // Add a detailed order note
                        var note = new OrderNote
                        {
                            OrderId = order.Id,
                            Note = $"ActivationDate update triggered.\n" +
                                   $"Previous ActivationDate: {(previousActivationDate.HasValue ? previousActivationDate.Value.ToString("u") : "null")}\n" +
                                   $"First Paid Order ID: {firstOrder.Id}\n" +
                                   $"New ActivationDate: {userProfileInfo.ActivationDate:u}",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        };

                        await _orderService.InsertOrderNoteAsync(note);
                    }
                }
            }
        }

        #endregion

        public async Task<bool> IsPhoneNumberTakenAsync(string phoneNumber)
        {
            // Step 1: Check if the phone number exists in the Customer table asynchronously
            bool isTakenInCustomerTable = await _customerRepository.Table
                .AnyAsync(c => c.Phone == phoneNumber);

            // Step 2: If not found, check the UserAdditionalInfo table for WhatsappNumber asynchronously
            if (!isTakenInCustomerTable)
            {
                bool isTakenInUserAdditionalInfoTable = await _UserProfileAdditionalInfoRepository.Table
                    .AnyAsync(u => u.WhatsappNumber == phoneNumber);

                return isTakenInUserAdditionalInfoTable;
            }

            return true; // Phone number found in the Customer table
        }


        public async Task<bool> IsPhoneOrWhatsappNumberTakenByOtherAsync(string phoneNumber, int currentCustomerId)
        {
            bool isTakenInCustomerTable = await _customerRepository.Table
                                         .AnyAsync(c => c.Phone == phoneNumber && c.Id != currentCustomerId);

            if (isTakenInCustomerTable)
                return true;

            bool isTakenInUserAdditionalInfoTable = await _UserProfileAdditionalInfoRepository.Table
                .AnyAsync(u => u.WhatsappNumber == phoneNumber && u.CustomerId != currentCustomerId);

            return isTakenInUserAdditionalInfoTable;
        }

        public async Task ValidatePhoneNumberAsync(string phoneNumber, int customerId, string errorResourceKey)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return;

            var isTaken = await IsPhoneOrWhatsappNumberTakenByOtherAsync(phoneNumber?.Trim(), customerId);
            if (isTaken)
                throw new NopException(await _localizationService.GetResourceAsync(errorResourceKey));
        }

    }
}
