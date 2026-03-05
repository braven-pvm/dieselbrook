using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.StaffCustomerCheckout
{
    /// <summary>
    /// StaffCustomerCheckoutRuleService service
    /// </summary>
    public class StaffCustomerCheckoutRuleService : IStaffCustomerCheckoutRuleService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly ILocalizationService _localizationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public StaffCustomerCheckoutRuleService(IRepository<Order> orderRepository,
            IStoreContext storeContext,
            ISettingService settingService,
            IDateTimeHelper dateTimeHelper,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IWorkContext workContext)
        {
            _orderRepository = orderRepository;
            _storeContext = storeContext;
            _settingService = settingService;
            _dateTimeHelper = dateTimeHelper;
            _anniqueCustomizationConfigurationService= anniqueCustomizationConfigurationService;
            _localizationService = localizationService;
            _shoppingCartService = shoppingCartService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get Customers's current calender month's total order count
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains Customers's current calender month's total order count
        /// </returns>
        public async Task<int> GetCustomerTotalOrderCountAsync(int customerId)
        {
            int orderCount = 0;

            DateTime now = DateTime.Now;
            //get current calender month's first date
            var startDate = new DateTime(now.Year, now.Month, 1);

            //get current calender month's last date
            var endDate = startDate.AddMonths(1).AddDays(-1);

            //convert dates to current time zone
            var createdFromUtc = _dateTimeHelper.ConvertToUtcTime(startDate, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var createdToUtc = _dateTimeHelper.ConvertToUtcTime(endDate, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            //get customer's order for current calender month from order table
            var query = from o in _orderRepository.Table
                         where (o.CustomerId == customerId) &&
                         (createdFromUtc <= o.CreatedOnUtc) &&
                         (createdToUtc >= o.CreatedOnUtc) &&
                         ((int)PaymentStatus.Paid == o.PaymentStatusId) &&
                         !o.Deleted
                         select new { o };

            //Count customer orders
            orderCount = query.Select(co => co.o.Id).Count();

            return orderCount;
        }

        /// <summary>
        /// Validate order total amount and exceed amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the true - OK; false - order total amount is valid or not, if not valid then how much amount exceed from order amount limit
        /// </returns>
        public async Task<(bool isValidAmount, decimal exceedAmount)> ValidateOrderTotalAmountAsync(IList<ShoppingCartItem> cart, Customer customer)
        {
            bool isValidAmount = true;
            decimal exceedAmount = decimal.Zero;

            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            //get active store
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            var (subTotal, _) = await _anniqueCustomizationConfigurationService.GetShoppingCartTotalsBeforeDiscountAsync(cart);
            
            //if subtotal is greater than order amount limit
            if (subTotal > settings.OrderAmountLimit)
            { 
                isValidAmount = false;
                //get diffrence of how much amount is exceed
                exceedAmount = subTotal - settings.OrderAmountLimit;
            }

            return (isValidAmount,exceedAmount);
        }

        /// <summary>
        /// process staff shopping cart validation
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="model">Shopping cart model</param>
        /// <param name="anniqueCustomizationSettings">Annique customization settings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task<ShoppingCartModel> ProcessStaffShoppingCartValidationsAsync(Customer customer, Store store, ShoppingCartModel model, AnniqueCustomizationSettings anniqueCustomizationSettings)
        {
            // Get customer's cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            // Get customer's current calendar month's order count
            var orderCount = await GetCustomerTotalOrderCountAsync(customer.Id);

            // If order count is greater than or equal to the set limit
            if (orderCount >= anniqueCustomizationSettings.TotalOrderNo)
            {
                // Show warning
                model.MinOrderSubtotalWarning = await _localizationService.GetResourceAsync("Checkout.OrderTotalNoExceedCustomerStaff");
            }
            else
            {
                // If customer has not placed any order within the current month, then check if the order total amount is valid
                var (isValidOrderTotalAmount, exceededAmount) = await ValidateOrderTotalAmountAsync(cart, customer);

                // If amount is not valid, show with how much amount the order is not valid
                if (!isValidOrderTotalAmount)
                {
                    // Convert amount to store currency
                    var workingCurrency = await _workContext.GetWorkingCurrencyAsync();
                    var exceededOrderAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(exceededAmount, workingCurrency);

                    // Show message with exceed amount
                    var warningMessage = await _localizationService.GetResourceAsync("Checkout.OrderSubtotalAmountForCustomerStaff");
                    var formattedAmount = await _priceFormatter.FormatPriceAsync(exceededOrderAmount, true, false);
                    model.MinOrderSubtotalWarning = string.Format(warningMessage, formattedAmount);
                }
            }

            return model;
        }

        #endregion
    }
}

