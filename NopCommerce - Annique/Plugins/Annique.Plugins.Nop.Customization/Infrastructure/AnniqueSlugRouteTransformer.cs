using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Infrastructure
{
    internal class AnniqueSlugRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly IUrlRecordService _urlRecordService;

        public AnniqueSlugRouteTransformer(IUrlRecordService urlRecordService)
        {
            _urlRecordService = urlRecordService;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (!values.TryGetValue("SeName", out var slugObj) || slugObj == null)
                return values;

            var slug = slugObj.ToString();
            var urlRecord = await _urlRecordService.GetBySlugAsync(slug);

            if (urlRecord?.EntityName != nameof(Report))
                return values;

            return new RouteValueDictionary
            {
                ["controller"] = "PublicAnniqueReport",
                ["action"] = "ReportDetails",
                ["reportId"] = urlRecord.EntityId
            };
        }

    }
}