using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfiguration = EngineContext.Current.Resolve<IAnniqueCustomizationConfigurationService>();
       
        // <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //get language pattern
            //it's not needed to use language pattern in AJAX requests and for actions returning the result directly (e.g. file to download),
            //use it only for URLs of pages that the user can go to
            var lang = GetLanguageRoutePattern();

            #region task 629 New Features on Annique Reports

            endpointRouteBuilder.MapDynamicControllerRoute<AnniqueSlugRouteTransformer>("{SeName}");

            endpointRouteBuilder.MapControllerRoute(
                    name: "Report",
                    pattern: "{SeName}",
                    defaults: new { controller = "Common", action = "SlugRedirect" });

            #endregion

            //custom code for manufacturer change to brand
            endpointRouteBuilder.MapControllerRoute(name: "ManufacturerList",
                pattern: $"{lang}/brand/all/",
                defaults: new { controller = "Catalog", action = "ManufacturerAll" });

            //autocomplete search term (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "ProvinceSearchAutoComplete",
                pattern: $"shipping/searchtermautocomplete",
                defaults: new { controller = "CustomCheckout", action = "ProvinceSearchTermAutoComplete" });
           
            //If user has consultant role then show user additional info
            var isConsultantUser = Task.Run(_anniqueCustomizationConfiguration.IsConsultantRoleAsync).Result;
            if (isConsultantUser)
            {
                //user profile
                endpointRouteBuilder.MapControllerRoute(name: "UserInfo",
                    pattern: $"customer/info",
                    defaults: new { controller = "UserProfileInfo", action = "Info" });
            }

            endpointRouteBuilder.MapControllerRoute("CustomerReports", "customer/reports",
                    new { controller = "PublicAnniqueReport", action = "CustomerReports" });

            endpointRouteBuilder.MapControllerRoute("EventList", "event_list",
                   new { controller = "AnniqueEvents", action = "EventList" });

            endpointRouteBuilder.MapControllerRoute("BookingDetails", "event_tickets/{eventId:min(0)}",
                  new { controller = "AnniqueEvents", action = "BookingDetails" });

            endpointRouteBuilder.MapControllerRoute("ConfirmEvent", "confirm_tickets/{eventId:min(0)}",
                 new { controller = "AnniqueEvents", action = "ConfirmFreeEvent" });


            endpointRouteBuilder.MapControllerRoute("GiftProductPopUp", pattern: $"checkout/gifts",
                    new { controller = "CustomCheckout", action = "GiftProductPopUp" });

            endpointRouteBuilder.MapControllerRoute(name: "ReOrder",
                pattern: $"{lang}/reorder/{{orderId:min(0)}}",
                defaults: new { controller = "CustomOrder", action = "ReOrder" });

            endpointRouteBuilder.MapControllerRoute(name: "MyAccountInfo",
                    pattern: $"customer/myaccount",
                    defaults: new { controller = "UserProfileInfo", action = "CustomMyAccountPage" });

            endpointRouteBuilder.MapControllerRoute("AwardList", "award_list",
                   new { controller = "Awards", action = "AwardList" });

            var settings = Task.Run(_anniqueCustomizationConfiguration.IsPickupCollectionEnableAsync).Result;
            if (settings)
            {
                //custom code for CheckoutSelectShippingAddress
                endpointRouteBuilder.MapControllerRoute(name: "CheckoutSelectShippingAddress",
                 pattern: $"{lang}/checkout/selectshippingaddress",
                 defaults: new { controller = "PublicPickUpCollection", action = "SelectShippingAddress" });
            }

            #region task 613 full text search

            //override routes for product search , product search auto complete and search products 

            //product search
            endpointRouteBuilder.MapControllerRoute(name: "ProductSearch",
                pattern: $"search/",
                defaults: new { controller = "AnniqueCatalog", action = "Search" });

            //autocomplete search term (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "ProductSearchAutoComplete",
                pattern: $"catalog/searchtermautocomplete",
                defaults: new { controller = "AnniqueCatalog", action = "SearchTermAutoComplete" });

            endpointRouteBuilder.MapControllerRoute(name: "SearchProducts",
                pattern: $"product/search",
                defaults: new { controller = "AnniqueCatalog", action = "SearchProducts" });

            #endregion

            endpointRouteBuilder.MapControllerRoute(
            name: "Consultant.Register.Form",
            pattern: "consultant-register",
            defaults: new { controller = "ConsultantRegistration", action = "RegisterConsultant" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => int.MaxValue;
    }
}
