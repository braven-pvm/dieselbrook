using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Models.AnniqueEvents;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.AnniqueEvents
{
    /// <summary>
    /// Event factory Interface
    /// </summary>
    public interface IEventsModelFactory
    {
        /// <summary>
        /// Prepare the Event list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Event list model
        /// </returns>
        Task<EventListModel> PrepareEventListModelAsync();

        /// <summary>
        /// Prepare the Booking Details
        /// </summary>
        /// <param name="model">Booking Details model</param>
        /// <param name="anniqueEvent">Annique Event</param>
        /// <param name="booking">List of the booking item</param>
        ///<returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Booking Details model
        /// </returns>
        Task<BookingDetailsModel> PrepareBookingDetailsModelAsync(BookingDetailsModel model, Event anniqueEvent,
            IList<Booking> bookings);
    }
}
