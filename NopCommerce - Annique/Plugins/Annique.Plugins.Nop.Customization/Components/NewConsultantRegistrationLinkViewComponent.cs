using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public;
using Annique.Plugins.Nop.Customization.Services.ConsultantRegistrations;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays new consultant link
    /// </summary>
    [ViewComponent(Name = "NewConsultantRegistrationLink")]
    public class NewConsultantRegistrationLinkViewComponent : ViewComponent
    {
        private readonly IConsultantNewRegistrationService _consultantNewRegistrationService;

        public NewConsultantRegistrationLinkViewComponent(IConsultantNewRegistrationService consultantNewRegistrationService)
        {
            _consultantNewRegistrationService = consultantNewRegistrationService;
        }

        #region Method

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var isNopConsultantRegistrationEnabled = await _consultantNewRegistrationService.IsNopBasedConsultantRegistrationEnabledAsync();
            var model = new ConsultantRegistrationLinkModel
            {
                RegistrationLink = isNopConsultantRegistrationEnabled ? "/consultant-register" : "../new-consultant-registration"
            };
            return View(model);
        }

        #endregion
    }
}