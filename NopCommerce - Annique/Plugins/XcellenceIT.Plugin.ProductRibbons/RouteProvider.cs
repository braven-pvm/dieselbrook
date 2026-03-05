using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace XcellenceIT.Plugin.ProductRibbons
{
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapControllerRoute("XcellenceIT.Plugin.ProductRibbons.List", "ProductRibbonAdmin/List",
              new { controller = "ProductRibbonAdmin", action = "List", area = "admin" });

            routeBuilder.MapControllerRoute("XcellenceIT.Plugin.ProductRibbons.Create", "ProductRibbonAdmin/Create",
            new { controller = "ProductRibbonAdmin", action = "Create", area = "admin" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get { return -1; }
        }

    }
}
