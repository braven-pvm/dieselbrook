using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Annique.Plugins.Nop.Customization.Domain.AnniqueReports
{
    public class ReportParameter : BaseEntity
    {
        public int ReportId { get; set; }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int AttributeControlTypeId { get; set; }

        public AttributeControlType AttributeControlType
        {
            get => (AttributeControlType)AttributeControlTypeId;
            set => AttributeControlTypeId = (int)value;
        }
    }
}
