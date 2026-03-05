using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using System;

namespace Annique.Plugins.Nop.Customization.Domain.AnniqueReports
{
    public partial class Report : BaseEntity, IAclSupported, ISlugSupported
    {
        public string Name { get; set; }

        public bool Published { get; set; }

        public bool SubjectToAcl { get; set; }

        public string CustomCSS { get; set; }

        public string CustomJS { get; set; }

        public string TemplateBlock { get; set; }

        public string CustomText { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool IsMenuOption { get; set; }

        public int TabOrder { get; set; }

        // Boolean for including ReportCommon.js
        public bool IncludeReportCommonJs { get; set; }

        // Boolean for including parameters (PRINT, REFRESH, etc.)
        public bool IncludeReportParameters { get; set; }

        // Comma-separated list of hidden fields (e.g., CustomerID, Affiliate, etc.)
        public string HiddenFields { get; set; }

        public bool PubliclyHostedPage { get; set; }
    }
}
