using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ViewEngine
{
    public class AnniqueCustomizationViewEngine : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfiguration = EngineContext.Current.Resolve<IAnniqueCustomizationConfigurationService>();

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            context.Values.TryGetValue(THEME_KEY, out string theme);

            if (context.AreaName == "Admin")
            {
                viewLocations = new[] {
                        $"~/Plugins/Annique.Customization/Views/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Views/Shared/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Views/Shared/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Views/Shared/{{2}}/{{1}}/{{0}}.cshtml"
                 }.Concat(viewLocations);
            }

            var settings = Task.Run(_anniqueCustomizationConfiguration.IsPickupCollectionEnableAsync).Result;
            if (settings)
            {
                viewLocations = new[] {
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/{{1}}/{{0}}.cshtml"
                    }.Concat(viewLocations);
            }

            viewLocations = new[] {
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Customer/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/UserProfileInfo/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/PublicAnniqueReport/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/AnniqueEvents/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/CustomCheckout/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/OrderSummary/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/OrderTotals/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/FlyoutShoppingCart/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/HeaderLinks/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/CustomerNavigation/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/AwardHeaderLink/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/AwardRedeemReminder/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/UserLoginTimeout/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/SearchBox/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/CheckoutButton/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Product/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Awards/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/OTPForm/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/DiscountList/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/PrivateMessages/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/UserInfoHeaderBar/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/ActiveSpecialOfferMarquee/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/SearchOptionSelector/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/AnniqueCatalog/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Shared/Components/TripPromotion/{{0}}.cshtml",
                        $"~/Plugins/Annique.Customization/Themes/{theme}/Views/ConsultantRegistration/{{0}}.cshtml",
                    }.Concat(viewLocations);

            if (context.AreaName == null && context.ViewName == "BillingAddress")
            {
                viewLocations = new[]
                {
                    $"~/Plugins/Annique.Customization/Themes/{theme}/Views/Checkout/BillingAddress.cshtml",
                }
                .Concat(viewLocations);
            }
            return viewLocations;
        }
    }
}
