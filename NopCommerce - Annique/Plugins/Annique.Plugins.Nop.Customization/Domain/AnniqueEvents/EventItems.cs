using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain.AnniqueEvents
{
    public class EventItems : BaseEntity
    {
        public int EventID { get; set; }

        public int ProductID { get; set; }

        public int nQtyLimit { get; set; }

        public DateTime dFrom { get; set; }

        public DateTime dTo { get; set; }

        public bool IActive { get; set; }

        public string Sku { get; set; }
    }
}
