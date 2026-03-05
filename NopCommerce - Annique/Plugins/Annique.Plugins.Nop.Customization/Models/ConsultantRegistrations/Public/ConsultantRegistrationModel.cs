using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public
{
    public record ConsultantRegistrationModel : BaseNopEntityModel
    {
        public ConsultantPostResgistrationModel ConsultantPostResgistrationModel { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.ConfirmEmail")]
        public string ConfirmEmail { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Cell")]
        public string Cell { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Whatsapp")]
        public string Whatsapp { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Language")]
        public string SelectedLanguage { get; set; }
        public IList<SelectListItem> Languages { get; set; } = new List<SelectListItem>();

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Postcode")]
        public string Postcode { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.BestTimeToCall")]
        public string SelectedCallTime { get; set; }
        public IList<SelectListItem> CallTimes { get; set; } = new List<SelectListItem>();

        public bool DisplayCaptcha { get; set; }

        public string Browser {  get; set; }

        public string AffiliateName { get; set; }

        public string Css { get; set; }

        public string Js { get; set; }

        public string TopSection { get; set; }

        public string LeftSection { get; set; }

        public string BottomSection { get; set; }

        public string Csponser { get; set; }

    }

    public class ConsultantPostResgistrationModel
    {
        public bool ShowWelcomePopup { get; set; }
        public string WelcomeMessage { get; set; }
        public bool ShowCustomerExistPopup { get; set; }
        public string ExistingCustomerMessage { get; set; }
        public string RedirectUrl { get; set; }
    }

}
