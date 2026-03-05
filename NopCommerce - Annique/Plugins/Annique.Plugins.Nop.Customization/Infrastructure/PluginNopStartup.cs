using Annique.Plugins.Nop.Customization.ActionFilters;
using Annique.Plugins.Nop.Customization.Controllers;
using Annique.Plugins.Nop.Customization.Factories;
using Annique.Plugins.Nop.Customization.Factories.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Factories.AnniqueReport;
using Annique.Plugins.Nop.Customization.Factories.Catalog;
using Annique.Plugins.Nop.Customization.Factories.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Factories.Checkout;
using Annique.Plugins.Nop.Customization.Factories.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Factories.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Factories.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Factories.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Factories.GiftCardAllocation;
using Annique.Plugins.Nop.Customization.Factories.OverriddenFactory;
using Annique.Plugins.Nop.Customization.Factories.PickUpCollection;
using Annique.Plugins.Nop.Customization.Factories.ShippingRule;
using Annique.Plugins.Nop.Customization.Factories.UserProfile;
using Annique.Plugins.Nop.Customization.Filters;
using Annique.Plugins.Nop.Customization.Infrastructure;
using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public;
using Annique.Plugins.Nop.Customization.Models.QuickCheckout;
using Annique.Plugins.Nop.Customization.Models.UserProfile;
using Annique.Plugins.Nop.Customization.Services;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Services.AnniqueReport;
using Annique.Plugins.Nop.Customization.Services.ApiServices;
using Annique.Plugins.Nop.Customization.Services.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Annique.Plugins.Nop.Customization.Services.GiftCardAllocation;
using Annique.Plugins.Nop.Customization.Services.Helper;
using Annique.Plugins.Nop.Customization.Services.NewActivityLogs;
using Annique.Plugins.Nop.Customization.Services.Orders;
using Annique.Plugins.Nop.Customization.Services.OTP;
using Annique.Plugins.Nop.Customization.Services.OverriddenServices;
using Annique.Plugins.Nop.Customization.Services.PickUpCollection;
using Annique.Plugins.Nop.Customization.Services.PrivateMessagePopUp;
using Annique.Plugins.Nop.Customization.Services.ShippingAddressValidation;
using Annique.Plugins.Nop.Customization.Services.ShippingRule;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Annique.Plugins.Nop.Customization.Services.StaffCustomerCheckout;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Annique.Plugins.Nop.Customization.Validators;
using Annique.Plugins.Nop.Customization.Validators.AnniqueReports;
using Annique.Plugins.Nop.Customization.Validators.QuickCheckout;
using Annique.Plugins.Nop.Customization.Validators.UserProfile;
using Annique.Plugins.Nop.Customization.ViewEngine;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Factories;

