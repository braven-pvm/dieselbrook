using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace XcellenceIT.Plugin.ProductRibbons.ViewEngine
{
    public class ProductRibbonsViewEngine : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        /// <summary>
        /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine the
        /// values that would be consumed by this instance of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.
        /// The calculated values are used to determine if the view location has changed since the last time it was located.
        /// </summary>
        /// <param name="context">Context</param>
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine potential locations for a view.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="viewLocations">View locations</param>
        /// <returns>iew locations</returns>
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            context.Values.TryGetValue(THEME_KEY, out string theme);

            if (context.AreaName == "Admin")
            {
                viewLocations = new[] {
                        $"~/Plugins/XcellenceIT.ProductRibbons/Views/Admin/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/XcellenceIT.ProductRibbons/Views/Admin/Shared/{{0}}.cshtml",
                        $"~/Plugins/XcellenceIT.ProductRibbons/Views/Admin/Shared/{{2}}/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/XcellenceIT.ProductRibbons/Views/Admin/Shared/{{1}}/{{0}}.cshtml"
                    }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                        $"~/Plugins/XcellenceIT.ProductRibbons/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/XcellenceIT.ProductRibbons/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"~/Plugins/XcellenceIT.ProductRibbons/Views/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/XcellenceIT.ProductRibbons/Views/Shared/{{0}}.cshtml"
                    }.Concat(viewLocations);
            }

            return viewLocations;
        }
    }
}
