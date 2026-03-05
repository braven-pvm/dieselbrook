// *************************************************************************
// *                                                                       *
// * Product Ribbons Plugin for nopCommerce                                *
// * Copyright (c) Xcellence-IT. All Rights Reserved.                      *
// *                                                                       *
// *************************************************************************
// *                                                                       *
// * Email: info@nopaccelerate.com                                         *
// * Website: http://www.nopaccelerate.com                                 *
// *                                                                       *
// *************************************************************************
// *                                                                       *
// * This  software is furnished  under a license  and  may  be  used  and *
// * modified  only in  accordance with the terms of such license and with *
// * the  inclusion of the above  copyright notice.  This software or  any *
// * other copies thereof may not be provided or  otherwise made available *
// * to any  other  person.   No title to and ownership of the software is *
// * hereby transferred.                                                   *
// *                                                                       *
// * You may not reverse  engineer, decompile, defeat  license  encryption *
// * mechanisms  or  disassemble this software product or software product *
// * license.  Xcellence-IT may terminate this license if you don't comply *
// * with  any  of  the  terms and conditions set forth in  our  end  user *
// * license agreement (EULA).  In such event,  licensee  agrees to return *
// * licensor  or destroy  all copies of software  upon termination of the *
// * license.                                                              *
// *                                                                       *
// * Please see the  License file for the full End User License Agreement. *
// * The  complete license agreement is also available on  our  website at * 
// * http://www.nopaccelerate.com/enterprise-license                       *
// *                                                                       *
// *************************************************************************

using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using System.IO;
using XcellenceIT.Plugin.ProductRibbons.Factories;
using XcellenceIT.Plugin.ProductRibbons.Models;
using XcellenceIT.Plugin.ProductRibbons.Services;
using XcellenceIT.Plugin.ProductRibbons.Utilities;
using XcellenceIT.Plugin.ProductRibbons.Validator;
using XcellenceIT.Plugin.ProductRibbons.ViewEngine;

namespace XcellenceIT.Plugin.ProductRibbons.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring plugin DB context on application startup
    /// </summary>
    public class PluginDbStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            string codeDll = "XcellenceIT.Plugin.ProductRibbons.XcellenceIt.Core.dll";
            EmbeddedAssembly.Load(codeDll, "XcellenceIt.Core.dll");

            services.AddScoped<IProductRibbonsService, ProductRibbonsService>();
            services.AddScoped<IProductPriceService, ProductPriceService>();
            services.AddScoped<IProductRibbonFactory, ProductRibbonFactory>();
            services.AddScoped<IProductRibbonPublicFactory, ProductRibbonPublicFactory>();

            services.AddTransient<IValidator<ProductRibbonModel>, ProductRibbonValidator>();
           
            //View Engine
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ProductRibbonsViewEngine());
            });
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            application.Use(async (context, next) =>
            {
                // Create a backup of the original response stream
                var backup = context.Response.Body;
                try
                {
                    using (var customStream = new MemoryStream())
                    {
                        // Assign readable/writeable stream
                        context.Response.Body = customStream;

                        await next();
                        customStream.Position = 0;

                        var content = new StreamReader(customStream).ReadToEnd();
                        customStream.Position = 0;
                        // Write custom content to response
                        await customStream.CopyToAsync(backup);
                    }
                }
                finally
                {
                    context.Response.Body = backup;
                }

            });
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => int.MaxValue;
    }
}
