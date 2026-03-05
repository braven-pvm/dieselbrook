using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.UserProfile
{
    public record UserProfileAdditionalInfoModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }

        //gets sets customerTitle
        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.Title")]
        public string Title { get; set; }

        public IList<SelectListItem> AvailableTitles { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.Nationality")]
        public string Nationality { get; set; }

        public IList<SelectListItem> AvaialableNationality { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.IdNumber")]
        public string IdNumber { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.Language")]
        public string Language { get; set; }

        public IList<SelectListItem> AvailableLanguages { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.Ethnicity")]
        public string Ethnicity { get; set; }

        public IList<SelectListItem> AvailableEthnicity { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.BankName")]
        public string BankName { get; set; }

        public IList<SelectListItem> AvailableBankNames { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.AccountHolder")]
        public string AccountHolder { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.AccountType")]
        public string AccountType { get; set;}
        public IList<SelectListItem> AvailableAccountTypes { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.ActivationDate")]
        public string ActivationDate { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.Accept")]
        public bool Accept { get; set; }

        [NopResourceDisplayName("Account.UserProfileAdditionalInfo.WhatsappNumber")]
        public string WhatsappNumber { get; set; }

        public bool ProfileUpdated { get; set; }

        public int BrevoID { get; set; }

        public bool IsConsultant { get; set; }

        public bool IsClientOrCustomer { get; set; }

    }
}
