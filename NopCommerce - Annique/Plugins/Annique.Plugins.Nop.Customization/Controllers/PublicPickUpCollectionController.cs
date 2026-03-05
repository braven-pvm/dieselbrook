using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Factories.PickUpCollection;
using Annique.Plugins.Nop.Customization.Models.PickUpCollection;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.PickUpCollection;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using LinqToDB.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class PublicPickUpCollectionController : BasePublicController
    {
        #region Fields

        private readonly IPickUpCollectionModelFactory _pickUpCollectionModelFactory;
        private readonly INopDataProvider _nopDataProvider;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly OrderSettings _orderSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPickUpCollectionService _pickUpCollectionService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly AddressSettings _addressSettings;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;

        #endregion

        #region Ctor

        public PublicPickUpCollectionController(IPickUpCollectionModelFactory pickUpCollectionModelFactory,
            INopDataProvider nopDataProvider,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICheckoutModelFactory checkoutModelFactory,
            IShoppingCartService shoppingCartService,
            IAddressAttributeService addressAttributeService,
            ILogger logger,
            ILocalizationService localizationService,
            OrderSettings orderSettings,
            IGenericAttributeService genericAttributeService,
            ISettingService settingService,
            ICountryService countryService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPickUpCollectionService pickUpCollectionService,
            IAddressModelFactory addressModelFactory,
            AddressSettings addressSettings,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService)
        {
            _pickUpCollectionModelFactory = pickUpCollectionModelFactory;
            _nopDataProvider = nopDataProvider;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _checkoutModelFactory = checkoutModelFactory;
            _shoppingCartService = shoppingCartService;
            _addressAttributeService = addressAttributeService;
            _logger = logger;
            _localizationService = localizationService;
            _orderSettings = orderSettings;
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
            _countryService = countryService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _pickUpCollectionService = pickUpCollectionService;
            _addressModelFactory = addressModelFactory;
            _addressSettings = addressSettings;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
        }

        #endregion

        #region Utilities


        /// <summary>
        /// Parses the value indicating whether the "pickup in store" is allowed
        /// </summary>
        /// <param name="form">The form</param>
        /// <returns>The value indicating whether the "pickup in store" is allowed</returns>
        protected virtual bool ParsePickupInStore(IFormCollection form)
        {
            var pickupInStore = false;

            var pickupInStoreParameter = form["PickupInStore"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(pickupInStoreParameter))
                _ = bool.TryParse(pickupInStoreParameter, out pickupInStore);

            return pickupInStore;
        }

        /// <summary>
        /// Parses the pickup option
        /// </summary>
        /// <param name="filterPickupStore">Shopping Cart</param>
        /// <param name="form">The form</param>
        /// <returns>
        /// The task result contains the pickup option
        /// </returns>
        protected async Task<PickupPoint> ParsePickupOptionAsync(FilterStorePickupPoint filterStorePickupPoint, Address address)
        {
            var selectedPoint = new PickupPoint
            {
                Id = filterStorePickupPoint.Id.ToString(),
                Name = filterStorePickupPoint.Name,
                Description = filterStorePickupPoint.Description,
                PickupFee = filterStorePickupPoint.PickupFee,
                ProviderSystemName = $"PickupPoints.PickupInStore",
                TransitDays = filterStorePickupPoint.TransitDays,
                Address = address.Address1,
                City = address.City,
                ZipPostalCode = address.ZipPostalCode,
                County = address.County
            };

            #region #583 Pickup Point - No Charge if above Free shipping limit

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var amount = await _orderTotalCalculationService.IsFreeShippingAsync(cart) ? 0 : selectedPoint.PickupFee;
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();

            if (amount > 0)
            {
                (amount, _) = await _taxService.GetShippingPriceAsync(amount, customer);
                amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount, currentCurrency);
                selectedPoint.PickupFee = amount;
            }

            //adjust rate
            var (shippingTotal, _) = await _orderTotalCalculationService.AdjustShippingRateAsync(selectedPoint.PickupFee, cart, true);
            var (rateBase, _) = await _taxService.GetShippingPriceAsync(shippingTotal, customer);
            var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase, currentCurrency);
            selectedPoint.PickupFee = rate; 
            
            #endregion

            return selectedPoint;
        }

        /// <summary>
        /// Saves the pickup option
        /// </summary>
        /// <param name="pickupPoint">The pickup option</param>
        protected virtual async Task SavePickupOptionAsync(PickupPoint pickupPoint)
        {
            var name = !string.IsNullOrEmpty(pickupPoint.Name) ?
                string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"), pickupPoint.Name) :
                await _localizationService.GetResourceAsync("Checkout.PickupPoints.NullName");
            var pickUpInStoreShippingOption = new ShippingOption
            {
                Name = name,
                Rate = pickupPoint.PickupFee,
                Description = pickupPoint.Description,
                ShippingRateComputationMethodSystemName = pickupPoint.ProviderSystemName,
                IsPickupInStore = true
            };

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, store.Id);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint, store.Id);
        }

        protected async Task<JsonResult> HandleCheckoutProcessAsync(Customer customer, IList<ShoppingCartItem> cart, Address address, bool onePageCheckout, FilterStorePickupPoint pickupPoint)
        {
            var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true);

            if (onePageCheckout)
            {
                return Json(new
                {
                    success = true,
                    selected_id = (int)customer.ShippingAddressId,
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "shipping",
                        html = await RenderPartialViewToStringAsync("~/Plugins/Annique.Customization/Themes/Avenue/Views/Checkout/OpcShippingAddress.cshtml", shippingAddressModel)
                    }
                });
            }

            var pickUpOptions = await ParsePickupOptionAsync(pickupPoint, address);
            await SavePickupOptionAsync(pickUpOptions);

            return Json(new
            {
                success = true,
                redirect = Url.RouteUrl("CheckoutPaymentMethod")
            });
        }

        #endregion

        #region Methods

        //Get Pick Up collection after Customer add postal code from PostNetStoreDelivery pop up
        [HttpPost]
        public async Task<IActionResult> GetPickUpCollection(PostNetStoreDeliveryModel model)
        {
            //get filtered pick up points
            var filterPickUpcollection = await _pickUpCollectionModelFactory.PrepareFilterPickUpStoreModelAsync(model);

            //If get filter pick up collections 
            if (filterPickUpcollection.PickupPoints.Count > 0)
            {
                var pickUpStoreJson = JsonConvert.SerializeObject(filterPickUpcollection.PickupPoints);
                return Json(new
                {
                    success = true,
                    pickUpStores = pickUpStoreJson
                });
            }

            //If no filter pick up collections available
            return Json(new
            {
                success = false,
                message = await _localizationService.GetResourceAsync("Annique.Plugin.PostNetStoreDelivery.PostalCodeAddress.NotFound")
            });
        }

        //Save Pick up store As new Customer Address
        [HttpPost]
        public async Task<IActionResult> SavePickUpStore(int pickUpStoreId, string firstName, string LastName, string cell)
        {
            try
            {
                //load settings
                var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(_storeContext.GetCurrentStore().Id);

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();

                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                var onePageCheckout = _orderSettings.OnePageCheckoutEnabled;

                //Get selected Pick up store using store procedure
                var pickUpStore = await _pickUpCollectionService.GetPickUpStoreByIdAsync(pickUpStoreId);

                //Get address of pickup store
                var newAddress = await _addressService.GetAddressByIdAsync(pickUpStore.AddressId);

                //update first name, lastname and phone number
                _pickUpCollectionService.UpdateAddressWithFormData(newAddress, firstName, LastName, cell);

                var countryName = await _countryService.GetCountryByAddressAsync(newAddress) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;

                //add pickup point code value to custom attribute and parse xml
                var customAttributes = await _pickUpCollectionService.GetCustomAttributesAsync(newAddress, pickUpStoreId);
                
                //try to find an address with the same values (don't duplicate records)
                var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                    newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                    customer.Email, newAddress.FaxNumber, newAddress.Company,
                    newAddress.Address1, newAddress.Address2, newAddress.City,
                    countryName, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                    newAddress.CountryId, customAttributes);

                if (address == null)
                {
                    address = _pickUpCollectionService.PrepareAddressFields(newAddress,customAttributes,customer.Email,countryName);

                    //check address is valid or not
                    if (await _addressService.IsAddressValidAsync(address))
                    {
                        //if valid add new address
                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(customer, address);

                        customer.ShippingAddressId = address.Id;

                        await _customerService.UpdateCustomerAsync(customer);
                    }
                    else
                    {
                        if (onePageCheckout)
                        {
                            //Display Error message
                            return Json(new
                            {
                                success = false,
                                message = await _localizationService.GetResourceAsync("Annique.Plugin.PostNetStoreDelivery.InvalidAddress"),
                                goto_section = "shipping"
                            });
                        }

                        //Display Error message
                        return Json(new
                        {
                            message = await _localizationService.GetResourceAsync("Annique.Plugin.PostNetStoreDelivery.InvalidAddress")
                        });
                    }
                }
                else
                {
                    if (onePageCheckout)
                    {
                        //Display Error message
                        return Json(new
                        {
                            success = false,
                            message = await _localizationService.GetResourceAsync("Annique.Plugin.PostNetStoreDelivery.AlredyExist")
                        });
                    }

                    //Display Error message
                    return Json(new
                    {
                        message = await _localizationService.GetResourceAsync("Annique.Plugin.PostNetStoreDelivery.AlreadyExist")
                    });
                }

                return await HandleCheckoutProcessAsync(customer, cart, address, onePageCheckout,pickUpStore);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> SelectShippingAddress(int addressId)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);

            if (address == null)
                return RedirectToRoute("CheckoutShippingAddress");

            //update shipping address id
            customer.ShippingAddressId = address.Id;
            await _customerService.UpdateCustomerAsync(customer);

            //load settings
            var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(_storeContext.GetCurrentStore().Id);

            //check it is normal shipping address or PEP address
            var pickUpStoreAddressAttribute = await _addressAttributeService.GetAddressAttributeByIdAsync(anniqueSettings.PickupCustomAttributeId);
            var pickUpStoreAddressAttributeValue = _addressAttributeParser.ParseValues(address.CustomAttributes, pickUpStoreAddressAttribute.Id).FirstOrDefault();

            //If get PEP store id from attribute value
            if (pickUpStoreAddressAttributeValue != null)
            {
                //Get selected Pick up store using store procedure
                var pickUpStore = (await _nopDataProvider.QueryProcAsync<FilterStorePickupPoint>("sp_GetPickUpStoreById", new DataParameter { Name = "id", Value = Convert.ToInt32(pickUpStoreAddressAttributeValue) })).FirstOrDefault();
                //Prepare and save pickup options
                var pickUpOptions = await ParsePickupOptionAsync(pickUpStore, address);
                await SavePickupOptionAsync(pickUpOptions);

                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //get customer generatic attributes
            var customerGenericAttributes = (await _genericAttributeService.GetAttributesForEntityAsync(customer.Id, "Customer")).ToList();
            
            //Get Shipping option attribute
            var shippingOptionAttribute = customerGenericAttributes.FirstOrDefault(ga =>
               ga.Key.Equals(NopCustomerDefaults.SelectedShippingOptionAttribute, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (shippingOptionAttribute != null)
                //delete shipping option if normal shipping address
                await _genericAttributeService.DeleteAttributeAsync(shippingOptionAttribute);

            //get pick up store attribute
            var pickUpStoreAttribute = customerGenericAttributes.FirstOrDefault(ga =>
               ga.Key.Equals(NopCustomerDefaults.SelectedPickupPointAttribute, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (pickUpStoreAttribute != null)
                //delete pick up store attribute if normal shipping address
                await _genericAttributeService.DeleteAttributeAsync(pickUpStoreAttribute);

            return RedirectToRoute("CheckoutShippingMethod");
        }

        #endregion

        #region shipping step add / edit address via pop up method

        [HttpPost("SaveEditShippingAddress")]
        //this method will add or update shipping address or return form with postback error to redisplay them in pop up 
        public virtual async Task<IActionResult> SaveEditShippingAddress(CheckoutShippingAddressModel model, IFormCollection form)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();

            //custom address attributes
            var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            var newAddress = model.ShippingNewAddress;

            if (ModelState.IsValid)
            {
                var customerAddresses = (await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList();

                //try to find an address with the same values (don't duplicate records)
                var address = _addressService.FindAddress(customerAddresses,
                    newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                    newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                    newAddress.Address1, newAddress.Address2, newAddress.City,
                    newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                    newAddress.CountryId, customAttributes);

                if (address == null)
                {
                    //if exisitng address then only update
                    if (newAddress.Id > 0)
                    {
                        var existingAddress = customerAddresses.Where(a => a.Id == newAddress.Id).FirstOrDefault();
                        if (existingAddress != null)
                        {
                            address = newAddress.ToEntity(existingAddress);
                            address.CustomAttributes = customAttributes;
                            await _addressService.UpdateAddressAsync(address);
                        }
                    }
                    else
                    {
                        //create new address
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;
                        if (address.CountryId == 0) address.CountryId = null;
                        if (address.StateProvinceId == 0) address.StateProvinceId = null;

                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(customer, address);
                    }
                }

                if (await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync() &&
                     (!customer.ShippingAddressId.HasValue || customer.ShippingAddressId.Value == 0))
                {
                    await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(
                        customer,
                        AnniqueCustomizationDefaults.CustomerTable,
                        customer.Id,
                        "ShippingAddress_Id",
                        customer.ShippingAddressId?.ToString() ?? "null",
                        address.Id.ToString()
                    );
                }
                customer.ShippingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);

                return Json(new { isSuccess = true, redirectUrl = Url.RouteUrl("CheckoutShippingMethod") });
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.ShippingNewAddress,
                address: null,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            ViewData.TemplateInfo.HtmlFieldPrefix = "ShippingNewAddress";

            var html = await RenderPartialViewToStringAsync("_CreateOrUpdateAddress", model.ShippingNewAddress);

            return Json(new { isSuccess = false, htmlData = html });
        }

        #endregion
    }
}
