using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin
{
    public record ReportParameterValueSearchModel : BaseSearchModel
    {
        public int ReportParameterId { get; set; }
    }
}
