using Annique.Plugins.Nop.Customization.Models.ShippingAddressValidation;
using Annique.Plugins.Nop.Customization.Services.ApiServices;
using LinqToDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ShippingAddressValidation
{
    public class ShippingAddressValidationService : IShippingAddressValidationService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IApiService _apiService;

        #endregion

        #region Ctor

        public ShippingAddressValidationService(ISettingService settingService,
            IStoreContext storeContext,
            IStateProvinceService stateProvinceService,
            IApiService apiService)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _stateProvinceService = stateProvinceService;
            _apiService = apiService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Get Subrub Combinations
        /// </summary>
        /// <param name="term">term</param>
        /// <param name="stateId">stateId</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains subrub combinations codes from  api
        /// </returns>      
        public async Task<List<SubrubResponseModel>> GetSubrubCombinationsAsync(string term, int stateId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //load settings
            var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            if (anniqueSettings.ShippingAddressValidationApi == null)
                return null;

            //Get shipping address validation Url
            var hostUrl = new Uri(anniqueSettings.ShippingAddressValidationApi);

            //get state name by stateId
            var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(stateId))?.Name;

            if (stateProvinceName == "other")
                return null;

            string relativePath = "?query=" + term + "&Province=" + stateProvinceName;
            string url = hostUrl + relativePath;

            var response = await _apiService.GetAPIResponse(url);
            if (response == null)
                return new List<SubrubResponseModel>(); // Return an empty list;

            return JsonConvert.DeserializeObject<List<SubrubResponseModel>>(response, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        #endregion
    }
}
