using System;
using System.Linq;
using System.Threading.Tasks;
using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.ManufacturerIntegration;
using Annique.Plugins.Nop.Customization.Services;
using Nop.Web.Framework.Models.Extensions;

namespace Annique.Plugins.Nop.Customization.Factories
{
    public class ManufacturerIntegrationModelFactory : IManufacturerIntegrationModelFactory
    {
        #region Fields

        private readonly IManufacturerIntegrationService _manufacturerIntegrationService;

        #endregion

        #region Ctor

        public ManufacturerIntegrationModelFactory(IManufacturerIntegrationService manufacturerIntegrationService)
        {
            _manufacturerIntegrationService = manufacturerIntegrationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare Custom manufacturer Tab model
        /// </summary>
        /// <param name="manufacturerId">Manufacturer Indetifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Custom Manufacturer Tab model
        /// </returns>
        public CustomManufacturerTabInfoModel PrepareCustomManufacturerTabModelInfoAsync(int manufacturerId)
        {
            var model = new CustomManufacturerTabInfoModel();

            model.ManufacturerIntegrationModel.ManufacturerId = manufacturerId;
            PrepareManufacturerIntegrationSearchModel(model.ManufacturerIntegrationSearchModel, manufacturerId);

            return model;
        }

        /// <summary>
        /// Prepare Manufacturer Integration search model
        /// </summary>
        /// <param name="searchModel">Manufacturer Integration search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Manufacturer Integration search model
        /// </returns>
        public virtual ManufacturerIntegrationSearchModel PrepareManufacturerIntegrationSearchModel(ManufacturerIntegrationSearchModel searchModel,
            int manufacturerId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.ManufacturerId = manufacturerId;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare Manufacturer Integration list model
        /// </summary>
        /// <param name="searchModel">Manufacturer Integration search model</param>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Manufacturer Integration list model
        /// </returns>
        public virtual async Task<ManufacturerIntegrationListModel> PrepareManufacturerIntegrationListModelAsync(ManufacturerIntegrationSearchModel searchModel, int manufacturerId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var manufacturerIntegrations = (await _manufacturerIntegrationService.GetAllManufacturerIntegrationByManufacturerIdAsync(manufacturerId)).ToPagedList(searchModel);;

            //prepare grid model
            var model = new ManufacturerIntegrationListModel().PrepareToGrid(searchModel, manufacturerIntegrations, () =>
            {
                return manufacturerIntegrations.Select(manufacturerIntegration =>
                {
                    var manufacturerIntegrationModel = new ManufacturerIntegrationModel
                    {
                        Id = manufacturerIntegration.Id,
                        ManufacturerId = manufacturerId,
                        IntegrationField = manufacturerIntegration.IntegrationField,
                        IntegrationValue = manufacturerIntegration.IntegrationValue
                    };
                    return manufacturerIntegrationModel;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare Manufacturer Integration model
        /// </summary>
        /// <param name="model">Manufacturer Integration model</param>
        /// <param name="manufacturerIntegration">MAnufacturer Integration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Manufacturer Integration model
        /// </returns>
        public virtual ManufacturerIntegrationModel PrepareManufacturerIntegrationModel(ManufacturerIntegrationModel model, ManufacturerIntegration manufacturerIntegration)
        {
            if (manufacturerIntegration != null)
            {
                model ??= new ManufacturerIntegrationModel();
                model.Id = manufacturerIntegration.Id;
                model.ManufacturerId = manufacturerIntegration.ManufacturerId;
                model.IntegrationField = manufacturerIntegration.IntegrationField;
                model.IntegrationValue = manufacturerIntegration.IntegrationValue;
            }
            return model;
        }

        /// <summary>
        /// Prepare Manufacturer Integration table Fields
        /// </summary>
        /// <param name="model">Manufacturer Integration model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Manufacturer Integration table fields
        /// </returns>
        public ManufacturerIntegration PrepareManufacturerIntegrationFields(ManufacturerIntegrationModel model)
        {
            if (model != null)
            {
                var manufacturerIntegration = new ManufacturerIntegration()
                {
                    Id = model.Id,
                    ManufacturerId = model.ManufacturerId,
                    IntegrationField = model.IntegrationField,
                    IntegrationValue = model.IntegrationValue
                };
                return manufacturerIntegration;
            }
            return null;
        }

        #endregion 
    }
}
