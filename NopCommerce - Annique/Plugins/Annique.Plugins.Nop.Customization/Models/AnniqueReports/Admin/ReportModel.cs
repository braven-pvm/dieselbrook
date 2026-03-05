using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin
{
    public record ReportModel : BaseNopEntityModel, IAclSupportedModel
    {
        public ReportModel()
        {
            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
            ReportParameterSearchModel = new ReportParameterSearchModel();
            AvailableHiddenFields = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.CustomCSS")]
        public string CustomCSS { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.CustomJS")]
        public string CustomJS { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.TemplateBlock")]
        public string TemplateBlock { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.CustomText")]
        public string CustomText { get; set; }

        //ACL (customer roles)
        [NopResourceDisplayName("Admin.AnniqueReport.Fields.AclCustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.IsMenuOption")]
        public bool IsMenuOption { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.TabOrder")]
        public int TabOrder { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.IncludeReportCommonJs")]
        public bool IncludeReportCommonJs { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.IncludeReportParameters")]
        public bool IncludeReportParameters { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.URL")]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.SelectedHiddenFields")]
        public IList<string> SelectedHiddenFields { get; set; }
        public IList<SelectListItem> AvailableHiddenFields { get; set; }

        public ReportParameterSearchModel ReportParameterSearchModel { get; set; }

        [NopResourceDisplayName("Admin.AnniqueReport.Fields.PubliclyHostedPage")]
        public bool PubliclyHostedPage { get; set; }
    }
}
