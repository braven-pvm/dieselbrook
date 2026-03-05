using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain
{
    public class Lookups : BaseEntity
    {
        public string ctype { get; set; }
        
        public string code { get; set; }

        public string description { get; set; }

        public bool Iactive { get; set; }

        public int StoreId { get; set; }
    }
}
