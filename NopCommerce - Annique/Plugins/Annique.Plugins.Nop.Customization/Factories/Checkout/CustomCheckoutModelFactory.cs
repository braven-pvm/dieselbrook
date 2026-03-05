using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.Checkout
{
    public class CustomCheckoutModelFactory : CheckoutModelFactory
    {
        #region Ctor
        public CustomCheckoutModelFactory(AddressSettings addressSettings,
            CaptchaSettings captchaSettings, 
            CommonSettings commonSettings, 
            IAddressModelFactory addressModelFactory, 
            IAddressService addressService, 
            ICountryService countryService, 
            ICurrencyService currencyService, 
            ICustomerService customerService, 
            IGenericAttributeService genericAttributeService, 
            ILocalizationService localizationService, 
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService, 
            IPaymentPluginManager paymentPluginManager, 
            IPaymentService paymentService, 
            IPickupPluginManager pickupPluginManager, 
            IPriceFormatter priceFormatter, 
            IRewardPointService rewardPointService, 
            IShippingPluginManager shippingPluginManager, 
            IShippingService shippingService, 
            IShoppingCartService shoppingCartService, 
            IStateProvinceService stateProvinceService, 
            IStoreContext storeContext, 
            IStoreMappingService storeMappingService, 
            ITaxService taxService, 
            IWorkContext workContext, 
            OrderSettings orderSettings, 
            PaymentSettings paymentSettings, 
            RewardPointsSettings rewardPointsSettings, 
            ShippingSettings shippingSettings, 
            TaxSettings taxSettings) : base(addressSettings, 
                captchaSettings, 
                commonSettings, 
                addressModelFactory, 
                addressService, 
                countryService, 
                currencyService, 
                customerService, 
                genericAttributeService, 
                localizationService, 
                orderProcessingService, 
                orderTotalCalculationService, 
                paymentPluginManager, 
                paymentService, 
                pickupPluginManager, 
                priceFormatter, 
                rewardPointService, 
                shippingPluginManager, 
                shippingService, 
                shoppingCartService, 
                stateProvinceService, 
                storeContext, 
                storeMappingService, 
                taxService,
                workContext, 
                orderSettings, 
                paymentSettings, 
                rewardPointsSettings, 
                shippingSettings, 
                taxSettings)
        {
        }

        #endregion

        #region Method
        public override async Task<CheckoutShippingAddressModel> PrepareShippingAddressModelAsync(
            IList<ShoppingCartItem> cart,
            int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            return await base.PrepareShippingAddressModelAsync(cart); // Do not Set prePopulateNewAddressWithCustomerFields so it take bydefault false to prevent auto-population
        }

        #endregion
    }
}
