using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.GiftCardAllocation;
using Annique.Plugins.Nop.Customization.Services.NewActivityLogs;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class CheckoutCompletedActionFilter : IActionFilter
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IGiftService _giftService;
        private readonly IGiftCardAdditionalInfoService _giftCardAdditionalInfoService;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly IAwardService _awardService;
        private readonly ILogger _logger;
        private readonly IAdditionalActivityLogService  _additionalActivityLogService;

        #endregion

        #region Ctor

        public CheckoutCompletedActionFilter(IStoreContext storeContext,
            ISettingService settingService,
            IOrderService orderService,
            ICustomerService customerService,
            IWorkContext workContext,
            IGiftService giftService,
            IGiftCardAdditionalInfoService giftCardAdditionalInfoService,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            IAwardService awardService,
            ILogger logger,
            IAdditionalActivityLogService additionalActivityLogService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _orderService = orderService;
            _customerService = customerService;
            _workContext = workContext;
            _giftService = giftService;
            _giftCardAdditionalInfoService = giftCardAdditionalInfoService;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _awardService = awardService;
            _logger = logger;
            _additionalActivityLogService = additionalActivityLogService;
        }

        #endregion

        #region Utilities

        private async Task HandleConsultantRoleTasks(Order order)
        {
            // Add order note to track that the payment status is Paid
            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                Note = "Payment status is Paid. Proceeding with activation date update.",
                DisplayToCustomer = false,  // Optionally set to true if you want the customer to see this note
                CreatedOnUtc = DateTime.UtcNow
            };

            // Insert the order note into the database
            await _orderService.InsertOrderNoteAsync(orderNote);

            //process giftcard usage
            await _giftCardAdditionalInfoService.ProcessGiftcardUsageOnCheckoutAsync(order);

            //Get order items
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            //process gifts takens and order notes related to gifts
            await _giftService.ProcessGiftsTakenAsync(order.CustomerId, orderItems);

            #region Update Activation Date for Consultant

            await _userProfileAdditionalInfoService.UpdateActivationDateOnFirstOrderAsync(order);

            #endregion

            #region Update Award taken

            await _awardService.ProcessAwardsTakenAsync(orderItems);

            #endregion
        }

        private async Task HandlePaymentStatusNotPaid(Order order)
        {
            // Add a note if the payment status is not Paid
            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                Note = "Payment status is not Paid. Activation date update skipped.",
                DisplayToCustomer = false,  // Optionally set to true if you want the customer to see this note
                CreatedOnUtc = DateTime.UtcNow
            };

            // Insert the order note into the database
            await _orderService.InsertOrderNoteAsync(orderNote);
        }

        #endregion

        #region Methods

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {

        }

        public async void OnActionExecuted(ActionExecutedContext context)
        {
            var controller = (ControllerActionDescriptor)context.ActionDescriptor;

            if (controller.ControllerTypeInfo == typeof(CheckoutController) && controller.ActionName.Equals("Completed"))
            {
                try
                {
                    var store = await _storeContext.GetCurrentStoreAsync();

                    //get Active store Annique Settings
                    var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                    //Get current customer and customer roles
                    var customer = await _workContext.GetCurrentCustomerAsync();

                    var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

                    //If plugin is enable & check customer contains Consultant role
                    if (settings.IsEnablePlugin)
                    {
                        context.RouteData.Values.TryGetValue("orderId", out object orderId);

                        if (orderId != null)
                        {
                            var Id = Convert.ToInt32(orderId);
                            if (Id > 0)
                            {
                                //get order by ID
                                var order = await _orderService.GetOrderByIdAsync(Id);

                                //If payment status paid the update bookings table
                                if (order.PaymentStatusId == (int)PaymentStatus.Paid)
                                {
                                    if (customerRoleIds.Contains(settings.ConsultantRoleId))
                                        await HandleConsultantRoleTasks(order);

                                    #region Task 644 new Activity logs

                                    await _additionalActivityLogService.InsertActivityTrackingLogAsync("PublicStore.OrderPaid", "Order Paid", customer);

                                    #endregion
                                }
                                else
                                {
                                    if (customerRoleIds.Contains(settings.ConsultantRoleId))
                                        await HandlePaymentStatusNotPaid(order);
                                }
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                }
            }
        }

        #endregion
    }
}
