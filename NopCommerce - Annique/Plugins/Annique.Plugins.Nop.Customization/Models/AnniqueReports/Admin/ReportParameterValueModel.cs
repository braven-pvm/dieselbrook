using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin
{
    public record ReportParameterValueModel : BaseNopEntityModel
    {
        public int ReportParameterId { get; set; }

        [NopResourceDisplayName("Admin.ReportParameterValue.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ReportParameterValue.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [NopResourceDisplayName("Admin.ReportParameterValue.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}
