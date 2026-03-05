using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.WebApi.Frontend.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(WebApiFrontendDefaults.ConfigurationRouteName, "Plugins/WebApiFrontend/Configure",
                new { controller = "WebApiFrontend", action = "Configure", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute("PaymentInfoWebViewPaymentInfo", "paymentinfowebview/paymentinfo",
                new { controller = "PaymentInfoWebView", action = "PaymentInfo" });

            endpointRouteBuilder.MapControllerRoute("PaymentInfoWebViewNextStep", "paymentinfowebview/nexstep/{step}",
                new { controller = "PaymentInfoWebView", action = "NextStep" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}
