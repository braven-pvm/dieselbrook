using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Models.AnniqueEvents;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueEvents
{
    /// <summary>
    /// Event Service interface
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Gets all Events
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the events
        /// </returns>
        Task<IList<Event>> GetAllEventsAsync(bool? published = null, bool? active = null);

        /// <summary>
        /// Gets a Event
        /// </summary>
        /// <param name="id">Event identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Booking
        /// </returns>
        Task<Event> GetEventByIdAsync(int id);

        /// <summary>
        /// Returns Wheather Event ticket is free or not
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if ticket price is zero
        Task<bool> IsFreeEventTicketAsync(int eventId);

        /// <summary>
        /// Gets a product by EventId
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        Task<Product> GetProductByEventIdAsync(int eventId);

        /// <summary>
        /// Gets a Event by product Id
        /// </summary>
        /// <param name="productId">ProductId Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        Task<Event> GetEventByProductIdAsync(int productId);

        /// <summary>
        /// Gets a Events by product Ids
        /// </summary>
        /// <param name="productIds">ProductId Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the events
        /// </returns>
        Task<IList<Event>> GetEventsByProductIdsAsync(IEnumerable<int> productIds);

        /// <summary>
        /// Inserts event product to shopping cart
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task InsertEventProductToCartAsync(int eventId);

        /// <summary>
        /// Remove event product from shopping cart
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task RemoveEventProductFromCartAsync(int eventId, Customer customer);

        /// <summary>
        /// Inserts a Booking
        /// </summary>
        /// <param name="booking">booking</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertBookingAsync(Booking booking);

        /// <summary>
        /// Update a Booking
        /// </summary>
        /// <param name="Booking">Booking</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateBookingAsync(Booking booking);

        /// <summary>
        /// Remove a Booking
        /// </summary>
        /// <param name="Booking">Booking</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RemoveBookingAsync(Booking booking);

        /// <summary>
        /// Remove a Bookings
        /// </summary>
        /// <param name="bookings">Bookings</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RemoveBookingsAsync(IList<Booking> bookings);

        /// <summary>
        /// Gets all Bookings related to Event
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        /// <param name="customerId">Customer Identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the events
        /// </returns>
        Task<IList<Booking>> GetAllBookingAsync(int eventId, int customerId);

        /// <summary>
        /// Gets all Bookings 
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the events
        /// </returns>
        Task<IList<Booking>> GetAllBookingsAsync();

        /// <summary>
        /// Gets a Booking
        /// </summary>
        /// <param name="bookingId">Booking identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Booking
        /// </returns>
        Task<Booking> GetBookingByIdAsync(int bookingId);

        /// <summary>
        /// Returns Wheather Consultant Guest ticket Exist or not
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        ///<param name="customerID"> Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if Guest ticket already exist
        bool IsConsultantGuestExist(int eventId, int customerID);

        /// <summary>
        /// Returns Consultant Guest ticket Exist by customer and order number
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        /// <param name="bookingId">Booking Identifier</param>
        /// <param name="consultantCustomerID">Consultant Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns username of customer who booked Guest ticket with order number
        Task<(string,int)> ConsultantGuestBookedByUserAsync(int eventId, int bookingId, int consultantCustomerID);

        /// <summary>
        /// Free event Place order
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task FreeEventOrderPlaceAsync(int eventId, Customer customer);

        /// <summary>
        /// Process events on order placed event
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItems">Order Items</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ProcessEventsOnOrderPlacedAsync(Order order,IList<OrderItem> orderItems);

        /// <summary>
        /// handle event bookings on cart item removal 
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="cart">cart Items</param>
        /// <param name="itemIdsToRemove">item ids to remove</param>
        /// <returns>Handles event bookings when event related cart item removed</returns>
        Task HandleEventRelatedCartItemRemovalAsync(Customer customer,IList<ShoppingCartItem> cart ,IEnumerable<int> itemIdsToRemove);

        /// <summary>
        /// validate consultant booking
        /// </summary>
        /// <param name="model">Booking items model</param>
        /// <returns>validates consultant booking already exist or not</returns>
        Task<(bool success, string message, int consultantCustomerID, string name)> ValidateConsultantBookingAsync(BookingDetailsModel.BookingItemsModel model);
    }
}
