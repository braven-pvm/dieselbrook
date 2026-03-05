using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain.AnniqueReports
{
    public class ReportParameterValue : BaseEntity
    {
        public int ReportParameterId { get; set; }

        public string Name { get; set; }

        public bool IsPreSelected { get; set; }

        public int DisplayOrder { get; set; }
    }
}
