using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Misc.WebApi.Frontend.Services;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Controllers;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers;

public partial class PaymentInfoWebViewController : BasePublicController
{
    #region Fields

    private readonly IAuthenticationService _authenticationService;
    private readonly ICheckoutModelFactory _checkoutModelFactory;
    private readonly ICheckoutService _checkoutService;
    private readonly ICustomerService _customerService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderService _orderService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IPaymentService _paymentService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IStoreContext _storeContext;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly OrderSettings _orderSettings;
    private readonly PaymentSettings _paymentSettings;

    #endregion

    #region Ctor

    public PaymentInfoWebViewController(IAuthenticationService authenticationService, 
        ICheckoutModelFactory checkoutModelFactory,
        ICheckoutService checkoutService,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IWebHelper webHelper,
        IWorkContext workContext,
        OrderSettings orderSettings,
        PaymentSettings paymentSettings)
    {
        _authenticationService = authenticationService;
        _checkoutModelFactory = checkoutModelFactory;
        _checkoutService = checkoutService;
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _paymentPluginManager = paymentPluginManager;
        _paymentService = paymentService;
        _shoppingCartService = shoppingCartService;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _workContext = workContext;
        _orderSettings = orderSettings;
        _paymentSettings = paymentSettings;
    }

    #endregion

    #region

    public async Task<IActionResult> Redirect(int orderId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        //validation
        if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
            return Challenge();

        //get the order
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null || order.CustomerId != customer.Id)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ShoppingCartPage });

        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
        if (paymentMethod == null)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.PaymentMethod });

        if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ShoppingCartPage });
        
        var postProcessPaymentRequest = new PostProcessPaymentRequest
        {
            Order = order
        };

        await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

        if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
            //redirection or POST has been done in PostProcessPayment
            return Content("Redirected");

        //if no redirection has been done (to a third-party payment page)
        //theoretically it's not possible
        return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
    }

    #endregion

    #region Methods

    public IActionResult NextStep()
    {
        return Content("");
    }

    public async Task<IActionResult> PaymentInfo()
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ShoppingCartPage });

        var orderId = _webHelper.QueryString<int>("orderId");
        if (orderId != 0)
            return await Redirect(orderId);

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ShoppingCartPage });

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ShoppingCartPage });

        //Check whether payment workflow is required
        var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
        if (!isPaymentWorkflowRequired) 
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ConfirmOrder });

        //load payment method
        var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(paymentMethodSystemName);
        if (paymentMethod == null)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.PaymentMethod });

        //Check whether payment info should be skipped
        if (paymentMethod.SkipPaymentInfo ||
            (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
        {
            //skip payment info page
            var paymentInfo = new ProcessPaymentRequest();

            await _checkoutService.SavePaymentInfoAsync(paymentInfo);

            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ConfirmOrder });
        }

        var model = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);

        await _authenticationService.SignInAsync(await _workContext.GetCurrentCustomerAsync(), true);

        return View("~/Plugins/Misc.WebApi.Frontend/Views/PaymentInfo.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PaymentInfo(IFormCollection form)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ShoppingCartPage });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ShoppingCartPage });

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ShoppingCartPage });

        //Check whether payment workflow is required
        var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
        if (!isPaymentWorkflowRequired) 
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ConfirmOrder });

        //load payment method
        var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(paymentMethodSystemName);
        if (paymentMethod == null)
            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.PaymentMethod });

        var warnings = await paymentMethod.ValidatePaymentFormAsync(form);
        foreach (var warning in warnings)
            ModelState.AddModelError("", warning);

        if (ModelState.IsValid)
        {
            //get payment info
            var paymentInfo = await paymentMethod.GetPaymentInfoAsync(form);
            //set previous order GUID (if exists)
            await _checkoutService.GenerateOrderGuidAsync(paymentInfo);

            await _checkoutService.SavePaymentInfoAsync(paymentInfo);

            return RedirectToAction("NextStep", new { step = (int)CheckoutStep.ConfirmOrder });
        }

        //if we got this far, something failed, redisplay form
        var model = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);

        return View("~/Plugins/Misc.WebApi.Frontend/Views/PaymentInfo.cshtml", model);
    }

    #endregion

    #region Nested enum

    public enum CheckoutStep
    {
        ShoppingCartPage,
        BillingAddress,
        ShippingAddress,
        ShippingMethod,
        PaymentMethod,
        PaymentInfo,
        ConfirmOrder,
        Completed
    }

    #endregion
}