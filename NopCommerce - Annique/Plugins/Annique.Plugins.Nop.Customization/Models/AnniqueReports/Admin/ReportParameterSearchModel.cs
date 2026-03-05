using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin
{
    public record ReportParameterSearchModel : BaseSearchModel
    {
        public int ReportId { get; set; }
    }
}
