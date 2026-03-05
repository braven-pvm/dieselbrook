using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.UserProfile;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.UserProfile
{
    public class UserProfileAdditionalInfoModelFactory : IUserProfileAdditionalInfoModelFactory
    {
        #region Fields

        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public UserProfileAdditionalInfoModelFactory(IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            ICustomerService customerService)
        {
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _customerService = customerService;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Prepare lookups list
        /// </summary>
        /// <param name="items">Lookups item list</param>
        /// <param name="LookupType">Lookups Type</param>
        /// <param name="storeId">Customer registered store Id</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task<IList<SelectListItem>> PrepareLookupsListAsync(IList<SelectListItem> items, string lookupType,int storeId)
        {
            foreach (var lookup in await _userProfileAdditionalInfoService.GetLookupsByCtypeAsync(lookupType,storeId))
            {
                items.Add(new SelectListItem { Value = lookup.code, Text = lookup.description });
            }

            return items;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare UserProfileAdditionalInfoModel
        /// </summary>
        /// <param name="customerId">Customer Indetifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the UserProfileAdditionalInfoModel
        /// </returns>
        public async Task<UserProfileAdditionalInfoModel> PrepareUserProfileAdditionalInfoModelAsync(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            UserProfileAdditionalInfoModel model = new();
            model.CustomerId = customer.Id;

            //Titles
            model.AvailableTitles = new List<SelectListItem>();
            //insert this default item at first
            model.AvailableTitles.Insert(0, new SelectListItem { Text = "Select Title", Value = string.Empty });
            //prepare title lookups
            model.AvailableTitles = await PrepareLookupsListAsync(model.AvailableTitles, AnniqueCustomizationDefaults.TitleLookup,customer.RegisteredInStoreId);

            //Nationality
            model.AvaialableNationality = new List<SelectListItem>();
            //insert this default item at first
            model.AvaialableNationality.Insert(0, new SelectListItem { Text = "Select Nationality", Value = string.Empty });
            //prepare nationality lookups
            model.AvaialableNationality = await PrepareLookupsListAsync(model.AvaialableNationality, AnniqueCustomizationDefaults.NationalityLookup,customer.RegisteredInStoreId);

            //Language
            model.AvailableLanguages = new List<SelectListItem>();
            //insert this default item at first
            model.AvailableLanguages.Insert(0, new SelectListItem { Text = "Select Language", Value = string.Empty });
            //prepare language lookups
            model.AvailableLanguages = await PrepareLookupsListAsync(model.AvailableLanguages, AnniqueCustomizationDefaults.LanguageLookup, customer.RegisteredInStoreId);

            //Ethnicity
            model.AvailableEthnicity = new List<SelectListItem>();
            //insert this default item at first
            model.AvailableEthnicity.Insert(0, new SelectListItem { Text = "Select Ethnicity", Value = string.Empty });
            //prepare ethnicity lookups
            model.AvailableEthnicity = await PrepareLookupsListAsync(model.AvailableEthnicity, AnniqueCustomizationDefaults.EthnicityLookup, customer.RegisteredInStoreId);

            //Bank names
            model.AvailableBankNames = new List<SelectListItem>();
            //insert this default item at first
            model.AvailableBankNames.Insert(0, new SelectListItem { Text = "Select Bank", Value = string.Empty });
            //prepare bank lookups
            model.AvailableBankNames = await PrepareLookupsListAsync(model.AvailableBankNames, AnniqueCustomizationDefaults.BankLookup, customer.RegisteredInStoreId);

            //Account type
            model.AvailableAccountTypes = new List<SelectListItem>();
            //insert this default item at first
            model.AvailableAccountTypes.Insert(0, new SelectListItem { Text = "Select Account type", Value = string.Empty });
            //prepare Account type lookups
            model.AvailableAccountTypes = await PrepareLookupsListAsync(model.AvailableAccountTypes, AnniqueCustomizationDefaults.AccountType, customer.RegisteredInStoreId);

            var userProfileInfo = await _userProfileAdditionalInfoService.GetUserProfileAdditionalInfoByCustomerIdAsync(customerId);
            if (userProfileInfo != null)
            {
                model.Id = userProfileInfo.Id;
                model.Title = userProfileInfo.Title;
                model.Nationality = userProfileInfo.Nationality;
                model.IdNumber = userProfileInfo.IdNumber;
                model.Language = userProfileInfo.Language;
                model.Ethnicity = userProfileInfo.Ethnicity;
                model.BankName = userProfileInfo.BankName;
                model.AccountHolder = userProfileInfo.AccountHolder;
                model.AccountType = userProfileInfo.AccountType;
                model.Accept = userProfileInfo.Accept;
                model.WhatsappNumber = userProfileInfo.WhatsappNumber;
                model.ProfileUpdated = userProfileInfo.ProfileUpdated;
                model.BrevoID = userProfileInfo.BrevoID;

                if (userProfileInfo.ActivationDate.HasValue)
                    model.ActivationDate = userProfileInfo.ActivationDate.Value.ToShortDateString();

                if (!string.IsNullOrEmpty(userProfileInfo.AccountNumber))
                    model.AccountNumber = _userProfileAdditionalInfoService.DecryptTextRC4(userProfileInfo.AccountNumber, AnniqueCustomizationDefaults.RC4EncryptionKey);
            }
            return model;
        }

        /// <summary>
        /// Prepare UserProfileAdditionalInfo fields
        /// </summary>
        /// <param name="model">UserProfileAdditionalInfo Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the UserProfileAdditionalInfo
        /// </returns>
        public UserProfileAdditionalInfo PrepareUserProfileAdditionalInfoFields(UserProfileAdditionalInfoModel model)
        {
            if (model != null)
            {
                var userProfileAdditionalInfo = new UserProfileAdditionalInfo()
                {
                    Id = model.Id,
                    CustomerId = model.CustomerId,
                    Title = model.Title,
                    Nationality = model.Nationality,
                    IdNumber = model.IdNumber,
                    Language = model.Language,
                    Ethnicity = model.Ethnicity,
                    BankName = model.BankName,
                    AccountHolder = model.AccountHolder,
                    AccountType = model.AccountType,
                    Accept = model.Accept,
                    WhatsappNumber = model.WhatsappNumber,
                    ProfileUpdated = model.ProfileUpdated,
                    BrevoID = model.BrevoID,
                };

                if(!string.IsNullOrEmpty(model.ActivationDate))
                    userProfileAdditionalInfo.ActivationDate = Convert.ToDateTime(model.ActivationDate);

                if (!string.IsNullOrEmpty(model.AccountNumber))
                {
                    userProfileAdditionalInfo.AccountNumber = _userProfileAdditionalInfoService.EncryptTextRC4(model.AccountNumber, AnniqueCustomizationDefaults.RC4EncryptionKey);
                }
                return userProfileAdditionalInfo;
            }
            return null;
        }

        /// <summary>
        /// Prepare Old address copy 
        /// </summary>
        /// <param name="oldAddress">Old address</param>
        /// <returns>
        /// The task result contains the old Address copy
        /// </returns>
        public Address PrepareOldAddressCopy(Address oldAddress)
        {
            return new Address
            {
                Id = oldAddress.Id,
                FirstName = oldAddress.FirstName,
                LastName = oldAddress.LastName,
                Email = oldAddress.Email,
                Company = oldAddress.Company,
                CountryId = oldAddress.CountryId,
                StateProvinceId = oldAddress.StateProvinceId,
                City = oldAddress.City,
                Address1 = oldAddress.Address1,
                Address2 = oldAddress.Address2,
                ZipPostalCode = oldAddress.ZipPostalCode,
                PhoneNumber = oldAddress.PhoneNumber,
                FaxNumber = oldAddress.FaxNumber,
                County = oldAddress.County
            };
        }

        #endregion
    }
}
