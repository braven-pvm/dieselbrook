using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain
{
    public class CategoryIntegration : BaseEntity
    {
        public int CategoryId {get; set;}

        public string IntegrationField { get; set;}

        public string IntegrationValue { get; set;}
    }
}
