using Microsoft.AspNetCore.Http;

namespace Annique.Plugins.Nop.Customization.Extensions
{
    public static class HttpContextExtensions
    {
        private const string TrackingCookieFlagKey = "TrackingCookieCreated";

        /// <summary>
        /// Marks that the tracking cookie was created in this request.
        /// </summary>
        public static void MarkTrackingCookieCreated(this HttpContext context)
        {
            if (context == null)
                return;

            context.Items[TrackingCookieFlagKey] = true;
        }

        /// <summary>
        /// Checks whether the tracking cookie was created during this request.
        /// </summary>
        public static bool HasTrackingCookieCreated(this HttpContext context)
        {
            if (context == null)
                return false;

            return context.Items.ContainsKey(TrackingCookieFlagKey);
        }
    }
}
