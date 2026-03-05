using Annique.Plugins.Payments.AdumoOnline.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Payments.AdumoOnline.Controllers
{
    public class PaymentAdumoOnlineController : BasePublicController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IAdumoOnlinePaymentModelFactory _adumoOnlinePaymentModelFactory;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PaymentAdumoOnlineController(ISettingService settingService,
            IStoreContext storeContext,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IAdumoOnlinePaymentModelFactory adumoOnlinePaymentModelFactory,
            IPaymentPluginManager paymentPluginManager,
            ILogger logger,
            IWorkContext workContext)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _adumoOnlinePaymentModelFactory = adumoOnlinePaymentModelFactory;
            _paymentPluginManager = paymentPluginManager;
            _logger = logger;
            _workContext = workContext;
        }

        #endregion

        #region Utitlity

        //prepare order note from adumo online payment response
        private async Task AddOrderNoteAsync(int orderId, IFormCollection formData)
        {
            //prepare string of key value pair from adumo response where value is not null or empty
            string adumoResponse = string.Join(", ", formData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{kv.Key}: {kv.Value}")
                .ToArray());

            //insert order note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = orderId,
                Note = adumoResponse,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        #endregion

        #region Method

        public async Task<IActionResult> CheckoutView(Guid orderGuid)
        {
            try
            {
                //load setting according store id
                var store = await _storeContext.GetCurrentStoreAsync();
                var settings = await _settingService.LoadSettingAsync<AdumoOnlineSettings>(store.Id);

                //check adumo online payment plugin installed, activated and enabled from plugin manager and configuration settings
                //if not then throw exception
                if (await _paymentPluginManager.LoadPluginBySystemNameAsync("Annique.AdumoOnline") is not AdumoOnlinePaymentMethods adumoProcessor || !_paymentPluginManager.IsPluginActive(adumoProcessor) || !settings.IsEnablePlugin)
                    throw new NopException("Adumo Online module cannot be loaded");

                //load order by guid
                var order = await _orderService.GetOrderByGuidAsync(orderGuid);

                if (order == null)
                    return RedirectToRoute("ShoppingCart");

                //prepare payment info model
                var model = await _adumoOnlinePaymentModelFactory.PreparePaymentInfoModelAsync(order);

                return View(model);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return RedirectToRoute("ShoppingCart");
            }
        }

        //Executes method on payment success call
        public async Task<IActionResult> SuccessReturn(Guid orderGuid)
        {
            try
            {
                //get order by guid
                var order = await _orderService.GetOrderByGuidAsync(orderGuid);

                if (order != null)
                {
                    //prepare order from adumo response and insert into order notes table
                    await AddOrderNoteAsync(order.Id, Request.Form);

                    //update transaction id in nopcommerce order AuthorizationTransactionId field
                    order.AuthorizationTransactionId = Request.Form["_TRANSACTIONINDEX"];

                    await _orderService.UpdateOrderAsync(order);

                    //mark order as paid
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);

                    //redirect to order completed page
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            }
            return RedirectToRoute("Homepage");
        }

        //executes on failed payment 
        public async Task<IActionResult> FailureReturn(Guid orderGuid)
        {
            try
            {
                //get order by guid
                var order = await _orderService.GetOrderByGuidAsync(orderGuid);

                if (order != null)
                {
                    //prepare order from adumo response and insert into order notes table
                    await AddOrderNoteAsync(order.Id, Request.Form);

                    return RedirectToRoute("OrderDetails", new { orderId = order.Id });
                }
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            }

            return RedirectToRoute("Homepage");
        }

        #endregion
    }
}
