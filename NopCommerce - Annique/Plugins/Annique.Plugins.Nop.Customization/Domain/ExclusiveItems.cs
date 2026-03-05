using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain
{
    public class ExclusiveItems : BaseEntity
    {
        public int? ProductID { get; set; }

        public int? CustomerID { get; set; }

        public int? RegistrationID { get; set; }

        public int? nQtyLimit { get; set; }

        public int? nQtyPurchased { get; set; }

        public DateTime? dFrom { get; set; }

        public DateTime? dTo { get; set; }

        public bool? IActive { get; set; }

        public bool IStarter { get; set; }

        public bool? IForce { get; set; }
    }
}
