using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain
{
    public class UserProfileAdditionalInfo : BaseEntity
    {
        public int CustomerId { get; set; }

        public string Title { get; set; }

        public string Nationality { get; set; }

        public string IdNumber { get; set; }

        public string Language { get; set; }

        public string Ethnicity { get; set; }

        public string BankName { get; set; }

        public string AccountHolder { get; set; }

        public string AccountNumber { get; set; }

        public string AccountType { get; set; }

        public DateTime? ActivationDate { get; set; }

        public bool Accept { get; set; }

        public bool ProfileUpdated { get; set; }

        public string WhatsappNumber { get; set; }

        public int BrevoID { get; set; }
    }
}
