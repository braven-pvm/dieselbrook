using Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public;
using Nop.Core;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ConsultantRegistrations
{
    public interface IConsultantNewRegistrationService
    {
        /// <summary>
        /// Gets page setting
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of page settings
        /// </returns>
        Task<RegistrationPageSettings> GetPageSettings();

        /// <summary>
        /// Insert page setting
        /// </summary>
        /// <param name="registrationPageSettings">Page Setting</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task creates page setting for consultant register page
        /// </returns>
        Task InsertPageSettings(RegistrationPageSettings registrationPageSettings);

        /// <summary>
        /// Update page setting
        /// </summary>
        /// <param name="registrationPageSettings">Page Setting</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task updates page setting for consultant register page
        /// </returns>
        Task UpdatePageSettings(RegistrationPageSettings registrationPageSettings);

        /// <summary>
        /// Gets all registration list
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the new consultant registration list
        /// </returns>
        Task<IPagedList<NewRegistrations>> GetAllRegistrationsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get registration by id
        /// </summary>
        /// <param name="id">Resgistration id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task constains registration data get by id
        /// </returns>
        Task<NewRegistrations> GetRegistrationById(int id);

        /// <summary>
        /// Insert new consultant registration
        /// </summary>
        /// <param name="newRegistrations">Resgistration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task creates new consultant registration
        /// </returns>
        Task InsertAsync(NewRegistrations newRegistrations);

        /// <summary>
        /// update consultant registration
        /// </summary>
        /// <param name="newRegistrations">Resgistration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task updates consultant registration
        /// </returns>
        Task UpdateAsyc(NewRegistrations  newRegistrations);

        /// <summary>
        /// validate consultant registration data
        /// </summary>
        /// <param name="id">Resgistration id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task validates data using thrid party api
        /// </returns>
        Task<ValidationResponse> ValidateConsultantAsync(int id);

        /// <summary>
        ///Return Nop based consultant registration is enable or disable
        /// </summary>
        Task<bool> IsNopBasedConsultantRegistrationEnabledAsync();

        /// <summary>
        /// Prepare new registrations
        /// </summary>
        /// <param name="model">Resgistration model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task prepares entity from model
        /// </returns>
        Task<NewRegistrations> PrepareNewRegistrationsFromModel(ConsultantRegistrationModel model);

        /// <summary>
        /// Gets all file urls 
        /// </summary>
        /// <returns>
        /// The task result contains the file urls and updated report configuration block
        /// </returns>
        string[] ExtractFileUrlsFromFirstLine(string reportBlock, string fileType, out string updatedBlock);
    }
}
