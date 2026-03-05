using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.PickUpCollection;
using Annique.Plugins.Nop.Customization.Services.PickUpCollection;
using LinqToDB.Data;
using Nop.Core;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.PickUpCollection
{
    public class PickUpCollectionModelFactory : IPickUpCollectionModelFactory
    {
        #region Fields 

        private readonly IPickUpCollectionService _pickUpCollectionService;
        private readonly INopDataProvider _nopDataProvider;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;

        #endregion

        #region ctor

        public PickUpCollectionModelFactory(IPickUpCollectionService pickUpCollectionService,
            INopDataProvider nopDataProvider,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IAddressService addressService)
        {
            _pickUpCollectionService = pickUpCollectionService;
            _nopDataProvider = nopDataProvider;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _addressService = addressService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare PostNetStoreDeliveryModel with Filter PickUp Store
        /// </summary>
        /// <param name="PostNetStoreDeliveryModel">model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the filter pickup store Collections
        /// </returns>
        public async Task<PostNetStoreDeliveryModel> PrepareFilterPickUpStoreModelAsync(PostNetStoreDeliveryModel model)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //initialize latitude and longitude as floating 0
            var latitude = 0.0f;
            var longitude = 0.0f;

            //get current address from entered postal code
            var postalCodesResponseModel = await _pickUpCollectionService.GetPostalCodesAsync(model.Location);
            if (postalCodesResponseModel != null && postalCodesResponseModel.PostalCodes.Any())
            {
                //get first postal code address from Geo api postal response
                var firstPostalCodeAddress = postalCodesResponseModel.PostalCodes.FirstOrDefault();
                if (firstPostalCodeAddress != null)
                {
                    //take latitude and longitude from geo api postal response
                    latitude = firstPostalCodeAddress.Latitude;
                    longitude = firstPostalCodeAddress.Longitude;
                }
            }

            //Get filter pick up store from store pickup table using entered postal code's longitude and latutude
            var filterStores = await _nopDataProvider.QueryProcAsync<FilterStorePickupPoint>("sp_GetFilterPickUpStoresPC", new DataParameter { Name = "radius", Value = anniqueSettings.PickUpStoreRadius }, new DataParameter { Name = "longitude", Value = longitude }, new DataParameter { Name = "latitude", Value = latitude }, new DataParameter { Name = "PostCode", Value = model.Location });
            if (filterStores == null)
                return model;

            var customer = await _workContext.GetCurrentCustomerAsync();

            //If get filter pick up store then add in model.PickupPoints
            if (filterStores != null)
            {
                var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                model.PickupPoints = await filterStores.SelectAwait(async point =>
                {
                    //get Pickup store Address
                    var pointAddress = await _addressService.GetAddressByIdAsync(point.AddressId);

                    var pickupPointModel = new CustomCheckoutPickupPointModel
                    {
                        Id = point.Id,
                        Name = point.Name,
                        Description = point.Description,
                        Address = pointAddress.Address1,
                        City = pointAddress.City,
                        County = pointAddress.County,
                        ZipPostalCode = pointAddress.ZipPostalCode,
                        Latitude = point.Latitude,
                        Longitude = point.Longitude,
                        OpeningHours = point.OpeningHours,
                        Kms = point.Kms
                    };

                    return pickupPointModel;
                }).ToListAsync();
            }
            return model;
        }

        #endregion
    }
}
