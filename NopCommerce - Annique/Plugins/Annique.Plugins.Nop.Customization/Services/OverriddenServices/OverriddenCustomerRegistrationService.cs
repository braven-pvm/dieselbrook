using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    public class OverriddenCustomerRegistrationService : CustomerRegistrationService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public OverriddenCustomerRegistrationService(CustomerSettings customerSettings, 
            IActionContextAccessor actionContextAccessor,
            IAuthenticationService authenticationService,
            ICustomerActivityService customerActivityService, 
            ICustomerService customerService, 
            IEncryptionService encryptionService, 
            IEventPublisher eventPublisher, 
            IGenericAttributeService genericAttributeService, 
            ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService, 
            INotificationService notificationService, 
            IPermissionService permissionService, 
            IRewardPointService rewardPointService, 
            IShoppingCartService shoppingCartService, 
            IStoreContext storeContext, 
            IStoreService storeService, 
            IUrlHelperFactory urlHelperFactory, 
            IWorkContext workContext, 
            IWorkflowMessageService workflowMessageService, 
            RewardPointsSettings rewardPointsSettings,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            ISettingService settingService) : base(customerSettings,
                actionContextAccessor, 
                authenticationService,
                customerActivityService, 
                customerService, 
                encryptionService,
                eventPublisher, 
                genericAttributeService, 
                localizationService, 
                multiFactorAuthenticationPluginManager, 
                newsLetterSubscriptionService, 
                notificationService,
                permissionService, 
                rewardPointService,
                shoppingCartService, 
                storeContext, 
                storeService, 
                urlHelperFactory, 
                workContext, 
                workflowMessageService, 
                rewardPointsSettings)
        {
            _customerSettings = customerSettings;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _localizationService = localizationService;
            _rewardPointsSettings = rewardPointsSettings;
            _rewardPointService = rewardPointService;
            _storeContext = storeContext;
            _storeService = storeService;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _settingService = settingService;   
        }

        #endregion

        /// <summary>
        /// Register customer
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public override async Task<CustomerRegistrationResult> RegisterCustomerAsync(CustomerRegistrationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Customer == null)
                throw new ArgumentException("Can't load current customer");

            var store = await _storeContext.GetCurrentStoreAsync();

            //get current store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            if (settings != null && !settings.IsEnablePlugin)
                return await base.RegisterCustomerAsync(request);

            var result = new CustomerRegistrationResult();
            if (request.Customer.IsSearchEngineAccount())
            {
                result.AddError("Search engine can't be registered");
                return result;
            }

            if (request.Customer.IsBackgroundTaskAccount())
            {
                result.AddError("Background task account can't be registered");
                return result;
            }

            if (await _customerService.IsRegisteredAsync(request.Customer))
            {
                result.AddError("Current customer is already registered");
                return result;
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                result.AddError(await _localizationService.GetResourceAsync("Account.Register.Errors.EmailIsNotProvided"));
                return result;
            }

            if (!CommonHelper.IsValidEmail(request.Email))
            {
                result.AddError(await _localizationService.GetResourceAsync("Common.WrongEmail"));
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError(await _localizationService.GetResourceAsync("Account.Register.Errors.PasswordIsNotProvided"));
                return result;
            }

            #region Task 620 Phone number validation

            if (string.IsNullOrWhiteSpace(request.Customer.Phone))
            {
                result.AddError(await _localizationService.GetResourceAsync("Account.Register.Errors.PhoneNumberIsNotProvided"));
                return result;
            }

            if(await _userProfileAdditionalInfoService.IsPhoneNumberTakenAsync(request.Customer.Phone))
            { 
                result.AddError(await _localizationService.GetResourceAsync("Annique.Plugin.PhoneNumber.Validation.Message"));
                return result;
            }

            #endregion

            if (_customerSettings.UsernamesEnabled && string.IsNullOrEmpty(request.Username))
            {
                result.AddError(await _localizationService.GetResourceAsync("Account.Register.Errors.UsernameIsNotProvided"));
                return result;
            }

            //validate unique user
            if (await _customerService.GetCustomerByEmailAsync(request.Email) != null)
            {
                result.AddError(await _localizationService.GetResourceAsync("Account.Register.Errors.EmailAlreadyExists"));
                return result;
            }

            #region Task 600 email verification 

            var isEmailValid = await _userProfileAdditionalInfoService.VerifyEmailByApiAsync(request.Email);
            if (!isEmailValid)
            {
                result.AddError(await _localizationService.GetResourceAsync("Annique.Plugin.EmailableApi.Validation.Message"));
            }

            #endregion

            if (result.Errors.Any()) return result;

            if (_customerSettings.UsernamesEnabled && await _customerService.GetCustomerByUsernameAsync(request.Username) != null)
            {
                result.AddError(await _localizationService.GetResourceAsync("Account.Register.Errors.UsernameAlreadyExists"));
                return result;
            }

            //at this point request is valid
            request.Customer.Username = request.Username;
            request.Customer.Email = request.Email;

            var customerPassword = new CustomerPassword
            {
                CustomerId = request.Customer.Id,
                PasswordFormat = request.PasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };
            switch (request.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    customerPassword.Password = request.Password;
                    break;
                case PasswordFormat.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(request.Password);
                    break;
                case PasswordFormat.Hashed:
                    var saltKey = _encryptionService.CreateSaltKey(NopCustomerServicesDefaults.PasswordSaltKeySize);
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _customerSettings.HashedPasswordFormat);
                    break;
            }

            await _customerService.InsertCustomerPasswordAsync(customerPassword);

            request.Customer.Active = request.IsApproved;

            //add to 'Registered' role
            var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole == null)
                throw new NopException("'Registered' role could not be loaded");

            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = request.Customer.Id, CustomerRoleId = registeredRole.Id });

            //remove from 'Guests' role            
            if (await _customerService.IsGuestAsync(request.Customer))
            {
                var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
                await _customerService.RemoveCustomerRoleMappingAsync(request.Customer, guestRole);
            }

            //add reward points for customer registration (if enabled)
            if (_rewardPointsSettings.Enabled && _rewardPointsSettings.PointsForRegistration > 0)
            {
                var endDate = _rewardPointsSettings.RegistrationPointsValidity > 0
                    ? (DateTime?)DateTime.UtcNow.AddDays(_rewardPointsSettings.RegistrationPointsValidity.Value) : null;
                await _rewardPointService.AddRewardPointsHistoryEntryAsync(request.Customer, _rewardPointsSettings.PointsForRegistration,
                    request.StoreId, await _localizationService.GetResourceAsync("RewardPoints.Message.EarnedForRegistration"), endDate: endDate);
            }

            await _customerService.UpdateCustomerAsync(request.Customer);

            return result;
        }

    }
}
