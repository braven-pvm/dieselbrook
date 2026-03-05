using Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Admin;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.ConsultantRegistrations
{
    public interface IConsultantRegistrationModelFactory
    {
        #region Admin methods

        /// <summary>
        /// Prepare registration search model
        /// </summary>
        /// <param name="searchModel">ConsultantRegistration search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ConsultantRegistration search model
        /// </returns>
        Task<ConsultantRegistrationSearchModel> PrepareConsultantRegistrationSearchModelAsync(ConsultantRegistrationSearchModel searchModel);

        /// <summary>
        /// Prepare paged Consultant Registration list model
        /// </summary>
        /// <param name="searchModel">Consultant Registration search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Consultant Registration list model
        /// </returns>
        Task<ConsultantRegistrationListModel> PrepareConsultantRegistrationListModelAsync(ConsultantRegistrationSearchModel searchModel);

        /// <summary>
        /// Prepare Consultant Registration model
        /// </summary>
        /// <param name="model">Log model</param>
        /// <param name="newRegistrations">new Consultant Registration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ConsultantRegistration overview model
        /// </returns>
        Task<ConsultantRegistrationOverviewModel> PrepareConsultantRegistrationOverviewModelAsync(ConsultantRegistrationOverviewModel model, NewRegistrations newRegistrations);

        #endregion

        #region Public methods

        /// <summary>
        /// Prepare Consultant Registration model
        /// </summary>
        /// <param name="model">Log model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ConsultantRegistration model
        /// </returns>
        Task<ConsultantRegistrationModel> PrepareConsultantRegistrationModelAsync(ConsultantRegistrationModel model = null);

        #endregion
    }
}
