using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueReports.Public
{
    public record CustomerReportListModel : BaseNopModel
    {
        #region Ctor

        public CustomerReportListModel()
        {
            Reports = new List<ReportDetailsModel>();
        }

        #endregion

        #region Property

        public IList<ReportDetailsModel> Reports { get; set; }

        #endregion

        #region Nested Class

        public record ReportDetailsModel : BaseNopEntityModel
        {
            public ReportDetailsModel()
            {
                ReportParameters = new List<CustomReportParameterModel>();
            }
            public string Username { get; set; }
            public int StoreId { get; set; }

            public string Name { get; set; }

            public string ScriptsBlock { get; set; }

            public string CommonMyAppJs { get; set; }

            public string CustomCSS { get; set; }

            public string CustomJS { get; set; }

            public string TemplateBlock { get; set; }

            public string CustomText { get; set; }

            public bool IncludeReportCommonJs { get; set; }

            public bool IncludeReportParameters { get; set; }

            public string SeName { get; set; }

            public string HiddenFieldsHtml { get; set; } // Full HTML string ready for rendering

            public string HiddenFieldsJs { get; set; } // Full HTML string ready for rendering

            public IList<CustomReportParameterModel> ReportParameters { get; set; }
        }

        #endregion
    }
}