namespace Nop.Plugin.ONP.ONPTheme.Infrastructure
{
    public class PluginDbStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(AffiliateLinksActionFilter));
                options.Filters.Add(typeof(CheckCustomAffiliateAttribute));
                options.Filters.Add(typeof(UpdateCartActionFilter));
                options.Filters.Add(typeof(CheckoutCompletedActionFilter));
                options.Filters.Add(typeof(CheckoutActionFilter));
                options.Filters.Add(typeof(BillingAddressActionFilter));
                options.Filters.Add(typeof(RePostPaymentActionFilter));
                options.Filters.Add(typeof(AddressEditActionFilter));
                options.Filters.Add(typeof(CustomerInfoActionFilter));
                options.Filters.Add(typeof(CustomerRegisterActionFilter));
                options.Filters.Add(typeof(AdminOrderActionFilter));
                options.Filters.Add(typeof(PaymentInfoActionFilter));
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new AnniqueCustomizationViewEngine());
            });

            //register Services
            services.AddScoped<IAnniqueCustomizationConfigurationService, AnniqueCustomizationConfigurationService>();
            services.AddScoped<ICategoryIntegrationService, CategoryIntegrationService>();
            services.AddScoped<IPickUpCollectionService, PickUpCollectionService>();
            services.AddScoped<IManufacturerIntegrationService, ManufacturerIntegrationService>();
            services.AddScoped<IStaffCustomerCheckoutRuleService, StaffCustomerCheckoutRuleService>();
            services.AddScoped<IShippingAddressValidationService, ShippingAddressValidationService>();
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IExclusiveItemsService,ExclusiveItemsService>();
            services.AddScoped<IProductService, CustomProductService>();
            services.AddScoped<IUserProfileAdditionalInfoService, UserProfileAdditionalInfoService>();
            services.AddScoped<IAnniqueReportService, AnniqueReportService>();
            services.AddScoped<IReportParameterService, ReportParameterService>();
            services.AddScoped<IEventService,EventService>();
            services.AddScoped<IGiftService, GiftService>();
            services.AddScoped<ICustomOrderProcessingService,CustomOrderProcessingService>();
            services.AddScoped<IGiftCardAdditionalInfoService, GiftCardAdditionalInfoService>();
            services.AddScoped<IAwardService, AwardService>();
            services.AddScoped<IShoppingCartService,AnniqueShoppingCartService>();
            services.AddScoped<IOrderTotalCalculationService,AnniqueOrderTotalCalculationService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IDiscountCustomerMappingService, DiscountCustomerMappingService>();
            services.AddScoped<IDiscountService, OverrideDiscountService>();
            services.AddScoped<IPriceCalculationService, OverriddenPriceCalculationService>();
            services.AddScoped<ICustomPrivateMessageService, CustomPrivateMessageService>();
            services.AddScoped<IOrderReportService, OverriddenOrderReportService>();
            services.AddScoped<ICustomCacheManagerService, CustomCacheManagerService>();
            services.AddScoped<ISpecialOffersService,SpecialOffersService>();
            services.AddScoped<IAddressService, OverriddenAddressService>();
            services.AddScoped<ICustomShippingRuleService,CustomShippingRuleService>();
            services.AddScoped<ICustomerRegistrationService, OverriddenCustomerRegistrationService>();
            services.AddScoped<IAuthenticationService, OverriddenCookieAuthenticationService>();
            services.AddScoped<AnniqueSlugRouteTransformer>();
            services.AddScoped<IConsultantNewRegistrationService, ConsultantNewRegistrationService>();
            services.AddScoped<IAdditionalActivityLogService,AdditionalActivityLogService>();
            services.AddScoped<IWebHelper,OverriddenWebHelper>();

            //Register factories
            services.AddScoped<ICategoryIntegrationModelFactory, CategoryIntegrationModelFactory>();
            services.AddScoped<IPickUpCollectionModelFactory, PickUpCollectionModelFactory>();
            services.AddScoped<IManufacturerIntegrationModelFactory, ManufacturerIntegrationModelFactory>();
            services.AddScoped<IUserProfileAdditionalInfoModelFactory, UserProfileAdditionalInfoModelFactory>();
            services.AddScoped<IAnniqueReportModelFactory,AnniqueReportModelFactory>();
            services.AddScoped<IEventsModelFactory,EventsModelFactory>();
            services.AddScoped<IGiftModelFactory,GiftModelFactory>();
            services.AddScoped<IGiftCardAdditionalInfoModelFactory, GiftCardAdditionalInfoModelFactory>();
            services.AddScoped<IAwardsModelFactory,AwardsModelFactory>();
            services.AddScoped<ICheckoutModelFactory, CustomCheckoutModelFactory>();
            services.AddScoped<ICatalogModelFactory, AnniqueCatalogModelFactory>();
            services.AddScoped<IDiscountAllocationModelFactory, DiscountAllocationModelFactory>();
            services.AddScoped<ICommonModelFactory, OverriddenCommonModelFactory>();
            services.AddScoped<ICustomShippingRuleFactory,CustomShippingRuleFactory>();
            services.AddScoped<IConsultantRegistrationModelFactory, ConsultantRegistrationModelFactory>();

            //register a validators
            services.AddTransient<IValidator<UserProfileAdditionalInfoModel>, UserProfileAdditionalInfoValidator>();
            services.AddTransient<IValidator<ReportModel>, ReportModelValidator>();
            services.AddTransient<IValidator<ReportParameterModel>, ReportParameterModelValidator>();
            services.AddTransient<IValidator<ReportParameterValueModel>, ReportParameterValueModelValidator>();
            services.AddTransient<IValidator<CheckoutLoginModal>, CheckoutLoginValidator>();
            services.AddTransient<IValidator<CheckoutRegisterModel>,CheckoutRegisiterValidator>();
            services.AddTransient<IValidator<ConsultantRegistrationModel>, ConsultantRegistrationModelValidator>();

            services.AddScoped<BasePublicController, BaseCustomPublicController>();
            services.AddScoped<ProductController, OverriddenProductController>();

            services.AddSingleton<ISearchSanitizationService, SearchSanitizationService>();
            services.AddScoped<IChatbotService,ChatbotService>();
            services.AddScoped<IChatFeedbackModelFactory,ChatFeedbackModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
           
        }

        public int Order => int.MaxValue;
    }
}
