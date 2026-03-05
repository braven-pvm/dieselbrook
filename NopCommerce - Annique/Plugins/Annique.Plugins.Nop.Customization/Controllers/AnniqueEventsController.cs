using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Factories.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Models.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.AnniqueEvents;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class AnniqueEventsController : BasePublicController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IEventService _eventService;
        private readonly IEventsModelFactory _eventsModelFactory;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public AnniqueEventsController(ICustomerService customerService,
            IWorkContext workContext,
            IEventService eventService,
            IEventsModelFactory eventsModelFactory,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            ILocalizationService localizationService,
            ILogger logger)
        {
            _customerService = customerService;
            _workContext = workContext;
            _eventService = eventService;
            _eventsModelFactory = eventsModelFactory;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _localizationService = localizationService;
            _logger = logger;
        }

        #endregion

        #region method

        //Event List
        public virtual async Task<IActionResult> EventList()
        {
            //Check for consultant role
            var isConsultantRole = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();

            if (!isConsultantRole)
                return Challenge();

            var model = await _eventsModelFactory.PrepareEventListModelAsync();
            return View(model);
        }

        //Event Booking Page
        public virtual async Task<IActionResult> BookingDetails(int eventId)
        {
            var anniqueEvent = await _eventService.GetEventByIdAsync(eventId);
            if (anniqueEvent == null || !anniqueEvent.Bookingopen)
                return InvokeHttp404();

            var customer = await _workContext.GetCurrentCustomerAsync();
            var bookings = await _eventService.GetAllBookingAsync(eventId,customer.Id);

            var model = await _eventsModelFactory.PrepareBookingDetailsModelAsync(new BookingDetailsModel(),anniqueEvent,bookings);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> BookingDelete(int bookingId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            //find booking
            var booking = await _eventService.GetBookingByIdAsync(bookingId);
            if (booking != null)
            {  
                //now delete the booking record
                await _eventService.RemoveBookingAsync(booking);

                //Remove related product from shopping cart
                await _eventService.RemoveEventProductFromCartAsync(booking.EventID, customer);
            }
            //redirect to the booking list page
            return Json(new
            {
                redirect = Url.RouteUrl("BookingDetails", new { eventId = booking.EventID }),
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> BookingAdd(BookingDetailsModel.BookingItemsModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            #region Validate & set consultantCustomerID

            var (success, message, consultantCustomerID, name) = await _eventService.ValidateConsultantBookingAsync(model);
            if (!success)
            {
                return Json(new { success = false, message });
            }

            #endregion

            if (ModelState.IsValid)
            {
                var booking = new Booking
                {
                    EventID = model.EventId,
                    CustomerID = customer.Id,
                    Name = name,
                    ConsultantCustomerID = consultantCustomerID
                };

                //Add booking
                await _eventService.InsertBookingAsync(booking);

                var isFreeEvent = await _eventService.IsFreeEventTicketAsync(model.EventId);
                if (!isFreeEvent)
                    //Add product accociated with event in shopping cart
                    await _eventService.InsertEventProductToCartAsync(booking.EventID);

                return Json(new
                {
                    success = true,
                    redirect = Url.RouteUrl("BookingDetails", new { eventId = booking.EventID }),
                });
            }

            return Json(new
            {
                success = false,
                error = await _localizationService.GetResourceAsync("AnniqueEvents.EventBooking.Error")
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> BookingUpdate(BookingDetailsModel.BookingItemsModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            //find booking
            var booking = await _eventService.GetBookingByIdAsync(model.Id);
            if (booking == null)
            {
                return Json(new
                {
                    success = false,
                    error = await _localizationService.GetResourceAsync("AnniqueEvents.EventBooking.NotFound"),
                });
            }

            #region Validate & set consultantCustomerID

            var (success, message, consultantCustomerID, name) = await _eventService.ValidateConsultantBookingAsync(model);
            if (!success)
            {
                return Json(new { success = false, message });
            }

            #endregion

            if (ModelState.IsValid)
            {
                booking.Name = name;
                booking.ConsultantCustomerID = consultantCustomerID;
                booking.dlastupd = DateTime.UtcNow;

                //update booking
                await _eventService.UpdateBookingAsync(booking);

                return Json(new
                {
                    success = true,
                    redirect = Url.RouteUrl("BookingDetails", new { eventId = booking.EventID }),
                });
            }

            return Json(new
            {
                success = false,
                error = await _localizationService.GetResourceAsync("AnniqueEvents.EventBooking.Error")
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> BookForSelf(int eventId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var booking = new Booking
            {
                EventID = eventId,
                CustomerID = customer.Id,
                IsPrimaryRegistrant = true,
                Name = await _customerService.GetCustomerFullNameAsync(customer)
            };

            //Add a booking
            await _eventService.InsertBookingAsync(booking);

            var isFreeEvent = await _eventService.IsFreeEventTicketAsync(eventId);
            if (!isFreeEvent)
                //Add product accociated with event in shopping cart
                await _eventService.InsertEventProductToCartAsync(booking.EventID);

            return RedirectToRoute("BookingDetails", new { eventId = booking.EventID });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmFreeEvent(int eventId)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (!await _customerService.IsRegisteredAsync(customer))
                    return Challenge();

                //Place order for free event
                await _eventService.FreeEventOrderPlaceAsync(eventId, customer);

                return Json(new
                {
                    success = true,
                    redirect = Url.RouteUrl("BookingDetails", new { eventId = eventId }),
                });

            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        #endregion
    }
}
