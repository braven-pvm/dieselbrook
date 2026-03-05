using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    public class AnniqueShoppingCartService : ShoppingCartService
    {
        #region Fields

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IAwardService _awardService;
        private readonly IDiscountCustomerMappingService _discountCustomerMappingService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public AnniqueShoppingCartService(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IAwardService awardService,
            CatalogSettings catalogSettings,
            IAclService aclService,
            IActionContextAccessor actionContextAccessor,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IRepository<ShoppingCartItem> sciRepository,
            IShippingService shippingService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings,
            IDiscountCustomerMappingService discountCustomerMappingService,
            ISettingService settingService) : base(
                catalogSettings,
                aclService,
                actionContextAccessor,
                checkoutAttributeParser,
                checkoutAttributeService,
                currencyService,
                customerService,
                dateRangeService,
                dateTimeHelper,
                genericAttributeService,
                localizationService,
                permissionService,
                priceCalculationService,
                priceFormatter,
                productAttributeParser,
                productAttributeService,
                productService,
                sciRepository,
                shippingService,
                staticCacheManager,
                storeContext,
                storeService,
                storeMappingService,
                urlHelperFactory,
                urlRecordService,
                workContext,
                orderSettings,
                shoppingCartSettings)
        {
            _awardService = awardService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _productAttributeParser = productAttributeParser;
            _discountCustomerMappingService = discountCustomerMappingService;
            _storeContext = storeContext;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _productService = productService;
            _settingService = settingService;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Utilities



        /// <summary>
        /// Determine if the shopping cart item is the same as the one being compared
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">Price entered by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shopping cart item is equal
        /// </returns>
        protected override async Task<bool> ShoppingCartItemIsEqualAsync(ShoppingCartItem shoppingCartItem,
            Product product,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate)
        {
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return await base.ShoppingCartItemIsEqualAsync(shoppingCartItem, product, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate);

            if (shoppingCartItem.ProductId != product.Id)
                return false;

            var attributesEqual = await _awardService.AreProductAttributesEqualAsync(shoppingCartItem.AttributesXml, attributesXml, false, false);

            if (!attributesEqual)
                return false;

            //gift cards
            if (product.IsGiftCard)
            {
                _productAttributeParser.GetGiftCardAttribute(attributesXml, out var giftCardRecipientName1, out var _, out var giftCardSenderName1, out var _, out var _);

                _productAttributeParser.GetGiftCardAttribute(shoppingCartItem.AttributesXml, out var giftCardRecipientName2, out var _, out var giftCardSenderName2, out var _, out var _);

                var giftCardsAreEqual = giftCardRecipientName1.Equals(giftCardRecipientName2, StringComparison.InvariantCultureIgnoreCase)
                    && giftCardSenderName1.Equals(giftCardSenderName2, StringComparison.InvariantCultureIgnoreCase);
                if (!giftCardsAreEqual)
                    return false;
            }

            //price is the same (for products which require customers to enter a price)
            if (product.CustomerEntersPrice)
            {
                //we use rounding to eliminate errors associated with storing real numbers in memory when comparing
                var customerEnteredPricesEqual = Math.Round(shoppingCartItem.CustomerEnteredPrice, 2) == Math.Round(customerEnteredPrice, 2);
                if (!customerEnteredPricesEqual)
                    return false;
            }

            if (!product.IsRental)
                return true;

            //rental products
            var rentalInfoEqual = shoppingCartItem.RentalStartDateUtc == rentalStartDate && shoppingCartItem.RentalEndDateUtc == rentalEndDate;

            return rentalInfoEqual;
        }

        #endregion

        #region Method

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Product attributes (XML format)</param>
        /// <param name="customerEnteredPrice">Customer entered price (if specified)</param>
        /// <param name="rentalStartDate">Rental start date (null for not rental products)</param>
        /// <param name="rentalEndDate">Rental end date (null for not rental products)</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shopping cart unit price (one item). Applied discount amount. Applied discounts
        /// </returns>
        public override async Task<(decimal unitPrice, decimal discountAmount, List<Discount> appliedDiscounts)> GetUnitPriceAsync(Product product,
            Customer customer,
            Store store,
            ShoppingCartType shoppingCartType,
            int quantity,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate, DateTime? rentalEndDate,
            bool includeDiscounts)
        {
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return await base.GetUnitPriceAsync(product,customer,store,shoppingCartType,quantity,attributesXml,customerEnteredPrice,rentalStartDate,rentalEndDate,includeDiscounts);

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<Discount>();

            decimal finalPrice;

            if (!string.IsNullOrEmpty(attributesXml) && _awardService.ContainsAwardProductAttribute(attributesXml))
                return (decimal.Zero, discountAmount, appliedDiscounts);

            var _priceCalculationService = EngineContext.Current.Resolve<IPriceCalculationService>();

            var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
            if (combination?.OverriddenPrice.HasValue ?? false)
            {
                (_, finalPrice, discountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product,
                        customer,
                        store,
                        combination.OverriddenPrice.Value,
                        decimal.Zero,
                        includeDiscounts,
                        quantity,
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null);
            }
            else
            {
                //summarize price of all attributes
                var attributesTotalPrice = decimal.Zero;
                var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml);
                if (attributeValues != null)
                {
                    foreach (var attributeValue in attributeValues)
                    {
                        attributesTotalPrice += await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product,
                            attributeValue,
                            customer,
                            store,
                            product.CustomerEntersPrice ? (decimal?)customerEnteredPrice : null,
                            quantity);
                    }
                }

                //get price of a product (with previously calculated price of all attributes)
                if (product.CustomerEntersPrice)
                {
                    finalPrice = customerEnteredPrice;
                }
                else
                {
                    int qty;
                    if (_shoppingCartSettings.GroupTierPricesForDistinctShoppingCartItems)
                    {
                        //the same products with distinct product attributes could be stored as distinct "ShoppingCartItem" records
                        //so let's find how many of the current products are in the cart                        
                        qty = (await GetShoppingCartAsync(customer, shoppingCartType: shoppingCartType, productId: product.Id))
                            .Sum(x => x.Quantity);

                        if (qty == 0)
                        {
                            qty = quantity;
                        }
                    }
                    else
                    {
                        qty = quantity;
                    }

                    (_, finalPrice, discountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product,
                        customer,
                        store,
                        attributesTotalPrice,
                        includeDiscounts,
                        qty,
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null);


                    var _specialOffersService = EngineContext.Current.Resolve<ISpecialOffersService>();
                    if (!string.IsNullOrEmpty(attributesXml) && _specialOffersService.ContainsSpecialOfferAttribute(attributesXml))
                    {
                        //if special offer product then calculate special discount first
                        //then calculate other discounts on discounted unit price
                        (var finalSpecialPrice, var specialDiscountAmount, var specialDiscounts) = await _specialOffersService.ApplySpecialAndStandardDiscountsAsync(product,
                attributesXml, product.Price, appliedDiscounts);

                        finalPrice = finalSpecialPrice;
                        discountAmount = specialDiscountAmount;
                        appliedDiscounts.Clear();
                        appliedDiscounts.AddRange(specialDiscounts);
                    }
                }
            }

            //rounding
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                finalPrice = await _priceCalculationService.RoundPriceAsync(finalPrice);

            return (finalPrice, discountAmount, appliedDiscounts);
        }

        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart item sub total. Applied discount amount. Applied discounts. Maximum discounted qty. Return not nullable value if discount cannot be applied to ALL items</returns>
        public override async Task<(decimal subTotal, decimal discountAmount, List<Discount> appliedDiscounts, int? maximumDiscountQty)> GetSubTotalAsync(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return await base.GetSubTotalAsync(shoppingCartItem, includeDiscounts);

            decimal subTotal = decimal.Zero;
            int? maximumDiscountQty = null;

            //unit price
            var (unitPrice, discountAmount, appliedDiscounts) = await GetUnitPriceAsync(shoppingCartItem, includeDiscounts);

            //discount
            if (appliedDiscounts.Any())
            {
                //get customer discount mappings
                var discountCustomerMappings = await _discountCustomerMappingService.GetAllDiscountCustomerMappingsAsync(shoppingCartItem.CustomerId);
                Discount specialDiscount = null;

                if (discountCustomerMappings.Any())
                {
                    // Find the special discount from discountCustomerMappings
                    var specialDiscountIds = discountCustomerMappings.Select(dc => dc.DiscountId).ToList();
                    //get only product level discounts 
                    specialDiscount = appliedDiscounts.FirstOrDefault(d => specialDiscountIds.Contains(d.Id) && (d.DiscountType == DiscountType.AssignedToSkus || d.DiscountType == DiscountType.AssignedToCategories));
                }

                var productPrice = (await GetUnitPriceAsync(shoppingCartItem, false)).unitPrice;
                if (appliedDiscounts.Count > 1 && specialDiscount != null)
                {
                    //product total RSP(price without discount)
                    var TotalBeforeDiscount = productPrice * shoppingCartItem.Quantity;
                    var commonDiscountAmount = discountAmount - specialDiscount.DiscountAmount;
                    var commonDiscountAmountSubTotal = commonDiscountAmount * shoppingCartItem.Quantity;

                    // Special discount applied to some items, common discount applied to others
                    if (specialDiscount.MaximumDiscountedQuantity.HasValue && shoppingCartItem.Quantity > specialDiscount.MaximumDiscountedQuantity.Value)
                    {
                        // Special discount applied to limited items
                        maximumDiscountQty = specialDiscount.MaximumDiscountedQuantity.Value;

                        var discountedQuantity = specialDiscount.MaximumDiscountedQuantity.Value;
                        var notDiscountedQuantity = shoppingCartItem.Quantity - discountedQuantity;

                        var discountedSubTotal = specialDiscount.DiscountAmount * discountedQuantity;
                        var notDiscountedSubTotal = specialDiscount.DiscountAmount * notDiscountedQuantity;

                        discountAmount = commonDiscountAmountSubTotal + discountedSubTotal;
                        subTotal = TotalBeforeDiscount - discountAmount;
                    }
                    else
                    {
                        // discount amount applied to all items
                        subTotal = (productPrice - discountAmount) * shoppingCartItem.Quantity;
                    }
                }
                else if (appliedDiscounts.Count == 1 && specialDiscount != null)
                {
                    //we can properly use "MaximumDiscountedQuantity" property only for one discount (not cumulative ones)
                    Discount oneAndOnlyDiscount = null;
                    oneAndOnlyDiscount = appliedDiscounts.First();

                    if ((oneAndOnlyDiscount?.MaximumDiscountedQuantity.HasValue ?? false) &&
                        shoppingCartItem.Quantity > oneAndOnlyDiscount.MaximumDiscountedQuantity.Value)
                    {
                        #region 638 Customer / Client Free product Voucher

                        maximumDiscountQty = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;

                        var discountedQuantity = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                        var notDiscountedQuantity = shoppingCartItem.Quantity - discountedQuantity;

                        // Calculate subtotal for discounted quantity
                        // Example: 100% discount → price - discountAmount = 0 for discountedQuantity
                        var discountedSubTotal = (productPrice - oneAndOnlyDiscount.DiscountAmount) * discountedQuantity;

                        // Calculate subtotal for remaining quantity without any discount
                        var notDiscountedSubTotal = productPrice * notDiscountedQuantity;

                        // Final subtotal = discounted items subtotal + non-discounted items subtotal
                        subTotal = discountedSubTotal + notDiscountedSubTotal;

                        // Total discount amount = per-unit discount × number of discounted units
                        discountAmount = oneAndOnlyDiscount.DiscountAmount * discountedQuantity;

                        #endregion
                    }
                    else
                    {
                        #region 638 Customer / Client Free product Voucher

                        // If the discount applies to ALL quantities:
                        // - Special display logic: we do not want to show "0" as the unit price even for a 100% discount,
                        //   so the displayed unit price will be the product's actual price.
                        // - For subtotal calculation purposes:
                        //     * If the unit price equals the discount amount (meaning 100% discount), use 0 as the actual unit price.
                        //     * Otherwise, use the actual unit price after subtracting the discount.
                        if (unitPrice == discountAmount)
                            unitPrice = decimal.Zero;
                        else
                            unitPrice =  productPrice - discountAmount;

                        #endregion
                        //discount is applied to all items (quantity)
                        //calculate discount amount for all items
                        discountAmount *= shoppingCartItem.Quantity;

                        subTotal = unitPrice * shoppingCartItem.Quantity;
                    }
                }
                else
                {
                    //discount is applied to all items (quantity)
                    //calculate discount amount for all items
                    discountAmount *= shoppingCartItem.Quantity;

                    subTotal = unitPrice * shoppingCartItem.Quantity;
                }
            }
            else
            {
                subTotal = unitPrice * shoppingCartItem.Quantity;
            }

            return (subTotal, discountAmount, appliedDiscounts, maximumDiscountQty);
        }

        /// <summary>
        /// Migrate shopping cart
        /// </summary>
        /// <param name="fromCustomer">From customer</param>
        /// <param name="toCustomer">To customer</param>
        /// <param name="includeCouponCodes">A value indicating whether to coupon codes (discount and gift card) should be also re-applied</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task MigrateShoppingCartAsync(Customer fromCustomer, Customer toCustomer, bool includeCouponCodes)
        {
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                await base.MigrateShoppingCartAsync(fromCustomer, toCustomer,includeCouponCodes);

            if (fromCustomer == null)
                throw new ArgumentNullException(nameof(fromCustomer));
            if (toCustomer == null)
                throw new ArgumentNullException(nameof(toCustomer));

            if (fromCustomer.Id == toCustomer.Id)
                return; //the same customer

            //get active store
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(toCustomer);

            //check customer contains Consultant role 
            if (customerRoleIds.Contains(settings.ConsultantRoleId))
                return;

            //shopping cart items
            var fromCart = await GetShoppingCartAsync(fromCustomer);

            for (var i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                await AddToCartAsync(toCustomer, product, sci.ShoppingCartType, sci.StoreId,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false);
            }

            for (var i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                await DeleteShoppingCartItemAsync(sci);
            }

            //copy discount and gift card coupon codes
            if (includeCouponCodes)
            {
                //discount
                foreach (var code in await _customerService.ParseAppliedDiscountCouponCodesAsync(fromCustomer))
                    await _customerService.ApplyDiscountCouponCodeAsync(toCustomer, code);

                //gift card
                foreach (var code in await _customerService.ParseAppliedGiftCardCouponCodesAsync(fromCustomer))
                    await _customerService.ApplyGiftCardCouponCodeAsync(toCustomer, code);

                //save customer
                await _customerService.UpdateCustomerAsync(toCustomer);
            }

            //move selected checkout attributes
            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(fromCustomer, NopCustomerDefaults.CheckoutAttributes, store.Id);
            await _genericAttributeService.SaveAttributeAsync(toCustomer, NopCustomerDefaults.CheckoutAttributes, checkoutAttributesXml, store.Id);
        }


        #endregion
    }
}
