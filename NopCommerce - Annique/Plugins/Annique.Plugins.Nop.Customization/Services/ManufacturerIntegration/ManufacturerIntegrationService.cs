using System.Collections.Generic;
using System.Threading.Tasks;
using Annique.Plugins.Nop.Customization.Domain;
using Nop.Data;
using System.Linq;

namespace Annique.Plugins.Nop.Customization.Services
{
    /// <summary>
    /// Manufacturer integration service
    /// </summary>
    public class ManufacturerIntegrationService : IManufacturerIntegrationService
    {
        #region Fields

        protected readonly IRepository<ManufacturerIntegration> _manufacturerIntegrationRepository;

        #endregion

        #region Ctor

        public ManufacturerIntegrationService(IRepository<ManufacturerIntegration> manufacturerIntegrationRepository)
        {
            _manufacturerIntegrationRepository = manufacturerIntegrationRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a manufacturer Integration
        /// </summary>
        /// <param name="manufacturerIntegration">Manufacturer Integration</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertManufacturerIntegrationAsync(ManufacturerIntegration manufacturerIntegration)
        {
            await _manufacturerIntegrationRepository.InsertAsync(manufacturerIntegration);
        }

        /// <summary>
        /// Deletes a Manufacturer Integration
        /// </summary>
        /// <param name="manufacturerIntegration">manufacturerIntegration</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteManufacturerIntegrationAsync(ManufacturerIntegration manufacturerIntegration)
        {
            await _manufacturerIntegrationRepository.DeleteAsync(manufacturerIntegration);
        }

        /// <summary>
        /// Gets a Manufacturer Integration 
        /// </summary>
        /// <param name="manufacturerIntegrationId">manufacturer Integration identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer Integration
        /// </returns>
        public async Task<ManufacturerIntegration> GetManufacturerIntegrationAsync(int manufacturerIntegrationId)
        {
            return await _manufacturerIntegrationRepository.GetByIdAsync(manufacturerIntegrationId);
        }

        /// <summary>
        /// Gets a manufcaturer integrations by manufcaturer identifier
        /// </summary>
        /// <param name="manufacturerId">The manufacturer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufcaturer Integrations
        /// </returns>
        public async Task<IList<ManufacturerIntegration>> GetAllManufacturerIntegrationByManufacturerIdAsync(int manufacturerId)
        {
            var query = from mi in _manufacturerIntegrationRepository.Table
                        where mi.ManufacturerId == manufacturerId
                        select mi;                       

            var manufacturerIntegrations = await query.ToListAsync();

            return manufacturerIntegrations;
        }

        #endregion
    }
}
