using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.PickUpCollection;
using Annique.Plugins.Nop.Customization.Services.ApiServices;
using LinqToDB.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.PickUpCollection
{
    public class PickUpCollectionService : IPickUpCollectionService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IApiService _apiService;
        private readonly INopDataProvider _nopDataProvider;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PickUpCollectionService(ISettingService settingService,
            IStoreContext storeContext,
            IApiService apiService,
            INopDataProvider nopDataProvider,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            ICustomerService customerService,
            IWorkContext workContext)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _apiService = apiService;
            _nopDataProvider = nopDataProvider;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _customerService = customerService;
            _workContext = workContext;
        }

        #endregion

        #region Method

        /// <summary>
        /// Get Postal Codes 
        /// </summary>
        /// <param name="postalCode">postalCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains postal codes from geo location api
        /// </returns>
        public async Task<PostalCodesResponseModel> GetPostalCodesAsync(string postalCode)
        {
            //load settings
            var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(_storeContext.GetCurrentStoreAsync().Id);

            if(anniqueSettings.PickUpStoreRadius == 0)
                return null;

            if(anniqueSettings.GeoLocationApiUsername == null)
                return null;

            //Get geo location Url
            var hostUrl = new Uri(AnniqueCustomizationDefaults.GeoLocationUrl);
            string relativePath = "?country=ZA&username=" + anniqueSettings.GeoLocationApiUsername;

            if (postalCode.Any(char.IsDigit))
            { 
                relativePath += "&postalcode=" + postalCode; 
            }
            else { 
                relativePath += "&placename=" + postalCode; 
            }

            string url = hostUrl + relativePath;

            var response = await _apiService.GetAPIResponse(url);
            if (response == null)
                return null;

            return JsonConvert.DeserializeObject<PostalCodesResponseModel>(response, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        /// <summary>
        /// Get pickup store
        /// </summary>
        /// <param name="pickUpStoreId">Pick up store id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result returns filtered store pickup point
        /// </returns>
        public async Task<FilterStorePickupPoint> GetPickUpStoreByIdAsync(int pickUpStoreId)
        {
            return (await _nopDataProvider.QueryProcAsync<FilterStorePickupPoint>("sp_GetPickUpStoreById", new DataParameter { Name = "id", Value = pickUpStoreId })).FirstOrDefault();
        }

        /// <summary>
        /// Update address form data
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="cell">cell number</param>
        /// <returns>
        /// The task update address with form data
        /// </returns>
        public void UpdateAddressWithFormData(Address address, string firstName, string lastName, string cell)
        {
            address.FirstName = firstName;
            address.LastName = lastName;
            address.PhoneNumber = cell;
        }

        /// <summary>
        /// Get custom attribute for PEP address
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="pickUpStoreId">Pick up store id</param>
        /// <returns>
        /// The task returns custom attributes for PEP address
        /// </returns>
        public async Task<string> GetCustomAttributesAsync(Address address, int pickUpStoreId)
        {
            var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(_storeContext.GetCurrentStoreAsync().Id);
            var pickUpAttribute = await _addressAttributeService.GetAddressAttributeByIdAsync(anniqueSettings.PickupCustomAttributeId);
            return _addressAttributeParser.AddAddressAttribute(string.Empty, pickUpAttribute, pickUpStoreId.ToString());
        }

        /// <summary>
        /// Prepare address fields
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="customAttributes">Custom attributes</param>
        /// <param name="customerEmail">Customer email</param>
        /// <param name="countryName">Country name</param>
        /// <returns>
        /// The task returns address
        public Address PrepareAddressFields(Address address, string customAttributes, string customerEmail, string countryName)
        {
            var newAddress =  new Address
            {
                FirstName = address.FirstName,
                LastName = address.LastName,
                Email = customerEmail,
                Company = address.Company,
                CountryId = address.CountryId,
                StateProvinceId = address.StateProvinceId,
                City = address.City,
                County = countryName,
                Address1 = address.Address1,
                Address2 = address.Address2,
                ZipPostalCode = address.ZipPostalCode,
                PhoneNumber = address.PhoneNumber,
                FaxNumber = address.FaxNumber,

                CustomAttributes = customAttributes,
                CreatedOnUtc = DateTime.UtcNow
            };

            //some validation
            if (newAddress.CountryId == 0)
                newAddress.CountryId = null;
            if (newAddress.StateProvinceId == 0)
                newAddress.StateProvinceId = null;

            return newAddress;
        }

        #region Task 622 Option to enable Pickup for only certain Roles
       
        /// <summary>
        /// customer allowed for pick up store
        /// </summary>
        /// <returns>
        /// The task returns true or false based on customer roles
        public async Task<bool> IsCustomerAllowedForPickupAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            // Get the store settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            // Parse customer role IDs from settings
            var customerIdsAllowedForPickup = (settings.CustomerRoleIdsForPickup?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id =>
                {
                    if (int.TryParse(id.Trim(), out var parsedId))
                        return (int?)parsedId;
                    return null;
                })
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList()) ?? new List<int>();

            // Get current customer role IDs
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            // Check if any role ID matches
            return customerRoleIds.Intersect(customerIdsAllowedForPickup).Any();
        }

        #endregion

        #endregion
    }
}
