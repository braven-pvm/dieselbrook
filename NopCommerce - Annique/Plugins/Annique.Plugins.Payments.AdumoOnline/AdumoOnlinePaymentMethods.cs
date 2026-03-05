using Annique.Plugins.Payments.AdumoOnline.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Annique.Plugins.Payments.AdumoOnline
{
    public class AdumoOnlinePaymentMethods : BasePlugin, IAdminMenuPlugin, IPaymentMethod
    {
        #region Fields

        private readonly IRepository<Language> _languageRepository;
        private readonly INopFileProvider _nopFileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly WidgetSettings _widgetSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly AdumoOnlineSettings _adumoOnlineSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PaymentSettings _paymentSettings;

        #endregion

        #region Ctor

        public AdumoOnlinePaymentMethods(IRepository<Language> languageRepository,
            INopFileProvider nopFileProvider,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            WidgetSettings widgetSettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            AdumoOnlineSettings adumoOnlineSettings,
            IHttpContextAccessor httpContextAccessor,
            PaymentSettings paymentSettings)
        {
            _languageRepository = languageRepository;
            _nopFileProvider = nopFileProvider;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _widgetSettings = widgetSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _adumoOnlineSettings = adumoOnlineSettings;
            _httpContextAccessor = httpContextAccessor;
            _paymentSettings = paymentSettings;
        }

        #endregion

        #region Admin Side Menu 

        /// <summary>
        ///Admin side menu
        /// </summary>
        public async Task ManageSiteMapAsync(SiteMapNode siteMapNode)
        {
            var storeUrl = _webHelper.GetStoreLocation();
            // Add Menu to Plugin Menu
            var mainMenuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Annique.Plugins.Payments.AdumoOnline.Menu.MainTitle"),
                Visible = true,
                IconClass = "nav-icon fab fa-buysellads fa-lg"
            };

            // Add Configure Menu
            var Configure = new SiteMapNode()
            {
                SystemName = "Annique.Plugins.Payments.AdumoOnline.Configure",
                Title = await _localizationService.GetResourceAsync("Annique.Plugins.Payments.AdumoOnline.Configure.Tab"),
                ControllerName = "AdumoOnlineConfiguration",
                ActionName = "Configure",
                IconClass = "fa fa-genderless",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "Annique.Plugins.Payments.AdumoOnline", null } }
            };
            mainMenuItem.ChildNodes.Add(Configure);

            var title = await _localizationService.GetResourceAsync("Annique.Plugins.Payments.AdumoOnline.Menu.MainTitle");
            var targetMenu = siteMapNode.ChildNodes.FirstOrDefault(x => x.Title == title);
            if (targetMenu != null)
            {
                targetMenu.ChildNodes.Add(Configure);
            }
            else
                siteMapNode.ChildNodes.Add(mainMenuItem);
        }

        #endregion

        #region Resource string install/uninstall

        /// <summary>
        ///Import Resource string from xml and save
        /// </summary>
        protected virtual async Task InstallLocaleResources()
        {
            //'English' language
            var languages = _languageRepository.Table.Where(l => l.Published).ToList();
            foreach (var language in languages)
            {
                //save resources
                foreach (var filePath in Directory.EnumerateFiles(_nopFileProvider.MapPath("~/Plugins/Annique.AdumoOnline/Localization/ResourceString"),
                 "resourceString.nopres.xml", SearchOption.TopDirectoryOnly))
                {
                    var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                    using (var streamReader = new StreamReader(filePath))
                    {
                        await localizationService.ImportResourcesFromXmlAsync(language, streamReader);
                    }
                }
            }
        }

        ///<summry>
        ///Delete Resource String
        ///</summry>
        protected virtual async Task DeleteLocalResources()
        {
            var file = Path.Combine(_nopFileProvider.MapPath("~/Plugins/Annique.AdumoOnline/Localization/ResourceString"), "resourceString.nopres.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                await _localizationService.DeleteLocaleResourcesAsync(item);
            }
        }

        #endregion

        #region Install Uninstall Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/AdumoOnlineConfiguration/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //install local resource strings
            await InstallLocaleResources();

            var settings = new AdumoOnlineSettings
            {
                IsEnablePlugin = true
            };

            //save settings
            await _settingService.SaveSettingAsync(settings);

            //enable payment method
            if (!_paymentSettings.ActivePaymentMethodSystemNames.Contains("Annique.AdumoOnline"))
            {
                _paymentSettings.ActivePaymentMethodSystemNames.Add("Annique.AdumoOnline");
                await _settingService.SaveSettingAsync(_paymentSettings);
            }

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            await DeleteLocalResources();

            //settings
            if (_paymentSettings.ActivePaymentMethodSystemNames.Contains("Annique.AdumoOnline"))
            {
                _paymentSettings.ActivePaymentMethodSystemNames.Remove("Annique.AdumoOnline");
                await _settingService.SaveSettingAsync(_paymentSettings);
            }

            await _settingService.DeleteSettingAsync<AdumoOnlineSettings>();

            await base.UninstallAsync();
        }

        #endregion

        #region Payment methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //get order 
            var orderGuid = postProcessPaymentRequest.Order.OrderGuid;

            //prepare link to redirect to payment adumo online controller's CheckoutView method
            string scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            string host = _httpContextAccessor.HttpContext.Request.Host.ToString();
            var absoluteUri = string.Concat(scheme, "://", host, "/PaymentAdumoOnline/CheckoutView/" + orderGuid);
            _httpContextAccessor.HttpContext.Response.Redirect(absoluteUri);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue - hide; false - display.
        /// </returns>
        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the additional handling fee
        /// </returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _adumoOnlineSettings.AdditionalFee, _adumoOnlineSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the capture payment result
        /// </returns>
        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(true);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of validating errors
        /// </returns>
        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment info holder
        /// </returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public Type GetPublicViewComponent()
        {
            return typeof(PaymentInfoViewComponent);
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Annique.Plugins.Payments.AdumoOnline.PaymentMethodDescription");
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;

        #endregion
    }
}
