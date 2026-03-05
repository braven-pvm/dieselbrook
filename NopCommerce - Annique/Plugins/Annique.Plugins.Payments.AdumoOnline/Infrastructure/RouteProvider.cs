using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Annique.Plugins.Payments.AdumoOnline.Infrastructure
{
    /// <summary>
    /// Represents route provider of adumo online plugin
    /// </summary>
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        // <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(name: "CheckoutView",
               pattern: $"/PaymentAdumoOnline/CheckoutView/{{orderGuid:guid}}",
               defaults: new { controller = "PaymentAdumoOnline", action = "CheckoutView" });

            //Complete
            endpointRouteBuilder.MapControllerRoute(name: "SuccessReturn",
                pattern: $"/PaymentAdumoOnline/SuccessReturn/{{orderGuid:guid}}",
                 new { controller = "PaymentAdumoOnline", action = "SuccessReturn" });

            //Cancel
            endpointRouteBuilder.MapControllerRoute(name: "FailureReturn",
                pattern: $"/PaymentAdumoOnline/FailureReturn/{{orderGuid:guid}}",
                 new { controller = "PaymentAdumoOnline", action = "FailureReturn" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 1;
    }
}
