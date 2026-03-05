using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Models.AnniqueEvents;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueEvents
{
    /// <summary>
    /// Event Service Class
    /// </summary>
    public class EventService : IEventService
    {
        #region Fields

        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public EventService(IRepository<Event> eventRepository,
            IRepository<Booking> bookingRepository,
            IWorkContext workContext,
            IProductService productService,
             IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            ICustomNumberFormatter customNumberFormatter,
            IOrderService orderService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            ICustomerService customerService,
            IAddressService addressService,
            ILocalizationService localizationService)
        {
            _eventRepository = eventRepository;
            _bookingRepository = bookingRepository;
            _workContext = workContext;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _orderService = orderService;
            _customNumberFormatter = customNumberFormatter;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _customerService = customerService;
            _addressService = addressService;
            _localizationService = localizationService;
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Gets all Events
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the events
        /// </returns>
        public async Task<IList<Event>> GetAllEventsAsync(bool? published = null, bool? active = null)
        {
            return await _eventRepository.GetAllAsync(query =>
            {
                if (published.HasValue)
                    query = query.Where(e => e.Published == published);

                if (active.HasValue)
                    query = query.Where(e => e.IActive == active);

                query = query.OrderBy(e => e.Id);

                return query;
            },_ => default);
        }

        /// <summary>
        /// Gets a Event
        /// </summary>
        /// <param name="id">Event identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Booking
        /// </returns>
        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await _eventRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Returns Wheather Event ticket is free or not
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if ticket price is zero
        public async Task<bool> IsFreeEventTicketAsync(int eventId)
        {
            var anniqueEvent = await GetEventByIdAsync(eventId);

            var product = await _productService.GetProductByIdAsync(anniqueEvent.ProductID);

            if (product.Price == decimal.Zero)
                return true;

            return false;
        }

        /// <summary>
        /// Gets a product by EventId
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        public async Task<Product> GetProductByEventIdAsync(int eventId)
        {
            var anniqueEvent = await GetEventByIdAsync(eventId);

            var product = await _productService.GetProductByIdAsync(anniqueEvent.ProductID);

            return product;
        }

        /// <summary>
        /// Gets a Event by product Id
        /// </summary>
        /// <param name="productId">ProductId Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        public async Task<Event> GetEventByProductIdAsync(int productId)
        {
            var query = from e in _eventRepository.Table
                        where e.ProductID == productId
                        && e.Published
                        && e.IActive
                        && e.Bookingopen
                        select e;

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a Events by product Ids
        /// </summary>
        /// <param name="productIds">ProductId Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the events
        /// </returns>
        public async Task<IList<Event>> GetEventsByProductIdsAsync(IEnumerable<int> productIds)
        {
            var query = from e in _eventRepository.Table
                        where productIds.Contains(e.ProductID)
                           && e.Published
                           && e.IActive
                           && e.Bookingopen
                        select e;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Inserts event product to shopping cart
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task InsertEventProductToCartAsync(int eventId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var product = await GetProductByEventIdAsync(eventId);
            product.Published = true;

            //Add ticket to shopping cart
            await _shoppingCartService.AddToCartAsync(customer, product, ShoppingCartType.ShoppingCart, store.Id, null, 0, null, null, 1, true);
        }

        /// <summary>
        /// Remove event product from shopping cart
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task RemoveEventProductFromCartAsync(int eventId, Customer customer)
        {
            //Get product accocoiated with event
            var product = await GetProductByEventIdAsync(eventId);

            var store = await _storeContext.GetCurrentStoreAsync();

            //Customer current cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            //first, try to find product in existing shopping cart 
            var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, ShoppingCartType.ShoppingCart, product);
            if (shoppingCartItem != null)
            {
                var newQuantity = 0;
                if (shoppingCartItem.Quantity > 0)
                    newQuantity = shoppingCartItem.Quantity - 1;

                //update cart
                await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                    shoppingCartItem.Id, shoppingCartItem.AttributesXml, shoppingCartItem.CustomerEnteredPrice,
                    shoppingCartItem.RentalStartDateUtc, shoppingCartItem.RentalEndDateUtc, newQuantity, true);
            }
        }

        /// <summary>
        /// Free event Place order
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task FreeEventOrderPlaceAsync(int eventId, Customer customer)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            var product = await GetProductByEventIdAsync(eventId);

            //var bookings = (await GetAllBookingAsync(eventId)).Where(b => b.OrderID == 0);
            var bookings = (await GetAllBookingAsync(eventId,customer.Id)).Where(b => b.OrderID == 0);
            var addressId = customer.BillingAddressId;
            //if no billing address
            if (!addressId.HasValue)
            {
                //create dummy address
                var address = new Address()
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _addressService.InsertAddressAsync(address);
                customer.BillingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);
            }

            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(customer.CurrencyId ?? 0);
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : currentCurrency;
            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            var order = new Order
            {
                StoreId = store.Id,
                OrderGuid = Guid.NewGuid(),
                CustomerId = customer.Id,
                AffiliateId = customer.AffiliateId,
                BillingAddressId = (int)customer.BillingAddressId,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                CustomerTaxDisplayType = 0,
                CustomerCurrencyCode = customerCurrency.CurrencyCode,
                CurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate,
                OrderSubtotalInclTax = decimal.Zero,
                OrderSubtotalExclTax = decimal.Zero,
                OrderSubTotalDiscountInclTax = decimal.Zero,
                OrderSubTotalDiscountExclTax = decimal.Zero,
                OrderShippingInclTax = decimal.Zero,
                OrderShippingExclTax = decimal.Zero,
                PaymentMethodAdditionalFeeInclTax = decimal.Zero,
                PaymentMethodAdditionalFeeExclTax = decimal.Zero,
                AllowStoringCreditCardNumber = false,
                OrderTax = decimal.Zero,
                OrderTotal = decimal.Zero,
                RefundedAmount = decimal.Zero,
                OrderDiscount = decimal.Zero,
                OrderStatus = OrderStatus.Processing,
                PaymentStatus = PaymentStatus.Paid,
                ShippingStatus = ShippingStatus.ShippingNotRequired,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty
            };

            await _orderService.InsertOrderAsync(order);

            //generate and set custom order number
            order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
            await _orderService.UpdateOrderAsync(order);

            //save order item
            var orderItem = new OrderItem
            {
                OrderItemGuid = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                UnitPriceInclTax = decimal.Zero,
                UnitPriceExclTax = decimal.Zero,
                PriceInclTax = decimal.Zero,
                PriceExclTax = decimal.Zero,
                OriginalProductCost = decimal.Zero,
                AttributeDescription = await _localizationService.GetResourceAsync("AnniqueEvents.OrderItem.AttributeDescription"),
                AttributesXml = null,
                Quantity = bookings.Count(),
                DiscountAmountInclTax = decimal.Zero,
                DiscountAmountExclTax = decimal.Zero,
                DownloadCount = 0,
                IsDownloadActivated = false,
                LicenseDownloadId = 0,
                ItemWeight = decimal.Zero,
                RentalStartDateUtc = null,
                RentalEndDateUtc = null
            };

            await _orderService.InsertOrderItemAsync(orderItem);

            //Update bookings table
            foreach (var booking in bookings)
            {
                booking.OrderID = order.Id;
                booking.DateBooked = DateTime.Now;
                booking.dlastupd = DateTime.Now;
                await UpdateBookingAsync(booking);
            }
        }

        #endregion

        #region Booking Methods

        /// <summary>
        /// Inserts a Booking
        /// </summary>
        /// <param name="booking">booking</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertBookingAsync(Booking booking)
        {
            await _bookingRepository.InsertAsync(booking);
        }

        /// <summary>
        /// Update a Booking
        /// </summary>
        /// <param name="Booking">Booking</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateBookingAsync(Booking booking)
        {
            await _bookingRepository.UpdateAsync(booking);
        }

        /// <summary>
        /// Remove a Booking
        /// </summary>
        /// <param name="Booking">Booking</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task RemoveBookingAsync(Booking booking)
        {
            await _bookingRepository.DeleteAsync(booking);
        }

        /// <summary>
        /// Remove a Bookings
        /// </summary>
        /// <param name="bookings">Bookings</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task RemoveBookingsAsync(IList<Booking> bookings)
        {
            await _bookingRepository.DeleteAsync(bookings);
        }

        /// <summary>
        /// Gets all Bookings related to Event
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        /// <param name="customerId">Customer Identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the events
        /// </returns>
        public async Task<IList<Booking>> GetAllBookingAsync(int eventId,int customerId)
        {
            //var customer = await _workContext.GetCurrentCustomerAsync();
            var query = _bookingRepository.Table;

            query = query.Where(b => b.EventID == eventId && b.CustomerID == customerId);

            return await query.OrderBy(t => t.Id).ToListAsync();
        }

        /// <summary>
        /// Gets all Bookings 
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the events
        /// </returns>
        public async Task<IList<Booking>> GetAllBookingsAsync()
        {
            var query = _bookingRepository.Table;
            return await query.OrderBy(t => t.Id).ToListAsync();
        }

        /// <summary>
        /// Gets a Booking
        /// </summary>
        /// <param name="bookingId">Booking identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Booking
        /// </returns>
        public async Task<Booking> GetBookingByIdAsync(int bookingId)
        { 
            return await _bookingRepository.GetByIdAsync(bookingId);
        }

        /// <summary>
        /// Returns Wheather Consultant Guest ticket Exist or not
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        ///<param name="customerID"> Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if Guest ticket already exist
        public bool IsConsultantGuestExist(int eventId, int customerId)
        {
            var query = from b in _bookingRepository.Table
                        where b.EventID == eventId && (b.ConsultantCustomerID == customerId || (b.CustomerID == customerId && b.IsPrimaryRegistrant))
                        select b;

            if (query.Any())
                return true;

            return false;
        }

        /// <summary>
        /// Returns Consultant Guest ticket Exist by customer and order number
        /// </summary>
        /// <param name="eventId">Event Identifier</param>
        /// <param name="bookingId">Booking Identifier</param>
        /// <param name="consultantCustomerID">Consultant Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns username of customer who booked Guest ticket with order number
        public async Task<(string, int)> ConsultantGuestBookedByUserAsync(int eventId, int bookingId, int consultantCustomerID)
        {
            var username = string.Empty;
            var orderNumber = 0;
            var query = from b in _bookingRepository.Table
                        where b.EventID == eventId && b.Id != bookingId && (b.ConsultantCustomerID == consultantCustomerID || b.CustomerID == consultantCustomerID)
                        select b;

            if (query.Any())
            {
                var customerId = query.Select(b => b.CustomerID).FirstOrDefault();
                orderNumber = query.Select(b => b.OrderID).FirstOrDefault();
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                username = await _customerService.GetCustomerFullNameAsync(customer);
            }

            return (username,orderNumber);
        }

        /// <summary>
        /// Process events on order placed event
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItems">Order Items</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ProcessEventsOnOrderPlacedAsync(Order order,IList<OrderItem> orderItems)
        {
            // Get distinct ProductIds from orderItems
            var productIds = orderItems.Select(oi => oi.ProductId).Distinct();

            // Events by productIds
            var anniqueEvents = await GetEventsByProductIdsAsync(productIds);
            if (anniqueEvents.Any())
            {
                foreach (var anniqueEvent in anniqueEvents)
                {
                    // Get event bookings
                    var bookings = (await GetAllBookingAsync(anniqueEvent.Id, order.CustomerId))
                        .Where(b => b.OrderID == 0).ToList();

                    // Order note variable with total number of bookings
                    var orderNote = $"{anniqueEvent.Name} Total Bookings: {bookings.Count}\n";

                    // Update bookings table
                    foreach (var booking in bookings)
                    {
                        // Append information to order note for trace issue
                        orderNote += $"BookingId: {booking.Id}, OrderId: {order.Id}\n";

                        booking.OrderID = order.Id;
                        booking.DateBooked = DateTime.UtcNow;
                        booking.dlastupd = DateTime.UtcNow;
                        booking.Status = order.OrderStatusId.ToString();
                        await UpdateBookingAsync(booking);
                    }

                    // Insert order note
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = orderNote,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    var item = orderItems.FirstOrDefault(o => o.ProductId == anniqueEvent.ProductID);
                    if (item != null)
                    {
                        // Update order item attribute description 'Ticket'
                        item.AttributeDescription = await _localizationService.GetResourceAsync("AnniqueEvents.OrderItem.AttributeDescription");
                        await _orderService.UpdateOrderItemAsync(item);
                    }
                }
            }
        }

        /// <summary>
        /// handle event bookings on cart item removal 
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="cart">cart Items</param>
        /// <param name="itemIdsToRemove">item ids to remove</param>
        /// <returns>Handles event bookings when event related cart item removed</returns>
        public async Task HandleEventRelatedCartItemRemovalAsync(Customer customer,IList<ShoppingCartItem> cart,IEnumerable<int> itemIdsToRemove)
        {
            foreach (var id in itemIdsToRemove)
            {
                var shoppingCartItem = cart.Where(item => item.Id == id).FirstOrDefault();
                if (shoppingCartItem != null)
                {
                    var anniqueEvent = await GetEventByProductIdAsync(shoppingCartItem.ProductId);
                    if (anniqueEvent != null)
                    {
                        // Get all bookings for the customer and event
                        var eventBookings = await GetAllBookingAsync(anniqueEvent.Id, customer.Id);
                        if (eventBookings != null)
                        {
                            // Get only unconfirmed bookings
                            var itemToRemove = eventBookings.Where(e => e.OrderID == 0).ToList();

                            // Remove unconfirmed bookings
                            await RemoveBookingsAsync(itemToRemove);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// validate consultant booking
        /// </summary>
        /// <param name="model">Booking items model</param>
        /// <returns>validates consultant booking already exist or not</returns>
        public async Task<(bool success, string message, int consultantCustomerID, string name)> ValidateConsultantBookingAsync(BookingDetailsModel.BookingItemsModel model)
        {
            var consultantCustomerID = 0;
            var name = model.Name;

            if (!string.IsNullOrEmpty(model.Consultant))
            {
                var consultantCustomer = await _customerService.GetCustomerByUsernameAsync(model.Consultant);
                if (consultantCustomer != null)
                {
                    consultantCustomerID = consultantCustomer.Id;
                    name = await _customerService.GetCustomerFullNameAsync(consultantCustomer);
                }
                else
                {
                    var message = await _localizationService.GetResourceAsync("AnniqueEvents.EventBooking.Consultant.InValid");
                    return (false, message, consultantCustomerID, name);
                }

                if (consultantCustomerID > 0)
                {
                    // Check if a ticket already exists for another consultant's guest
                    var (userFullName, orderNumber) = await ConsultantGuestBookedByUserAsync(model.EventId, model.Id, consultantCustomerID);
                    if (!string.IsNullOrWhiteSpace(userFullName))
                    {
                        var message = string.Format(await _localizationService.GetResourceAsync("AnniqueEvents.EventBooking.Consultant.AlreadyExist"), userFullName, orderNumber);
                        return (false, message, consultantCustomerID, name);
                    }
                }
            }

            return (true, null, consultantCustomerID, name);
        }
        #endregion
    }
}
