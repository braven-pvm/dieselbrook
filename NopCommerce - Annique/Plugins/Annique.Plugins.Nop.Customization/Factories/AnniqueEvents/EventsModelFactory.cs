using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Models.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Services.AnniqueEvents;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Tax;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.AnniqueEvents
{
    /// <summary>
    /// Event factory Class
    /// </summary>
    public class EventsModelFactory : IEventsModelFactory
    {
        #region Fields

        private readonly IEventService _eventService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;

        #endregion

        #region Ctor

        public EventsModelFactory(IEventService eventService,
            IPictureService pictureService,
            IProductService productService,
            IPriceCalculationService priceCalculationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPriceFormatter priceFormatter,
            ILocalizationService localizationService,
            MediaSettings mediaSettings,
            ICustomerService customerService,
            ISettingService settingService,
            ITaxService taxService)
        {
            _eventService = eventService;
            _pictureService = pictureService;
            _productService = productService;
            _priceCalculationService = priceCalculationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _priceFormatter = priceFormatter;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _customerService = customerService;
            _settingService = settingService;
            _taxService = taxService;
        }

        #endregion

        #region Utilites

        /// <summary>
        /// Prepare the Event Picture Model
        /// </summary>
        /// <param name="event">Event</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the picture model for the default picture
        /// </returns>
        protected async Task<PictureModel> PrepareEventPictureModelAsync(Event e)
        {
            var pictures = await _pictureService.GetPicturesByProductIdAsync(e.ProductID, 1);
            var defaultPicture = pictures.FirstOrDefault();

            var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;

            string fullSizeImageUrl, imageUrl;
            (imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, false);
            (fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, false);

            var defaultPictureModel = new PictureModel
            {
                ImageUrl = imageUrl,
                FullSizeImageUrl = fullSizeImageUrl,

                //"title" attribute
                Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                defaultPicture.TitleAttribute :
                string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), e.Name),
                //"alt" attribute
                AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                defaultPicture.AltAttribute :
                string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), e.Name)
            };

            return defaultPictureModel;
        }

        /// <summary>
        /// Prepare the Booking item model
        /// </summary>
        /// <param name="bookings">List of the booking item</param>
        /// <param name="booking">Booking item</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Booking item model
        /// </returns>
        protected async Task<BookingDetailsModel.BookingItemsModel> PrepareBookingItemModelAsync(IList<Booking> bookings, Booking booking)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //prepare booking item model
            var bookingItemModel = new BookingDetailsModel.BookingItemsModel
            {
                Id = booking.Id,
                EventId = booking.EventID,
                Name = booking.Name,
                CustomerId = booking.CustomerID,
                OrderId = booking.OrderID,
                IsPrimaryRegistrant = booking.IsPrimaryRegistrant,
            };

            //cSono shows booking confirm from back office
            if (!string.IsNullOrEmpty(booking.cSono))
                bookingItemModel.Confirmed = booking.cSono;

            //Show consultant name for primary registrant or another consultant book by current customer
            var customer = new Customer();
            if(booking.IsPrimaryRegistrant && booking.ConsultantCustomerID == 0)
            {
                customer = await _customerService.GetCustomerByIdAsync(booking.CustomerID);
            }
            
            if(!booking.IsPrimaryRegistrant && booking.ConsultantCustomerID > 0)
            {
                customer = await _customerService.GetCustomerByIdAsync(booking.ConsultantCustomerID);
            }

            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            //check customer contains Consultant role 
            if (customerRoleIds.Contains(settings.ConsultantRoleId))
                bookingItemModel.Consultant = customer.Username;

            return bookingItemModel;
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Prepare the Event list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Event list model
        /// </returns>
        public async Task<EventListModel> PrepareEventListModelAsync()
        {
            var model = new EventListModel();

            //Get all Events
            var events = await _eventService.GetAllEventsAsync();

            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();

            foreach (var e in events)
            {
                if (e.ProductID == null || e.ProductID == 0)
                    continue;

                var eventModel = new EventListModel.EventDetailsModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.ShortDescription,
                    Time = e.LocationName,
                    BookingOpen = e.Bookingopen
                };

                //if start date end date same then only show date one time
                if (e.StartDateTime == e.EndDateTime)
                    eventModel.Date = e.StartDateTime.ToString("dddd, dd MMMM yyyy");
                else
                    eventModel.Date = $"{e.StartDateTime:dddd, dd MMMM yyyy} to { e.EndDateTime:dddd, dd MMMM yyyy}";

                if(!string.IsNullOrEmpty(e.LocationAddress1))
                    eventModel.LocationAddress1 = e.LocationAddress1;

                if (!string.IsNullOrEmpty(e.LocationAddress2))
                    eventModel.LocationAddress2 = e.LocationAddress2;

                if (e.ProductID > 0)
                {
                    //prepare event picture Model
                    eventModel.PictureModel = await PrepareEventPictureModelAsync(e);

                    //Get product from event
                    var product = await _productService.GetProductByIdAsync(e.ProductID);

                    //Get product price with include tax
                    var (_, price, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store,decimal.Zero,false,1);
                    var (priceWithTax, _) = await _taxService.GetProductPriceAsync(product, price);

                    eventModel.Price = await _priceFormatter.FormatPriceAsync(priceWithTax, true, false);
                }

                model.Events.Add(eventModel);
            }
            return model;
        }

        #endregion

        #region Booking Methods

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
        public async Task<BookingDetailsModel> PrepareBookingDetailsModelAsync(BookingDetailsModel model,Event anniqueEvent,
            IList<Booking> bookings)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.EventId = anniqueEvent.Id;
            model.EventName = anniqueEvent.Name;
            model.IsFreeTicket = await _eventService.IsFreeEventTicketAsync(anniqueEvent.Id);
            model.ShowBookForSelfPopUp = true;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var existCustomer = _eventService.IsConsultantGuestExist(anniqueEvent.Id, customer.Id);

            //check any booking for current customer's it self
            if (existCustomer)
                model.ShowBookForSelfPopUp = false;

            if (!bookings.Any())
                return model;
            
            //booking items
            foreach (var booking in bookings)
            {
                var bookingItemModel = await PrepareBookingItemModelAsync(bookings, booking);
                model.Items.Add(bookingItemModel);
            }

            return model;
        }

        #endregion
    }
}
