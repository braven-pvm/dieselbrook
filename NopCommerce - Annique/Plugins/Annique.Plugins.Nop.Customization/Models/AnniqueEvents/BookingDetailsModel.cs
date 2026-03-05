using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueEvents
{
    public record BookingDetailsModel : BaseNopModel
    {
        public BookingDetailsModel()
        {
            Items = new List<BookingItemsModel>();
        }

        public int EventId { get; set; }

        public string EventName { get; set; }

        public bool IsFreeTicket { get; set; }

        public bool ShowBookForSelfPopUp { get; set; }

        public IList<BookingItemsModel> Items { get; set; }

        #region Nested Class

        public record BookingItemsModel : BaseNopModel
        {
            public int Id { get; set; }

            public int EventId { get; set; }

            public int CustomerId { get; set; }

            public string Consultant { get; set; }

            public string Name { get; set; }

            public int OrderId { get; set; }

            public string Confirmed { get; set; }

            public bool IsPrimaryRegistrant { get; set; }
        }

        #endregion
    }
}
