using Annique.Plugins.Nop.Customization.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Logging;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.NewActivityLogs
{
    public class AdditionalActivityLogService : IAdditionalActivityLogService
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public AdditionalActivityLogService(ICustomerActivityService customerActivityService,
            IHttpContextAccessor httpContextAccessor,
            IWebHelper webHelper)
        {
            _customerActivityService = customerActivityService;
            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
        }

        #endregion

        public class TrackingCookie
        {
            public string Id { get; set; }
            public string Contents { get; set; }

            public string _fbp { get; set; }
        }

        public TrackingCookie GetCustomerActivityTrackingCookie()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.Request.Cookies.TryGetValue(AnniqueCustomizationDefaults.TrackingCookieName, out var visitId) == true)
            {
                // Decode the cookie value (if it was URL encoded)
                var decodedCookie = Uri.UnescapeDataString(visitId);

                // Deserialize the JSON string into a strongly-typed object
                var cookieObject = JsonConvert.DeserializeObject<TrackingCookie>(decodedCookie);

                // Try to fetch the _fbp cookie if not already stored
                if (string.IsNullOrEmpty(cookieObject._fbp))
                {
                    ctx.Request.Cookies.TryGetValue("_fbp", out var fbPixelValue);
                    if (!string.IsNullOrEmpty(fbPixelValue))
                    {
                        cookieObject._fbp = fbPixelValue;

                        // Update our cookie so future requests include it
                        var updatedValue = JsonConvert.SerializeObject(cookieObject);
                        ctx.Response.Cookies.Append(
                            AnniqueCustomizationDefaults.TrackingCookieName,
                            updatedValue,
                            new CookieOptions
                            {
                                HttpOnly = true,
                                IsEssential = true,
                                Secure = _webHelper.IsCurrentConnectionSecured(),
                                Expires = DateTime.UtcNow.AddHours(24)
                            });
                    }
                }

                // Return the deserialized object
                return cookieObject;
            }

            //bug 654 when no cookie , then create new cookie so cookie never get null during tracking
            var newCookieObject = CreateAndGetTrackingCookieObject();

            //mark cookie flag created so new cookie not created for each request
            ctx.MarkTrackingCookieCreated();

            return newCookieObject;
        }

        public async Task InsertActivityTrackingLogAsync(string activityLogType, string activityName, Customer customer)
        {
            // Get the deserialized cookie object
            var cookieObject = GetCustomerActivityTrackingCookie();

            // Prepare the activity data
            var activityData = new
            {
                Activity = activityName,
                Request = _webHelper.GetThisPageUrl(true),
                CustomerId = customer.Id,
                Cookie = new { cookieObject.Id, cookieObject.Contents , cookieObject._fbp}
            };

            string jsonMessage = JsonConvert.SerializeObject(activityData);

            await _customerActivityService.InsertActivityAsync(activityLogType,
                      jsonMessage, customer);
        }

        public TrackingCookie CreateAndGetTrackingCookieObject()
        {
            var guidValue = Guid.NewGuid().ToString();

            // Get the query string from the current request
            var queryString = _httpContextAccessor.HttpContext.Request.QueryString.ToString();

            // If query string is empty, set contents to null or some default value
            var contents = string.IsNullOrEmpty(queryString) ? null : queryString;

            // Try to read the Facebook Pixel cookie (if exists)
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("_fbp", out var fbPixelValue);

            // Create an object to store both GUID and contents (query string)
            var cookieData = new
            {
                Id = guidValue,
                Contents = contents, // contents will be null if the query string is empty
                _fbp = string.IsNullOrEmpty(fbPixelValue) ? null : fbPixelValue
            };

            // Serialize the object to JSON
            string cookieValue = JsonConvert.SerializeObject(cookieData);

            //create cookie
            _httpContextAccessor.HttpContext.Response.Cookies.Append(AnniqueCustomizationDefaults.TrackingCookieName, cookieValue, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = _webHelper.IsCurrentConnectionSecured(),
                Expires = DateTime.UtcNow.AddHours(24) // expires in 24 hours
            });

            //to escape cookie object
            return JsonConvert.DeserializeObject<TrackingCookie>(cookieValue);
        }
    }
}
