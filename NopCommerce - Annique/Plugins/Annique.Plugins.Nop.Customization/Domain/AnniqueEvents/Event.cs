using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain.AnniqueEvents
{
    public class Event : BaseEntity
    {
       public string Name { get; set; }

       public DateTime StartDateTime { get; set; }

       public DateTime EndDateTime { get; set; }

       public string LocationName { get; set; }

       public string LocationAddress1 { get; set; }

       public string LocationAddress2 { get; set; }

       public string LocationCity { get; set;}
      
       public string LocationLocation { get; set;}

       public string LocationPostalCode { get; set;}

        public string LocationCountry { get; set;}

        public string ContactName { get; set; }

        public string ContactEmail { get; set; }

        public string ContactPhone { get; set; }

        public string ShortDescription { get; set; }

        public string TicketCode { get; set; }

        public int ProductID { get; set; }

        public bool Bookingopen { get; set; }

        public bool IActive { get; set; }

        public DateTime dlastupd { get; set; }

        public string ZoomCode { get; set; }

        public bool IsOnline { get; set; }

        public bool Published { get; set; }

        public bool isField { get; set; }

        public bool isOptIn { get; set; }

        public int CloseDays { get; set; }

        public int BookingOpenDays { get; set; }

        public int HOAprovalDays { get; set; }

        public int NOTIFICATIONDays { get; set; }

        public int LoadItemsDays { get; set; }

        public int NotOrderedDays { get; set; }
    }
}
