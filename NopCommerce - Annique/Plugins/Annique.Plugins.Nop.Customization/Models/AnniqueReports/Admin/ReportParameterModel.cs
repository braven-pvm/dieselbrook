using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin
{
    public record ReportParameterModel : BaseNopEntityModel
    {
        public ReportParameterModel()
        {
            ReportParameterValueSearchModel = new ReportParameterValueSearchModel();
        }

        public int ReportId { get; set; }

        [NopResourceDisplayName("Admin.ReportParameter.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ReportParameter.Fields.AttributeControlType")]
        public int AttributeControlTypeId { get; set; }

        [NopResourceDisplayName("Admin.ReportParameter.Fields.AttributeControlType")]
        public string AttributeControlTypeName { get; set; }

        [NopResourceDisplayName("Admin.ReportParameter.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public ReportParameterValueSearchModel ReportParameterValueSearchModel { get; set; }
    }
}
