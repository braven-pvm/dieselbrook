using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Themes;
using Nop.Services.Topics;
using Nop.Web.Factories;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.UI;
using Nop.Web.Models.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.OverriddenFactory
{
    public class OverriddenCommonModelFactory : CommonModelFactory
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ForumSettings _forumSettings;
        private readonly ICustomerService _customerService;
        private readonly IForumService _forumService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public OverriddenCommonModelFactory(BlogSettings blogSettings,
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            CustomerSettings customerSettings,
            DisplayDefaultFooterItemSettings displayDefaultFooterItemSettings,
            ForumSettings forumSettings,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IForumService forumService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            INopHtmlHelper nopHtmlHelper,
            IPermissionService permissionService,
            IPictureService pictureService,
            IShoppingCartService shoppingCartService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IThemeProvider themeProvider,
            ITopicService topicService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            NewsSettings newsSettings,
            RobotsTxtSettings robotsTxtSettings,
            SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings,
            StoreInformationSettings storeInformationSettings,
            VendorSettings vendorSettings,
            ISettingService settingService, IRepository<Order> orderRepository) : base(blogSettings,

                captchaSettings,
                catalogSettings,
                commonSettings,
                customerSettings,
                displayDefaultFooterItemSettings,
                forumSettings,
                currencyService,
                customerService,
                forumService,
                genericAttributeService,
                httpContextAccessor,
                languageService,
                localizationService,
                fileProvider,
                nopHtmlHelper,
                permissionService,
                pictureService,
                shoppingCartService,
                staticCacheManager,
                storeContext,
                themeContext,
                themeProvider,
                topicService,
                urlRecordService,
                webHelper,
                workContext,
                localizationSettings,
                mediaSettings,
                newsSettings,
                robotsTxtSettings,
                sitemapSettings,
                sitemapXmlSettings,
                storeInformationSettings,
                vendorSettings)
        {
            _customerSettings = customerSettings;
            _forumSettings = forumSettings;
            _customerService = customerService;
            _forumService = forumService;
            _genericAttributeService = genericAttributeService;
            _permissionService = permissionService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _settingService = settingService;
            _orderRepository = orderRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Method

        public async Task<bool> HasPendingOrdersAsync(int customerId)
        {
            if (customerId <= 0)
                return false;

            // Define cache key
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                AnniqueCustomizationDefaults.CustomerHasPendingOrdersCacheKey, customerId);

            // Return from cache 
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = _orderRepository.Table
                    .Where(o => o.CustomerId == customerId &&
                                o.OrderStatusId == (int)OrderStatus.Pending &&
                                !o.Deleted);

                return await query.AnyAsync();
            });
        }

        /// <summary>
        /// Prepare the header links model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the header links model
        /// </returns>
        public override async Task<HeaderLinksModel> PrepareHeaderLinksModelAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //if settings null or annique plugin not enable then call base factory method
            if (settings == null || !settings.IsEnablePlugin)
                return await base.PrepareHeaderLinksModelAsync();

            int fromCustomerId = settings.AdminCustomerId;

            var privateMessages = await _forumService.GetAllPrivateMessagesAsync(store.Id,
                fromCustomerId, customer.Id, null, false, false, string.Empty);

            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;

            if (privateMessages.Count > 0)
            {
                // Filter private messages based on subject containing '[' and ']' or private message is unread
                var filteredPrivateMessages = privateMessages.Where(pm => (pm.Subject.Contains("[") && pm.Subject.Contains("]")) || !pm.IsRead);

                // Create a single string containing all the private message texts, separated by br tag
                var privateMessagesText = string.Join("<br>", filteredPrivateMessages.Select(pm => pm.Text));

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
                    !await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.NotifiedAboutNewPrivateMessagesAttribute, store.Id))
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.NotifiedAboutNewPrivateMessagesAttribute, true, store.Id);
                    alertMessage = privateMessagesText;
                }
            }

            var model = new HeaderLinksModel
            {
                RegistrationType = _customerSettings.UserRegistrationType,
                IsAuthenticated = await _customerService.IsRegisteredAsync(customer),
                CustomerName = await _customerService.IsRegisteredAsync(customer) ? await _customerService.FormatUsernameAsync(customer) : string.Empty,
                ShoppingCartEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart),
                WishlistEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist),
                AllowPrivateMessages = await _customerService.IsRegisteredAsync(customer) && _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage,
            };

            #region task 626 pending orders

            //check for customer has any pending orders if yes then add custom property 
            var hasPendingOrders = await HasPendingOrdersAsync(customer.Id);
            if (hasPendingOrders)
            {
                model.CustomProperties.Clear();
                model.CustomProperties["PendingOrders"] = hasPendingOrders.ToString();
            }

            #endregion
            //performance optimization (use "HasShoppingCartItems" property)
            if (customer.HasShoppingCartItems)
            {
                model.ShoppingCartItems = (await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id))
                    .Sum(item => item.Quantity);

                model.WishlistItems = (await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id))
                    .Sum(item => item.Quantity);
            }

            return model;
        }

        #endregion
    }
}
