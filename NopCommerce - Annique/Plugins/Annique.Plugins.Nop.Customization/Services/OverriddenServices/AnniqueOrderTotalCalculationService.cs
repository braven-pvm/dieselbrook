using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    public class AnniqueOrderTotalCalculationService : OrderTotalCalculationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IShippingService _shippingService;
        private readonly ShippingSettings _shippingSettings;

        #endregion

        #region Ctor

        public AnniqueOrderTotalCalculationService(CatalogSettings catalogSettings,
            IAddressService addressService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICustomerService customerService,
            IDiscountService discountService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            IProductService productService,
            IRewardPointService rewardPointService,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IAwardService awardService) : base(catalogSettings,
                addressService,
                checkoutAttributeParser,
                customerService,
                discountService,
                genericAttributeService,
                giftCardService,
                orderService,
                paymentService,
                priceCalculationService,
                productService,
                rewardPointService,
                shippingPluginManager,
                shippingService,
                shoppingCartService,
                storeContext,
                taxService,
                workContext,
                rewardPointsSettings,
                shippingSettings,
                shoppingCartSettings,
                taxSettings)
        {
            _customerService = customerService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _shippingService = shippingService;
            _shippingSettings = shippingSettings;
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether shipping is free
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="subTotal">Subtotal amount; pass null to calculate subtotal</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a value indicating whether shipping is free
        /// </returns>
        public override async Task<bool> IsFreeShippingAsync(IList<ShoppingCartItem> cart, decimal? subTotal = null)
        {
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return await base.IsFreeShippingAsync(cart, subTotal);

            //check whether customer is in a customer role with free shipping applied
            var customer = await _customerService.GetCustomerByIdAsync(cart.FirstOrDefault()?.CustomerId ?? 0);

            if (customer != null && (await _customerService.GetCustomerRolesAsync(customer)).Any(role => role.FreeShipping))
                return true;

            //check whether all shopping cart items and their associated products marked as free shipping
            if (await cart.AllAwaitAsync(async shoppingCartItem => await _shippingService.IsFreeShippingAsync(shoppingCartItem)))
                return true;

            //free shipping over $X
            if (!_shippingSettings.FreeShippingOverXEnabled)
                return false;

            #region Task 586 Free Shipping Calculation Based on Total RSP(subtotal total before discount)
           
            if (!subTotal.HasValue)
            {
                var (subTotalWithoutDiscount, _) = await _anniqueCustomizationConfigurationService.GetShoppingCartTotalsBeforeDiscountAsync(cart);
                subTotal = subTotalWithoutDiscount;
            }

            #endregion

            //check whether we have subtotal enough to have free shipping
            if (subTotal.Value > _shippingSettings.FreeShippingOverXValue)
                return true;

            return false;
        }
    }
}
