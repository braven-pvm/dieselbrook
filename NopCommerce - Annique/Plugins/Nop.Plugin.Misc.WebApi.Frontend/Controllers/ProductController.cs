using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class ProductController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ShippingSettings _shippingSettings;

        #endregion

        #region Ctor

        public ProductController(
            CatalogSettings catalogSettings,
            IAclService aclService,
            ICompareProductsService compareProductsService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDownloadService downloadService,
            IEventPublisher eventPublisher,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IReviewTypeService reviewTypeService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            ShoppingCartSettings shoppingCartSettings,
            ShippingSettings shippingSettings)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _compareProductsService = compareProductsService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _downloadService = downloadService;
            _eventPublisher = eventPublisher;
            _htmlFormatter = htmlFormatter;
            _localizationService = localizationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _reviewTypeService = reviewTypeService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
        }

        #endregion

        #region Utilities

        protected virtual async ValueTask<IList<string>> ValidateProductReviewAvailabilityAsync(Product product)
        {
            var res = new List<string>();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                res.Add(await _localizationService.GetResourceAsync("Reviews.OnlyRegisteredUsersCanWriteReviews"));

            if (!_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
                return res;

            var hasCompletedOrders = product.ProductType == ProductType.SimpleProduct
                ? await HasCompletedOrdersAsync(product)
                : await (await _productService.GetAssociatedProductsAsync(product.Id)).AnyAwaitAsync(HasCompletedOrdersAsync);

            if (!hasCompletedOrders)
                res.Add(await _localizationService.GetResourceAsync("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));

            return res;
        }

        protected virtual async ValueTask<bool> HasCompletedOrdersAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return (await _orderService.SearchOrdersAsync(customerId: customer.Id,
                productId: product.Id,
                osIds: new List<int> { (int)OrderStatus.Complete },
                pageSize: 1)).Any();
        }

        /// <summary>
        /// Parse a customer entered price of the product
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer entered price of the product
        /// </returns>
        protected virtual async Task<decimal> ParseCustomerEnteredPriceAsync(Product product, IDictionary<string, string> form)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var customerEnteredPriceConverted = decimal.Zero;
            if (product.CustomerEntersPrice)
                foreach (var formKey in form.Keys)
                {
                    if (formKey.Equals($"addtocart_{product.Id}.CustomerEnteredPrice", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (decimal.TryParse(form[formKey], out var customerEnteredPrice))
                            customerEnteredPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(customerEnteredPrice, await _workContext.GetWorkingCurrencyAsync());
                        break;
                    }
                }

            return customerEnteredPriceConverted;
        }

        /// <summary>
        /// Parse a entered quantity of the product
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <returns>Customer entered price of the product</returns>
        protected virtual int ParseEnteredQuantity(Product product, IDictionary<string, string> form)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var quantity = 1;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{product.Id}.EnteredQuantity", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = int.TryParse(form[formKey], out quantity);
                    break;
                }

            return quantity;
        }

        /// <summary>
        /// Get product attributes from the passed form
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form values</param>
        /// <param name="errors">Errors</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the attributes in XML format
        /// </returns>
        protected virtual async Task<string> ParseProductAttributesAsync(Product product, IDictionary<string, string> form, List<string> errors)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //product attributes
            var attributesXml = await GetProductAttributesXmlAsync(product, form, errors);

            //gift cards
            AddGiftCardsAttributesXml(product, form, ref attributesXml);

            return attributesXml;
        }

        /// <summary>
        /// Gets product attributes in XML format
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="errors">Errors</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the attributes in XML format
        /// </returns>
        protected virtual async Task<string> GetProductAttributesXmlAsync(Product product, IDictionary<string, string> form, List<string> errors)
        {
            var attributesXml = string.Empty;
            var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
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
                                {
                                    //get quantity entered by customer
                                    var quantity = 1;
                                    var quantityStr = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                    if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                        (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                        errors.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString(), quantity > 1 ? quantity : null);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                    {
                                        //get quantity entered by customer
                                        var quantity = 1;
                                        var quantityStr = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{item}_qty"];
                                        if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                            (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                            errors.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));

                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString(), quantity > 1 ? quantity : null);
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                //get quantity entered by customer
                                var quantity = 1;
                                var quantityStr = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));

                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? quantity : null);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml, attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                            }
                            catch
                            {
                                // ignored
                            }

                            if (selectedDate.HasValue)
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml, attribute, selectedDate.Value.ToString("D"));
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            if(Guid.TryParse(form[controlId], out var downloadGuid))
                            {
                                var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                if (download != null)
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }
            return attributesXml;
        }

        /// <summary>
        /// Adds gift cards attributes in XML format
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        protected virtual void AddGiftCardsAttributesXml(Product product, IDictionary<string, string> form, ref string attributesXml)
        {
            if (!product.IsGiftCard)
                return;

            var recipientName = "";
            var recipientEmail = "";
            var senderName = "";
            var senderEmail = "";
            var giftCardMessage = "";
            foreach (var formKey in form.Keys)
            {
                if (formKey.Equals($"giftcard_{product.Id}.RecipientName", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientName = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.RecipientEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientEmail = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.SenderName", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderName = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderEmail = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.Message", StringComparison.InvariantCultureIgnoreCase))
                {
                    giftCardMessage = form[formKey];
                }
            }

            attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml, recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
        }

        /// <summary>
        /// Parse product rental dates on the product details page
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        protected virtual void ParseRentalDates(Product product, IDictionary<string, string> form, out DateTime? startDate, out DateTime? endDate)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            startDate = null;
            endDate = null;

            if (product.IsRental)
            {
                var startControlId = $"rental_start_date_{product.Id}";
                var endControlId = $"rental_end_date_{product.Id}";
                var ctrlStartDate = form[startControlId];
                var ctrlEndDate = form[endControlId];
                try
                {
                    //currently we support only this format (as in the \Views\Product\_RentalInfo.cshtml file)
                    const string datePickerFormat = "d";
                    startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                    endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
                }
                catch
                {
                    // ignored
                }
            }
        }

        #endregion

        #region Product details page

        /// <summary>
        /// Get the product details
        /// </summary>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductDetails(int productId, [FromQuery] int updateCartItemId = 0)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return NotFound($"No product found with the specified id={productId}");

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
            if (notAvailable && !hasAdminAccess)
                return BadRequest();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return NotFound($"Not found parentGroupedProduct by id={product.ParentGroupedProductId}");

                return NotFound("Product is not visible individually.");
            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updateCartItem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updateCartItemId > 0)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: store.Id);
                updateCartItem = cart.FirstOrDefault(x => x.Id == updateCartItemId);
                //not found?
                if (updateCartItem == null) 
                    return NotFound("The requested shopping cart is not found.");
                //is it this product?
                if (product.Id != updateCartItem.ProductId) 
                    return BadRequest("The product does not match the requested.");
            }

            //save as recently viewed
            await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            //model
            var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, updateCartItem);
            //template
            var productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);

            var productDetailsModel = model.ToDto<ProductDetailsModelDto>();

            if (product.HasUserAgreement)
            {
                productDetailsModel.HasUserAgreement = product.HasUserAgreement;
                productDetailsModel.UserAgreementText = product.UserAgreementText;
            }

            if (product.HasSampleDownload) 
                productDetailsModel.SampleDownloadId = product.SampleDownloadId;

            var response = new ProductDetailsResponse 
            { 
                ProductDetailsModel = productDetailsModel,
                ProductTemplateViewPath = productTemplateViewPath
            };

            return Ok(response);
        }

        /// <summary>
        /// Get the estimate shipping
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(EstimateShippingResultModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> EstimateShipping([FromBody] BaseModelDtoRequest<ProductEstimateShippingModelDto> request)
        {            
            var errors = new List<string>();

            var model = request.Model.FromDto<ProductDetailsModel.ProductEstimateShippingModel>();

            if (model == null)
                model = new ProductDetailsModel.ProductEstimateShippingModel();

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

            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted)
            {
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShippingPopUp.Product.IsNotFound"));
                return Ok(new EstimateShippingResultModelDto
                {
                    Success = false,
                    Errors = errors
                });
            }
            var store = await _storeContext.GetCurrentStoreAsync();
            var wrappedProduct = new ShoppingCartItem
            {
                StoreId = store.Id,
                ShoppingCartType = ShoppingCartType.ShoppingCart,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                ProductId = product.Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            var addToCartWarnings = new List<string>();
            //customer entered price
            wrappedProduct.CustomerEnteredPrice = await ParseCustomerEnteredPriceAsync(product, request.Form);

            //entered quantity
            wrappedProduct.Quantity = ParseEnteredQuantity(product, request.Form);

            //product and gift card attributes
            wrappedProduct.AttributesXml = await ParseProductAttributesAsync(product, request.Form, addToCartWarnings);

            //rental attributes
            ParseRentalDates(product, request.Form, out var rentalStartDate, out var rentalEndDate);
            wrappedProduct.RentalStartDateUtc = rentalStartDate;
            wrappedProduct.RentalEndDateUtc = rentalEndDate;

            var result = await _shoppingCartModelFactory.PrepareEstimateShippingResultModelAsync(new[] { wrappedProduct }, model, false);

            return Ok(result.ToDto<EstimateShippingResultModelDto>());
        }

        /// <summary>
        /// Get product combinations
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<ProductCombinationModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductCombinations(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product by id={productId} not found.");

            var model = await _productModelFactory.PrepareProductCombinationModelsAsync(product);
            var modelDto = model.Select(p => p.ToDto<ProductCombinationModelDto>()).ToList();
            return Ok(modelDto);
        }

        /// <summary>
        /// Get related products
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="productThumbPictureSize"></param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<ProductOverviewModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRelatedProducts(int productId, [FromQuery] int? productThumbPictureSize = null)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product by id={productId} not found.");

            var productIds = (await _productService.GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToArray();

            //load products
            var products = await (await _productService.GetProductsByIdsAsync(productIds))
           //ACL and store mapping
           .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
           //availability dates
           .Where(p => _productService.ProductIsAvailable(p))
           //visible individually
           .Where(p => p.VisibleIndividually).ToListAsync();

            if (!products.Any())
                return Ok(new List<ProductOverviewModelDto>());

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

            var modelDto = model.Select(p => p.ToDto<ProductOverviewModelDto>()).ToList();
            return Ok(modelDto);
        }

        #endregion

        #region Recently viewed products

        /// <summary>
        /// Get recently viewed products
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<ProductOverviewModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return NotFound($"The setting {nameof(_catalogSettings.RecentlyViewedProductsEnabled)} is not enabled.");

            var products = await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(_catalogSettings.RecentlyViewedProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(await _productModelFactory.PrepareProductOverviewModelsAsync(products));

            var modelDto = model.Select(p => p.ToDto<ProductOverviewModelDto>()).ToList();

            return Ok(modelDto);
        }

        #endregion

        #region Home page products

        /// <summary>
        /// Get products on the home page (featured products)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductOverviewModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HomePageProducts()
        {
            var products = await (await _productService.GetAllProductsDisplayedOnHomepageAsync())
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            var modelDto = model.Select(p => p.ToDto<ProductOverviewModelDto>()).ToList();

            return Ok(modelDto);
        }

        #endregion
        
        #region Product reviews

        /// <summary>
        /// Validate product review availability for current customer
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ValidateProductReviewAvailabilityAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return NotFound($"Product id={productId} not found or does not meet the required criteria.");

            return Ok(await ValidateProductReviewAvailabilityAsync(product));
        }

        /// <summary>
        /// Get product reviews
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductReviewsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProductReviews(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return NotFound($"Product id={productId} not found or does not meet the required criteria.");

            var model = new ProductReviewsModel();
            model = await _productModelFactory.PrepareProductReviewsModelAsync(model, product);

            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;

            //default value for all additional review types
            if (model.ReviewTypeList.Count > 0)
                foreach (var additionalProductReview in model.AddAdditionalProductReviewList)
                    additionalProductReview.Rating = additionalProductReview.IsRequired ? _catalogSettings.DefaultProductRatingValue : 0;

            return Ok(model.ToDto<ProductReviewsModelDto>());
        }

        /// <summary>
        /// Add product reviews
        /// </summary>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductReviewsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProductReviewsAdd([FromBody] ProductReviewsModelDto model, int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            var store = await _storeContext.GetCurrentStoreAsync();

            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews ||
                !await _productService.CanAddReviewAsync(product.Id, store.Id))
                return NotFound($"Product id={productId} not found or does not meet the required criteria.");

            var resValidate = await ValidateProductReviewAvailabilityAsync(product);
            if (resValidate.Any())
                return BadRequest(resValidate);

            var productReviewsModel = await _productModelFactory.PrepareProductReviewsModelAsync(model.FromDto<ProductReviewsModel>(), product);

            //save review
            var rating = model.AddProductReview.Rating;
            if (rating < 1 || rating > 5)
                rating = _catalogSettings.DefaultProductRatingValue;
            var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

            var productReview = new ProductReview
            {
                ProductId = product.Id,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                Title = model.AddProductReview.Title,
                ReviewText = model.AddProductReview.ReviewText,
                Rating = rating,
                HelpfulYesTotal = 0,
                HelpfulNoTotal = 0,
                IsApproved = isApproved,
                CreatedOnUtc = DateTime.UtcNow,
                StoreId = store.Id,
            };

            await _productService.InsertProductReviewAsync(productReview);

            //add product review and review type mapping                
            foreach (var additionalReview in model.AddAdditionalProductReviewList)
            {
                var additionalProductReview = new ProductReviewReviewTypeMapping
                {
                    ProductReviewId = productReview.Id,
                    ReviewTypeId = additionalReview.ReviewTypeId,
                    Rating = additionalReview.Rating
                };

                await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(additionalProductReview);
            }

            //update product totals
            await _productService.UpdateProductReviewTotalsAsync(product);

            //notify store owner
            if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                await _workflowMessageService.SendProductReviewStoreOwnerNotificationMessageAsync(productReview, _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddProductReview",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddProductReview"), product.Name), product);

            //raise event
            if (productReview.IsApproved)
                await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));

            productReviewsModel.AddProductReview.Title = null;
            productReviewsModel.AddProductReview.ReviewText = null;

            productReviewsModel.AddProductReview.SuccessfullyAdded = true;
            if (!isApproved)
                productReviewsModel.AddProductReview.Result = await _localizationService.GetResourceAsync("Reviews.SeeAfterApproving");
            else
                productReviewsModel.AddProductReview.Result = await _localizationService.GetResourceAsync("Reviews.SuccessfullyAdded");

            return Ok(productReviewsModel.ToDto<ProductReviewsModelDto>());
        }

        /// <summary>
        /// Set product review helpfulness
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        /// <param name="washelpful">Indicator if the review was helpful</param>
        [HttpPost("{productReviewId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SetProductReviewHelpfulnessResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SetProductReviewHelpfulness(int productReviewId, [FromQuery][Required] bool washelpful)
        {
            var productReview = await _productService.GetProductReviewByIdAsync(productReviewId);
            if (productReview == null)
                return NotFound("No product review found with the specified id");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                return Ok(new SetProductReviewHelpfulnessResponse
                {
                    Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.OnlyRegistered"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                return Ok(new SetProductReviewHelpfulnessResponse
                {
                    Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.YourOwnReview"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });

            await _productService.SetProductReviewHelpfulnessAsync(productReview, washelpful);

            //new totals
            await _productService.UpdateProductReviewHelpfulnessTotalsAsync(productReview);

            return Ok(new SetProductReviewHelpfulnessResponse
            {
                Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        /// <summary>
        /// Customer product reviews for current customer
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerProductReviewsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CustomerProductReviews([FromQuery] int? pageNumber)
        {
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
                return BadRequest("Customer is not registered.");

            if (!_catalogSettings.ShowProductReviewsTabOnAccountPage)
                return NotFound($"The setting {nameof(_catalogSettings.ShowProductReviewsTabOnAccountPage)} is not enabled.");

            var model = await _productModelFactory.PrepareCustomerProductReviewsModelAsync(pageNumber);

            return Ok(model.ToDto<CustomerProductReviewsModelDto>());
        }

        #endregion

        #region Email a friend

        /// <summary>
        /// ProductEmailAFriend
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductEmailAFriendModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProductEmailAFriend(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return NotFound($"Product id={productId} not found or does not meet the required criteria.");

            var model = new ProductEmailAFriendModel();
            model = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model, product, false);
            return Ok(model.ToDto<ProductEmailAFriendModelDto>());
        }

        /// <summary>
        /// Send the product email a friend
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductEmailAFriendModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProductEmailAFriendSend([FromBody] ProductEmailAFriendModelDto model)
        {
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return NotFound($"Product id={model.ProductId} not found or does not meet the required criteria.");

            //check whether the current customer is guest and ia allowed to email a friend
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
                return BadRequest(await _localizationService.GetResourceAsync("Products.EmailAFriend.OnlyRegisteredUsers"));

            var productEmailAFriendModel = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model.FromDto<ProductEmailAFriendModel>(), product, true);

            //email
            await _workflowMessageService.SendProductEmailAFriendMessageAsync(await _workContext.GetCurrentCustomerAsync(),
                    (await _workContext.GetWorkingLanguageAsync()).Id, product,
                    model.YourEmailAddress, model.FriendEmail,
                    _htmlFormatter.FormatText(model.PersonalMessage, false, true, false, false, false, false));

            productEmailAFriendModel.SuccessfullySent = true;
            productEmailAFriendModel.Result = await _localizationService.GetResourceAsync("Products.EmailAFriend.SuccessfullySent");

            return Ok(productEmailAFriendModel.ToDto<ProductEmailAFriendModelDto>());
        }

        #endregion

        #region Comparing products

        /// <summary>
        /// Add product to compare list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(AddProductToCompareListResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddProductToCompareList(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published)
                return Ok(new AddProductToCompareListResponse
                {
                    Success = false,
                    Message = "No product found with the specified ID"
                });

            if (!_catalogSettings.CompareProductsEnabled)
                return Ok(new AddProductToCompareListResponse
                {
                    Success = false,
                    Message = "Product comparison is disabled"
                });

            await _compareProductsService.AddProductToCompareListAsync(productId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddToCompareList",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToCompareList"), product.Name), product);

            return Ok(new AddProductToCompareListResponse
            {
                Success = true,
                Message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToCompareList.Link"), Url.RouteUrl("CompareProducts"))
                //use the code below (commented) if you want a customer to be automatically redirected to the compare products page
                //redirect = Url.RouteUrl("CompareProducts"),
            });
        }

        /// <summary>
        /// Remove product from compare list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveProductFromCompareList(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"Product id={productId} not found or does not meet the required criteria.");

            if (!_catalogSettings.CompareProductsEnabled)
                return NotFound($"The setting {nameof(_catalogSettings.CompareProductsEnabled)} is not enabled.");

            await _compareProductsService.RemoveProductFromCompareListAsync(productId);

            return Ok();
        }

        /// <summary>
        /// Compare products
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CompareProductsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return NotFound($"The setting {nameof(_catalogSettings.CompareProductsEnabled)} is not enabled.");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };

            var products = await (await _compareProductsService.GetComparedProductsAsync())
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();

            //prepare model
            var poModels =
                (await _productModelFactory.PrepareProductOverviewModelsAsync(products,
                    prepareSpecificationAttributes: true)).ToList();

            foreach (var poModel in poModels) 
                model.Products.Add(poModel);

            return Ok(model.ToDto<CompareProductsModelDto>());
        }

        /// <summary>
        /// Clear compare products list
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual IActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return NotFound($"The setting {nameof(_catalogSettings.CompareProductsEnabled)} is not enabled.");

            _compareProductsService.ClearCompareProducts();

            return Ok();
        }

        #endregion
    }
}
