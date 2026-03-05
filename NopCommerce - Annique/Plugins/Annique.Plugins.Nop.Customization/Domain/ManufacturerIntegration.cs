using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain
{
    public class ManufacturerIntegration : BaseEntity
    {
        public int ManufacturerId { get; set; }
        public string IntegrationField { get; set; }
        public string IntegrationValue { get; set;}
    }
}
