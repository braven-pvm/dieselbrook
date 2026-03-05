using Annique.Plugins.Payments.AdumoOnline.Factories;
using Annique.Plugins.Payments.AdumoOnline.Models;
using Annique.Plugins.Payments.AdumoOnline.Services;
using Annique.Plugins.Payments.AdumoOnline.Validators;
using Annique.Plugins.Payments.AdumoOnline.ViewEngine;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.ONP.ONPTheme.Infrastructure
{
    public class PluginDbStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //register view engine
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new AdumoOnlineViewEngine());
            });

            //register a validators
            services.AddTransient<IValidator<ConfigurationModel>, ConfigurationValidator>();

            //register a service
            services.AddScoped<IAdumoOnlinePaymentService, AdumoOnlinePaymentService>();

            //regiser a model factory
            services.AddScoped<IAdumoOnlinePaymentModelFactory, AdumoOnlinePaymentModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 2000;
    }
}
