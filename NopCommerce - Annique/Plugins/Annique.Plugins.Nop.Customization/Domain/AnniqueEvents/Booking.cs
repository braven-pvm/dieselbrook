using Nop.Core;
using System;
namespace Annique.Plugins.Nop.Customization.Domain.AnniqueEvents
{
    public class Booking : BaseEntity
    {
        public int EventID { get; set; }

        public int CustomerID { get; set; }

        public int ConsultantCustomerID { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public DateTime DateBooked { get; set; }

        public string Attended { get; set; }

        public int OrderID { get; set; }

        public string cSono { get; set; }
       
        public string cInvno { get; set; }

        public bool IsPrimaryRegistrant { get; set; }

        public DateTime dlastupd { get; set; }

        public bool IEmail { get; set; }
    }
}
