using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Admin
{
    public record ConsultantRegistrationOverviewModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Fullname")]
        public string Fullname { get; set; }

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

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Postcode")]
        public string Postcode { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Country")]
        public string Country { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.BestTimeToCall")]
        public string SelectedCallTime { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.IPAddress")]
        public string IPAddress { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Browser")]
        public string Browser { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Sponsor")]
        public string Sponsor { get; set; }

        [NopResourceDisplayName("Plugins.ConsultantRegistration.Fields.Status")]
        public string Status { get; set; }
    }
}
