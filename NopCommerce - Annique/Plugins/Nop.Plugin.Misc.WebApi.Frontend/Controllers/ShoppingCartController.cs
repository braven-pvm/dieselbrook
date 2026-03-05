using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class ShoppingCartController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ShippingSettings _shippingSettings;

        #endregion

        #region Ctor

        public ShoppingCartController(ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            ITaxService taxService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            ShoppingCartSettings shoppingCartSettings,
            ShippingSettings shippingSettings)
        {
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
        }

        #endregion

        #region Utilities
        
        protected virtual async Task ParseAndSaveCheckoutAttributesAsync(IList<ShoppingCartItem> cart, IDictionary<string, string> form)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var checkoutAttributes = await _checkoutAttributeService.GetAllCheckoutAttributesAsync(store.Id, excludeShippableAttributes);
            foreach (var attribute in checkoutAttributes)
            {
                var controlId = $"checkout_attribute_{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _checkoutAttributeService.GetCheckoutAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.Trim();
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }

                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                            }
                            catch
                            {
                                // ignored
                            }

                            if (selectedDate.HasValue)
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                        }

                        break;
                    case AttributeControlType.FileUpload:
                        {
                            if(Guid.TryParse(form[controlId], out var downloadGuid))
                            {
                                var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                if (download != null)
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                        attribute, download.DownloadGuid.ToString());
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            //validate conditional attributes (if specified)
            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _checkoutAttributeParser.RemoveCheckoutAttribute(attributesXml, attribute);
            }

            //save checkout attributes
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CheckoutAttributes, attributesXml, store.Id);
        }
        
        protected virtual async Task SaveItemAsync(ShoppingCartItem updateCartItem, List<string> addToCartWarnings, Product product,
           ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
           DateTime? rentalEndDate, int quantity)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            if (updateCartItem == null)
            //add to the cart
                addToCartWarnings.AddRange(await _shoppingCartService.AddToCartAsync(customer,
                    product, cartType, store.Id,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity));
            else
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, updateCartItem.ShoppingCartType, store.Id);

                var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                    cart, updateCartItem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updateCartItem.Id)
                //ensure it's some other shopping cart item
                    otherCartItemWithSameParameters = null;
                //update existing item
                addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                    updateCartItem.Id, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0)));
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                //delete the same shopping cart item (the other one)
                    await _shoppingCartService.DeleteShoppingCartItemAsync(otherCartItemWithSameParameters);
            }
        }
        
        protected virtual async Task<IActionResult> GetProductToCartDetailsAsync(List<string> addToCartWarnings, ShoppingCartType cartType,
            Product product)
        {
            if (addToCartWarnings.Any())
            //cannot be added to the cart/wishlist
            //let's display warnings
                return Ok(new AddProductToCartResponse
                {
                    Success = false,
                    Errors = addToCartWarnings.ToArray()
                });

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);
                        
                        return Ok(new AddProductToCartResponse
                        {
                            Success = true,
                            Message = string.Format(
                                await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishlist.Link"),
                                Url.RouteUrl("Wishlist"))
                        });
                    }

                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);
                        
                        var rezModel = await _shoppingCartModelFactory.PrepareMiniShoppingCartModelAsync();

                        return Ok(new AddProductToCartResponse
                        {
                            Success = true,
                            Message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"),
                                Url.RouteUrl("ShoppingCart")),
                            Model = rezModel.ToDto<MiniShoppingCartModelDto>()
                        });
                    }
            }
        }

        #endregion

        #region Methods

        [HttpPost]
        [ProducesResponseType(typeof(SelectShippingOptionResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SelectShippingOption(
            [FromBody] BaseModelDtoRequest<EstimateShippingModelDto> request,
            [FromQuery, Required] string name)
        {
            var model = request.Model.FromDto<EstimateShippingModel>();

            if (model == null)
                model = new EstimateShippingModel();

            var errors = new List<string>();
            if (string.IsNullOrEmpty(model.ZipPostalCode))
                errors.Add(await _localizationService.GetResourceAsync(
                    "Shipping.EstimateShipping.ZipPostalCode.Required"));

            if (model.CountryId == null || model.CountryId == 0)
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));

            if (errors.Count > 0)
                return Ok(new SelectShippingOptionResponse {Success = false, Errors = errors});

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer,
                ShoppingCartType.ShoppingCart, store.Id);
            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, request.Form);

            var shippingOptions = new List<ShippingOption>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                //find shipping options
                //performance optimization. try cache first
                shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(
                    customer,
                    NopCustomerDefaults.OfferedShippingOptionsAttribute,
                    store.Id);

                if (shippingOptions == null || !shippingOptions.Any())
                {
                    var address = new Address
                    {
                        CountryId = model.CountryId,
                        StateProvinceId = model.StateProvinceId,
                        ZipPostalCode = model.ZipPostalCode,
                    };

                    //not found? let's load them using shipping service
                    var getShippingOptionResponse = await _shippingService.GetShippingOptionsAsync(cart, address,
                        customer,
                        storeId: store.Id);

                    if (getShippingOptionResponse.Success)
                        shippingOptions = getShippingOptionResponse.ShippingOptions.ToList();
                    else
                        foreach (var error in getShippingOptionResponse.Errors)
                            errors.Add(error);
                }
            }

            var selectedShippingOption = shippingOptions?.Find(so =>
                !string.IsNullOrEmpty(so.Name) && so.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (selectedShippingOption == null)
                errors.Add(await _localizationService.GetResourceAsync(
                    "Shipping.EstimateShippingPopUp.ShippingOption.IsNotFound"));

            if (errors.Count > 0)
                return Ok(new SelectShippingOptionResponse {Success = false, Errors = errors});

            //reset pickup point
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer,
                NopCustomerDefaults.SelectedPickupPointAttribute, null,
                store.Id);

            //cache shipping option
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute, selectedShippingOption,
                store.Id);

            var rezModel = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, true);

            return Ok(new SelectShippingOptionResponse {Success = true, Model = rezModel.ToDto<OrderTotalsModelDto>()});
        }

        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddProductToCartResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddProductToCartFromCatalog(int productId,
            [FromQuery, Required] ShoppingCartType shoppingCartType,
            [FromQuery, Required] int quantity)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return BadRequest("No product found with the specified ID");

            if (product.ProductType != ProductType.SimpleProduct)
                return Ok(new AddProductToCartResponse
                {
                    Success = false,
                    Message = "We can add only simple products"
                });

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
                return Ok(new AddProductToCartResponse
                {
                    Success = false,
                    Message = "We cannot add to the cart such products from category pages it can confuse customers"
                });

            if (product.CustomerEntersPrice)
                return Ok(new AddProductToCartResponse
                {
                    Success = false,
                    Message = "Cannot be added to the cart(requires a customer to enter price"
                });

            if (product.IsRental)
                return Ok(new AddProductToCartResponse
                {
                    Success = false,
                    Message = "Rental products require start/end dates to be entered"
                });

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0)
                return Ok(new AddProductToCartResponse
                {
                    Success = false,
                    Message = "Cannot be added to the cart (requires a customer to select a quantity from dropdownlist)"
                });

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes =
                await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
                return Ok(new AddProductToCartResponse
                {
                    Success = false,
                    Message = "Adding to the cart is possible when all attributes are of the read-only checkbox type"
                });

            //creating XML for "read-only checkboxes" attributes
            var attXml = await productAttributes.AggregateAwaitAsync(string.Empty, async (attributesXml, attribute) =>
            {
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                foreach (var selectedAttributeId in attributeValues
                    .Where(v => v.IsPreSelected)
                    .Select(v => v.Id)
                    .ToList())
                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                        attribute, selectedAttributeId.ToString());

                return attributesXml;
            });

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer,
                shoppingCartType, store.Id);
            var shoppingCartItem =
                await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, shoppingCartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = await _shoppingCartService
                .GetShoppingCartItemWarningsAsync(customer, shoppingCartType,
                    product, store.Id, string.Empty,
                    decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false,
                    false);
            if (addToCartWarnings.Any())
                //cannot be added to the cart
                //let's display standard warnings
                return Ok(new AddProductToCartResponse {Success = false, Errors = addToCartWarnings});

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = await _shoppingCartService.AddToCartAsync(
                customer: customer,
                product: product,
                shoppingCartType: shoppingCartType,
                storeId: store.Id,
                attributesXml: attXml,
                quantity: quantity);
            if (addToCartWarnings.Any())
                return Ok(new AddProductToCartResponse {Success = false, Errors = addToCartWarnings});

            //added to the cart/wishlist
            switch (shoppingCartType)
            {
                case ShoppingCartType.Wishlist:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                        string.Format(
                            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"),
                            product.Name), product);

                    return Ok(new AddProductToCartResponse
                    {
                        Success = true,
                        Message = string.Format(
                            await _localizationService.GetResourceAsync(
                                "Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist"))
                    });
                }

                case ShoppingCartType.ShoppingCart:
                default:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                        string.Format(
                            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"),
                            product.Name), product);

                    var rezModel = await _shoppingCartModelFactory.PrepareMiniShoppingCartModelAsync();

                    return Ok(new AddProductToCartResponse
                    {
                        Success = true,
                        Message = string.Format(
                            await _localizationService.GetResourceAsync(
                                "Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                        Model = rezModel.ToDto<MiniShoppingCartModelDto>()
                    });
                }
            }
        }

        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddProductToCartResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddProductToCartFromDetails(
            [FromBody] IDictionary<string, string> dictionary,
            int productId,
            [FromQuery, Required] ShoppingCartType shoppingCartType)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return BadRequest("Product not found");

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
                return Ok(new AddProductToCartResponse
                {
                    Success = false, Message = "Only simple products could be added to the cart"
                });

            var form = new FormCollection(dictionary.ToDictionary(i => i.Key, i => new StringValues(i.Value)));

            //update existing shopping cart item
            var updateCartItemid = 0;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.UpdateodShoppingCartItemId",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = int.TryParse(form[formKey], out updateCartItemid);
                    break;
                }

            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updateCartItemid > 0)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                //search with the same cart type as specified
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    shoppingCartType, store.Id);

                updatecartitem = cart.FirstOrDefault(x => x.Id == updateCartItemid);

                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                    return Ok(new AddProductToCartResponse
                    {
                        Success = false,
                        Message = "This product does not match a passed shopping cart item identifier"
                    });
            }

            var addToCartWarnings = new List<string>();

            //customer entered price
            var customerEnteredPriceConverted =
                await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

            //entered quantity
            var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

            //product and gift card attributes
            var attributes =
                await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

            //rental attributes
            _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

            var cartType = updatecartitem?.ShoppingCartType ?? shoppingCartType;

            await SaveItemAsync(updatecartitem, addToCartWarnings, product, cartType, attributes,
                customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity);

            //return result
            return await GetProductToCartDetailsAsync(addToCartWarnings, cartType, product);
        }

        [HttpPut("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductDetailsAttributeChangeResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProductDetailsAttributeChange(
            [FromBody] IDictionary<string, string> formCollection,
            int productId,
            [FromQuery, Required] bool validateAttributeConditions,
            [FromQuery, Required] bool loadPicture)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return BadRequest("Product not found");

            var form = new FormCollection(formCollection.ToDictionary(i => i.Key, i => new StringValues(i.Value)));

            var errors = new List<string>();
            var attributeXml = await _productAttributeParser.ParseProductAttributesAsync(product, form, errors);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
                _productAttributeParser.ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);

            //sku, mpn, gtin
            var sku = await _productService.FormatSkuAsync(product, attributeXml);
            var mpn = await _productService.FormatMpnAsync(product, attributeXml);
            var gtin = await _productService.FormatGtinAsync(product, attributeXml);

            // calculating weight adjustment
            var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml);
            var totalWeight = product.BasepriceAmount;

            foreach (var attributeValue in attributeValues)
                switch (attributeValue.AttributeValueType)
                {
                    case AttributeValueType.Simple:
                        //simple attribute
                        totalWeight += attributeValue.WeightAdjustment;
                        break;
                    case AttributeValueType.AssociatedToProduct:
                        //bundled product
                        var associatedProduct =
                            await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                        if (associatedProduct != null)
                            totalWeight += associatedProduct.BasepriceAmount * attributeValue.Quantity;
                        break;
                }

            //price
            var price = string.Empty;
            //base price
            var basepricepangv = string.Empty;
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices) &&
                !product.CustomerEntersPrice)
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                //we do not calculate price of "customer enters price" option is enabled
                var (finalPriceWithDiscountBase, _, _) = await _shoppingCartService.GetUnitPriceAsync(product,
                    currentCustomer,
                    currentStore,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate, true);

                var finalPriceWithDiscount =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase,
                        await _workContext.GetWorkingCurrencyAsync());
                price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                basepricepangv =
                    await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase, totalWeight);
            }

            //stock
            var stockAvailability = await _productService.FormatStockMessageAsync(product, attributeXml);

            //conditional attributes
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            enabledAttributeMappingIds.Add(attribute.Id);
                        else
                            disabledAttributeMappingIds.Add(attribute.Id);
                    }
                }
            }

            //picture. used when we want to override a default product picture when some attribute is selected
            var pictureFullSizeUrl = string.Empty;
            var pictureDefaultSizeUrl = string.Empty;
            if (loadPicture)
            {
                //first, try to get product attribute combination picture
                var pictureId =
                    (await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributeXml))
                    ?.PictureId ?? 0;

                //then, let's see whether we have attribute values with pictures
                if (pictureId == 0)
                    pictureId = (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                        .FirstOrDefault(attributeValue => attributeValue.PictureId > 0)?.PictureId ?? 0;

                if (pictureId > 0)
                {
                    var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                        NopModelCacheDefaults.ProductAttributePictureModelKey,
                        pictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                    var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                    {
                        var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                        string fullSizeImageUrl, imageUrl;

                        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                        (imageUrl, picture) =
                            await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);

                        return picture == null
                            ? new PictureModel()
                            : new PictureModel {FullSizeImageUrl = fullSizeImageUrl, ImageUrl = imageUrl};
                    });
                    pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    pictureDefaultSizeUrl = pictureModel.ImageUrl;
                }
            }

            var isFreeShipping = product.IsFreeShipping;
            if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
                isFreeShipping = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                    .Where(attributeValue =>
                        attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    .SelectAwait(async attributeValue =>
                        await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId))
                    .AllAsync(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled ||
                                                   associatedProduct.IsFreeShipping);

            return Ok(new ProductDetailsAttributeChangeResponse
            {
                ProductId = productId,
                Gtin = gtin,
                Mpn = mpn,
                Sku = sku,
                Price = price,
                BasePricePangv = basepricepangv,
                StockAvailability = stockAvailability,
                Enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                Disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                PictureFullSizeUrl = pictureFullSizeUrl,
                PictureDefaultSizeUrl = pictureDefaultSizeUrl,
                IsFreeShipping = isFreeShipping,
                Message = errors.Any() ? errors.ToArray() : null
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(CheckoutAttributeChangeResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckoutAttributeChange([FromBody] IDictionary<string, string> form)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            //save selected attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);
            var attributeXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.CheckoutAttributes, store.Id);

            //conditions
            var enabledAttributeIds = new List<int>();
            var disabledAttributeIds = new List<int>();
            var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            var attributes = await _checkoutAttributeService.GetAllCheckoutAttributesAsync(store.Id, excludeShippableAttributes);
            foreach (var attribute in attributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeIds.Add(attribute.Id);
                    else
                        disabledAttributeIds.Add(attribute.Id);
                }
            }
                        
            var orderTotalsModel = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, true);
            
            var formattedAttributes = await _shoppingCartModelFactory.FormatSelectedCheckoutAttributesAsync();
            
            return Ok(new CheckoutAttributeChangeResponse
            {
                OrderTotalsModel = orderTotalsModel.ToDto<OrderTotalsModelDto>(),
                FormattedAttributes = formattedAttributes,
                EnabledAttributeIds = enabledAttributeIds.ToArray(),
                DisabledAttributeIds = disabledAttributeIds.ToArray()
            });
        }

        [HttpPost("{attributeId}")] 
        [ProducesResponseType(typeof(UploadFileProductAttributeResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UploadFileProductAttribute(int attributeId)
        {
            var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
                return Ok(new UploadFileProductAttributeResponse
                {
                    Success = false,
                    DownloadGuid = string.Empty
                });

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
                return Ok(new UploadFileProductAttributeResponse
                {
                    Success = false,
                    Message = "No file uploaded",
                    DownloadGuid = string.Empty
                });

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                //when returning Ok the mime-type must be set to text/plain
                //otherwise some browsers will pop-up a "Save As" dialog.
                    return Ok(new UploadFileProductAttributeResponse
                    {
                        Success = false,
                        Message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        DownloadGuid = string.Empty
                    });
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = string.Empty,
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            //when returning Ok the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Ok(new UploadFileProductAttributeResponse
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
                DownloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                DownloadGuid = download.DownloadGuid.ToString()
            });
        }

        [HttpPost("{attributeId}")]
        [ProducesResponseType(typeof(UploadFileCheckoutAttributeResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UploadFileCheckoutAttribute(int attributeId)
        {
            var attribute = await _checkoutAttributeService.GetCheckoutAttributeByIdAsync(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
                return Ok(new UploadFileCheckoutAttributeResponse
                {
                    Success = false,
                    DownloadGuid = string.Empty
                });

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
                return Ok(new UploadFileCheckoutAttributeResponse
                {
                    Success = false,
                    Message = "No file uploaded",
                    DownloadGuid = string.Empty
                });

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                //when returning Ok the mime-type must be set to text/plain
                //otherwise some browsers will pop-up a "Save As" dialog.
                    return Ok(new UploadFileCheckoutAttributeResponse
                    {
                        Success = false,
                        Message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        DownloadGuid = string.Empty
                    });
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = string.Empty,
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            //when returning Ok the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Ok(new UploadFileCheckoutAttributeResponse
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
                DownloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                DownloadGuid = download.DownloadGuid.ToString()
            });
        }

        [HttpGet]
        [ProducesResponseType(typeof(ShoppingCartModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Cart()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return BadRequest();

            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            return Ok(model.ToDto<ShoppingCartModelDto>());
        }

        [HttpGet]
        [ProducesResponseType(typeof(MiniShoppingCartModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> MiniCart()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return BadRequest();

            var model = await _shoppingCartModelFactory.PrepareMiniShoppingCartModelAsync();

            return Ok(model.ToDto<MiniShoppingCartModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCartModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateCart([FromBody] IDictionary<string, string> form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return BadRequest();

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            //get identifiers of items to remove
            var itemIdsToRemove = form["removefromcart"]
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(idString => int.TryParse(idString, out var id) ? id : 0)
                .Distinct().ToList();

            var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
                .ToDictionary(item => item.Id, item => item);

            //get order items with changed quantity
            var itemsWithNewQuantity = cart.Select(item => new
            {
                //try to get a new quantity for the item, set 0 for items to remove
                NewQuantity = itemIdsToRemove.Contains(item.Id) ? 0 : int.TryParse(form[$"itemquantity{item.Id}"], out var quantity) ? quantity : item.Quantity,
                Item = item,
                Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
            }).Where(item => item.NewQuantity != item.Item.Quantity);

            //order cart items
            //first should be items with a reduced quantity and that require other products; or items with an increased quantity and are required for other products
            var orderedCart = await itemsWithNewQuantity
                .OrderByDescendingAwait(async cartItem =>
                    (cartItem.NewQuantity < cartItem.Item.Quantity &&
                     (cartItem.Product?.RequireOtherProducts ?? false)) ||
                    (cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && (await _shoppingCartService
                         .GetProductsRequiringProductAsync(cart, cartItem.Product)).Any()))
                .ToListAsync();

            //try to update cart items with new quantities and get warnings
            var warnings = await orderedCart.SelectAwait(async cartItem => new
            {
                ItemId = cartItem.Item.Id,
                Warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                    cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                    cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity)
            }).ToListAsync();

            //updated cart
            cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            //prepare request
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            //update current warnings
            foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
            {
                //find shopping cart item request to display appropriate warnings
                var itemModel = model.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
                if (itemModel != null)
                    itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
            }

            return Ok(model.ToDto<ShoppingCartModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCartModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ApplyDiscountCoupon([FromBody] IDictionary<string, string> form,
            [FromQuery, Required] string discountCouponCode)
        {
            //trim
            if (!string.IsNullOrEmpty(discountCouponCode))
                discountCouponCode = discountCouponCode.Trim();

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            //cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer,
                ShoppingCartType.ShoppingCart, store.Id);

            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            var model = new ShoppingCartModel();
            if (!string.IsNullOrWhiteSpace(discountCouponCode))
            {
                //we find even hidden records here. this way we can display a user-friendly Message if it's expired
                var discounts =
                    (await _discountService.GetAllDiscountsAsync(couponCode: discountCouponCode, showHidden: true))
                    .Where(d => d.RequiresCouponCode)
                    .ToList();
                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                    {
                        var validationResult = await _discountService.ValidateDiscountAsync(discount,
                            customer, new[] {discountCouponCode});
                        userErrors.AddRange(validationResult.Errors);

                        return validationResult.IsValid;
                    });

                    if (anyValidDiscount)
                    {
                        //valid
                        await _customerService.ApplyDiscountCouponCodeAsync(
                            customer, discountCouponCode);
                        model.DiscountBox.Messages.Add(
                            await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied"));
                        model.DiscountBox.IsApplied = true;
                    }
                    else
                    {
                        if (userErrors.Any())
                            //some user errors
                            model.DiscountBox.Messages = userErrors;
                        else
                            //general error text
                            model.DiscountBox.Messages.Add(
                                await _localizationService.GetResourceAsync(
                                    "ShoppingCart.DiscountCouponCode.WrongDiscount"));
                    }
                }
                else
                    //discount cannot be found
                    model.DiscountBox.Messages.Add(
                        await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.CannotBeFound"));
            }
            else
                //empty coupon code
                model.DiscountBox.Messages.Add(
                    await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Empty"));

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            return Ok(model.ToDto<ShoppingCartModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCartModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ApplyGiftCard([FromBody] IDictionary<string, string> form,
            [FromQuery, Required] string giftCardCouponCode)
        {
            //trim
            if (!string.IsNullOrEmpty(giftCardCouponCode))
                giftCardCouponCode = giftCardCouponCode.Trim();

            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            //cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer,
                ShoppingCartType.ShoppingCart, store.Id);

            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            var model = new ShoppingCartModel();
            if (!await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
            {
                if (!string.IsNullOrWhiteSpace(giftCardCouponCode))
                {
                    var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: giftCardCouponCode))
                        .FirstOrDefault();
                    var isGiftCardValid = giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard);
                    if (isGiftCardValid)
                    {
                        await _customerService.ApplyGiftCardCouponCodeAsync(
                            customer, giftCardCouponCode);
                        model.GiftCardBox.Message =
                            await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.Applied");
                        model.GiftCardBox.IsApplied = true;
                    }
                    else
                    {
                        model.GiftCardBox.Message =
                            await _localizationService.GetResourceAsync(
                                "ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        model.GiftCardBox.IsApplied = false;
                    }
                }
                else
                {
                    model.GiftCardBox.Message =
                        await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                model.GiftCardBox.Message =
                    await _localizationService.GetResourceAsync(
                        "ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                model.GiftCardBox.IsApplied = false;
            }

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            return Ok(model.ToDto<ShoppingCartModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(EstimateShippingResultModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetEstimateShipping([FromBody] BaseModelDtoRequest<EstimateShippingModelDto> request)
        {
            var model = request.Model.FromDto<EstimateShippingModel>();

            if (model == null)
                model = new EstimateShippingModel();

            var errors = new List<string>();

            if (!_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.ZipPostalCode))
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.ZipPostalCode.Required"));

            if (_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.City))
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.City.Required"));

            if (model.CountryId == null || model.CountryId == 0)
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));

            if (errors.Count > 0)
                return Ok(new EstimateShippingResultModelDto
                {
                    Success = false,
                    Errors = errors
                });

            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);
            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, request.Form);

            var result = await _shoppingCartModelFactory.PrepareEstimateShippingResultModelAsync(cart, model, true);

            return Ok(result.ToDto<EstimateShippingResultModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCartModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveDiscountCoupon([FromBody] IDictionary<string, string> form)
        {
            var model = new ShoppingCartModel();

            //get discount identifier
            var discountId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("removediscount-", StringComparison.InvariantCultureIgnoreCase))
                    discountId = Convert.ToInt32(formValue["removediscount-".Length..]);
            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (discount != null)
                await _customerService.RemoveDiscountCouponCodeAsync(customer, discount.CouponCode);

            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
            
            return Ok(model.ToDto<ShoppingCartModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCartModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveGiftCardCode([FromBody] IDictionary<string, string> form)
        {
            var model = new ShoppingCartModel();

            //get gift card identifier
            var giftCardId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("removegiftcard-", StringComparison.InvariantCultureIgnoreCase))
                    giftCardId = Convert.ToInt32(formValue["removegiftcard-".Length..]);
            var gc = await _giftCardService.GetGiftCardByIdAsync(giftCardId);
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (gc != null)
                await _customerService.RemoveGiftCardCouponCodeAsync(customer, gc.GiftCardCouponCode);

            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            return Ok(model.ToDto<ShoppingCartModelDto>());
        }

        #endregion
    }
}