using System.Threading.Tasks;
using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.ManufacturerIntegration;

namespace Annique.Plugins.Nop.Customization.Factories
{
    public interface IManufacturerIntegrationModelFactory
    {
        /// <summary>
        /// Prepare Custom manufacturer Tab model
        /// </summary>
        /// <param name="manufacturerId">Manufacturer Indetifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Custom Manufacturer Tab model
        /// </returns>
        CustomManufacturerTabInfoModel PrepareCustomManufacturerTabModelInfoAsync(int manufacturerId);

        /// <summary>
        /// Prepare Manufacturer Integration search model
        /// </summary>
        /// <param name="searchModel">Manufacturer Integration search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Manufacturer Integration search model
        /// </returns>
        ManufacturerIntegrationSearchModel PrepareManufacturerIntegrationSearchModel(ManufacturerIntegrationSearchModel searchModel,int manufacturerId);

        /// <summary>
        /// Prepare Manufacturer Integration list model
        /// </summary>
        /// <param name="searchModel">Manufacturer Integration search model</param>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Manufacturer Integration list model
        /// </returns>
        Task<ManufacturerIntegrationListModel> PrepareManufacturerIntegrationListModelAsync(ManufacturerIntegrationSearchModel searchModel, int manufacturerId);


        /// <summary>
        /// Prepare Manufacturer Integration model
        /// </summary>
        /// <param name="model">Manufacturer Integration model</param>
        /// <param name="manufacturerIntegration">MAnufacturer Integration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Manufacturer Integration model
        /// </returns>
        ManufacturerIntegrationModel PrepareManufacturerIntegrationModel(ManufacturerIntegrationModel model, ManufacturerIntegration manufacturerIntegration);

        /// <summary>
        /// Prepare Manufacturer Integration table Fields
        /// </summary>
        /// <param name="model">Manufacturer Integration model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Manufacturer Integration table fields
        /// </returns>
        ManufacturerIntegration PrepareManufacturerIntegrationFields(ManufacturerIntegrationModel model);
    }
}
