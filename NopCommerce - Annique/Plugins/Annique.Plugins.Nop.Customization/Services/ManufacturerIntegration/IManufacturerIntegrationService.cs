using System.Collections.Generic;
using System.Threading.Tasks;
using Annique.Plugins.Nop.Customization.Domain;

namespace Annique.Plugins.Nop.Customization.Services
{
    /// <summary>
    /// Manufacturer integration interface
    /// </summary>
    public interface IManufacturerIntegrationService
    {
        /// <summary>
        /// Inserts a manufacturer Integration
        /// </summary>
        /// <param name="manufacturerIntegration">Manufacturer Integration</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertManufacturerIntegrationAsync(ManufacturerIntegration manufacturerIntegration);

        /// <summary>
        /// Deletes a Manufacturer Integration
        /// </summary>
        /// <param name="manufacturerIntegration">manufacturerIntegration</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteManufacturerIntegrationAsync(ManufacturerIntegration manufacturerIntegration);

        /// <summary>
        /// Gets a Manufacturer Integration 
        /// </summary>
        /// <param name="manufacturerIntegrationId">manufacturer Integration identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer Integration
        /// </returns>
        Task<ManufacturerIntegration> GetManufacturerIntegrationAsync(int manufacturerIntegrationId);

        /// <summary>
        /// Gets a manufacturer integrations by manufacturer identifier
        /// </summary>
        /// <param name="manufacturerId">The manufacturer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer Integrations
        /// </returns>
        Task<IList<ManufacturerIntegration>> GetAllManufacturerIntegrationByManufacturerIdAsync(int manufacturerId);
    }
}
