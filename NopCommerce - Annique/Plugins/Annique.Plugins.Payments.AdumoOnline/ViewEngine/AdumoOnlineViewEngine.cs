using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace Annique.Plugins.Payments.AdumoOnline.ViewEngine
{
    public class AdumoOnlineViewEngine : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public void PopulateValues(ViewLocationExpanderContext context)
        {
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
                        $"~/Plugins/Annique.AdumoOnline/Views/Admin/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Annique.AdumoOnline/Views/Admin/Shared/{{0}}.cshtml",
                        $"~/Plugins/Annique.AdumoOnline/Views/Admin/Shared/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Annique.AdumoOnline/Views/Admin/Shared/{{2}}/{{1}}/{{0}}.cshtml"
                 }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                        $"~/Plugins/Annique.AdumoOnline/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Annique.AdumoOnline/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"~/Plugins/Annique.AdumoOnline/Views/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Annique.AdumoOnline/Views/Shared/{{0}}.cshtml"
                 }.Concat(viewLocations);
            }
           
            return viewLocations;
        }
    }
}
