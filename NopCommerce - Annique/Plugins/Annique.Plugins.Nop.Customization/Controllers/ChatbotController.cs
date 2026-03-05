using Annique.Plugins.Nop.Customization.Domain.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Domain.Enums;
using Annique.Plugins.Nop.Customization.Domain.FulltextSearch;
using Annique.Plugins.Nop.Customization.Models.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Services.Helper;
using Irony.Parsing;
using LinqToDB.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class ChatbotController : BasePublicController
    {
        #region Fields

        private readonly IHttpClientFactory _clientFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly INopUrlHelper _nopUrlHelper;
        private readonly IWebHelper _webHelper;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ISearchSanitizationService _searchSanitizationService;
        private readonly IChatbotService _chatbotService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public ChatbotController(IHttpClientFactory clientFactory,
             ISettingService settingService,
            IStoreContext storeContext,
            IProductService productService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
             IUrlRecordService urlRecordService,
            INopUrlHelper nopUrlHelper,
            IWebHelper webHelper,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IShoppingCartService shoppingCartService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ISearchSanitizationService searchSanitizationService,
            IChatbotService chatbotService,
            ILogger logger)
        {
            _clientFactory = clientFactory;
            _settingService = settingService;
            _storeContext = storeContext;
            _productService = productService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _settingService = settingService;
            _urlRecordService = urlRecordService;
            _nopUrlHelper = nopUrlHelper;
            _webHelper = webHelper;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _shoppingCartService = shoppingCartService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _workContext = workContext;
            _searchSanitizationService = searchSanitizationService;
            _chatbotService = chatbotService;   
            _logger = logger;
        }

        #endregion

        #region Utility method

        private static string StripHtml(string input)
        {
            return Regex.Replace(input ?? "", "<.*?>", String.Empty);
        }

        #endregion

        #region Methods

        [HttpPost("GetRecommendation")]
        public async Task<IActionResult> GetRecommendation([FromBody] ChatRequest request)
        {
            try
            {
                var storeScope = await _storeContext.GetCurrentStoreAsync();
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

                var hasAccess = await _chatbotService.IsCustomerAllowedAsync();

                if (!settings.IsChatbotEnable || !hasAccess)
                    return StatusCode(401, new { reply = "This feature is not available for you" });

                var apiKey = settings.OpenAIApiKey;

                string cleanedMessage = _searchSanitizationService.SanitizeSearchQuery(request.Message);

                // Get top 20 relevant products (can also filter using request.Message)
                var products = await _anniqueCustomizationConfigurationService.SearchProductsWithFullTextAsync(
                    pageIndex: 0, pageSize: 20,
                    visibleIndividuallyOnly: true, keywords: cleanedMessage, searchOption: SearchOption.AnyWords);

                //task 635 Chatbot Feedback
                //added stock status condition
                var tasks = products
                                .Where(p => p.StockQuantity > 0)
                                .Select(async product =>
                                {
                                    var seName = await _urlRecordService.GetSeNameAsync(product);
                                    var url = await _nopUrlHelper.RouteGenericUrlAsync<Product>(
                                        new { SeName = seName },
                                        _webHelper.GetCurrentRequestProtocol()
                                    );

                                    return new
                                    {
                                        Id = product.Id,
                                        Name = product.Name,
                                        Url = url,
                                        Description = StripHtml(product.ShortDescription),
                                    };
                                });

                var productList = await Task.WhenAll(tasks);

                string productJson = JsonConvert.SerializeObject(productList, Formatting.None);

                var promptTemplate = settings.PromptTemplate;

                var prompt = promptTemplate
                    .Replace("{{message}}", request.Message)
                    .Replace("{{productJson}}", productJson);

                var openAiRequest = new
                {
                    model = "gpt-4.1-mini",
                    messages = new[]
                        {
                        new { role = "user", content = prompt }
                    }
                };

                var httpClient = _clientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    new StringContent(JsonConvert.SerializeObject(openAiRequest), Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode(); // Will throw if status code is not 200-level

                var result = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(result);

                string reply = jsonResponse.choices[0].message.content;
                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync(ex.Message, ex);
                return StatusCode(500, new { reply = "An error occurred while generating a recommendation. Please try again later." });
            }
        }

        [HttpPost("AddRecommendProductToCart")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ChatbotAddProductToCartResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddProductToCart(int productId,
            [FromQuery, Required] ShoppingCartType shoppingCartType,
            [FromQuery, Required] int quantity)
        {
            var storeScope = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

            var hasAccess = await _chatbotService.IsCustomerAllowedAsync();

            if (!settings.IsChatbotEnable || !hasAccess)
                return StatusCode(401, new { reply = "This feature is not available for you" });

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return BadRequest("No product found with the specified ID");

            if (product.ProductType != ProductType.SimpleProduct)
                return Ok(new ChatbotAddProductToCartResponse
                {
                    Success = false,
                    Message = "We can add only simple products"
                });

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
                return Ok(new ChatbotAddProductToCartResponse
                {
                    Success = false,
                    Message = "We cannot add to the cart such products from category pages it can confuse customers"
                });

            if (product.CustomerEntersPrice)
                return Ok(new ChatbotAddProductToCartResponse
                {
                    Success = false,
                    Message = "Cannot be added to the cart(requires a customer to enter price"
                });

            if (product.IsRental)
                return Ok(new ChatbotAddProductToCartResponse
                {
                    Success = false,
                    Message = "Rental products require start/end dates to be entered"
                });

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0)
                return Ok(new ChatbotAddProductToCartResponse
                {
                    Success = false,
                    Message = "Cannot be added to the cart (requires a customer to select a quantity from dropdownlist)"
                });

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes =
                await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
                return Ok(new ChatbotAddProductToCartResponse
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
                return Ok(new ChatbotAddProductToCartResponse { Success = false, Errors = addToCartWarnings });

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = await _shoppingCartService.AddToCartAsync(
                customer: customer,
                product: product,
                shoppingCartType: shoppingCartType,
                storeId: store.Id,
                attributesXml: attXml,
                quantity: quantity);
            if (addToCartWarnings.Any())
                return Ok(new ChatbotAddProductToCartResponse { Success = false, Errors = addToCartWarnings });

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                string.Format(
                    await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"),
                    product.Name), product);

            return Ok(new ChatbotAddProductToCartResponse
            {
                Success = true,
                Message = string.Format(
                    await _localizationService.GetResourceAsync(
                        "Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart"))
            });
        }

        [HttpPost("SubmitChatFeedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] ChatFeedbackModel model)
        {
            try
            {
                var storeScope = await _storeContext.GetCurrentStoreAsync();
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

                var hasAccess = await _chatbotService.IsCustomerAllowedAsync();

                if (!settings.IsChatbotEnable || !hasAccess)
                    return StatusCode(401, new { reply = "This feature is not available for you" });

                if (string.IsNullOrWhiteSpace(model.Status) || string.IsNullOrWhiteSpace(model.OriginalMessage))
                {
                    await _logger.WarningAsync($"Feedback not saved — missing required fields. " +
                        $"OriginalMessage: '{model.OriginalMessage}', Status: '{model.Status}'");

                    return Ok(new { success = false });
                }

                var customer = await _workContext.GetCurrentCustomerAsync();

                // Decode encoded input from JS (safety)
                model.OriginalMessage = Uri.UnescapeDataString(model.OriginalMessage ?? "");
                model.AiResponse = Uri.UnescapeDataString(model.AiResponse ?? "");

                // Clean AI response
                if (!string.IsNullOrEmpty(model.AiResponse))
                {
                    // Remove button elements entirely (with content inside)
                    model.AiResponse = Regex.Replace(model.AiResponse, @"<button\b[^>]*>.*?</button>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    // Preserve <a> tags, remove all other HTML tags
                    model.AiResponse = Regex.Replace(model.AiResponse, @"<(?!a\s|/a\s*).*?>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    // Optionally decode HTML entities and trim the final result
                    model.AiResponse = System.Net.WebUtility.HtmlDecode(model.AiResponse).Trim();
                }

                var feedback = new ChatbotFeedback
                {
                    OriginalMessage = model.OriginalMessage,
                    Status = model.Status,
                    AiResponse = model.AiResponse,
                    IpAddress = customer.LastIpAddress,
                    CreatedOnUtc = DateTime.UtcNow,
                    Username = customer.Username,
                };

                await _chatbotService.InsertFeedbackAsync(feedback);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error submitting chatbot feedback", ex);
                return StatusCode(500, new { success = false, message = "An error occurred while saving feedback." });
            }
        }


        #endregion
    }
}
