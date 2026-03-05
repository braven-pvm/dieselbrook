using Annique.Plugins.Nop.Customization.Extensions;
using Annique.Plugins.Nop.Customization.Services.NewActivityLogs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Web.Framework;
using Nop.Web.Framework.Events;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    public class PageRenderingEventConsumer : IConsumer<PageRenderingEvent>
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public PageRenderingEventConsumer(IHttpContextAccessor httpContextAccessor,
            IWorkContext workContext,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService)
        {
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _webHelper = webHelper;
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region Method

        public async Task HandleEventAsync(PageRenderingEvent eventMessage)
        {
            #region  Task 644 new Activity logs

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (_httpContextAccessor.HttpContext.GetRouteValue("area") is not string area || area != AreaNames.Admin)
            {
                var context = _httpContextAccessor.HttpContext;

                // Skip if we already created the cookie earlier in this same request
                if (context.HasTrackingCookieCreated())
                    return;

                // Check if cookie exists
                if (!_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(AnniqueCustomizationDefaults.TrackingCookieName))
                {
                    //do not inject IAdditionalActivityLogService via constructor because it'll cause circular references
                    var _additionalActivityLogService = EngineContext.Current.Resolve<IAdditionalActivityLogService>();

                    var cookieObject = _additionalActivityLogService.CreateAndGetTrackingCookieObject();

                    context.MarkTrackingCookieCreated();

                    //prepare json message
                    var activityData = new
                    {
                        Activity = "First Landing page",
                        Request = _webHelper.GetThisPageUrl(true),
                        CustomerId = customer.Id,
                        Cookie = cookieObject,
                    };

                    string jsonMessage = JsonConvert.SerializeObject(activityData);

                    //insert activity for first landing page
                    await _customerActivityService.InsertActivityAsync("PublicStore.Visit",
                      jsonMessage, customer);
                }
            }

            #endregion
        }

        #endregion
    }
}
