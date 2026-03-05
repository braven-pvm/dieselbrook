using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain.ConsultantAwards
{
    public class Award : BaseEntity
    {
        public int CustomerId { get; set; }

        public string AwardType { get; set; }

        public string Description { get; set; }

        public int MaxValue { get; set; }

        public DateTime ExpiryDate { get; set; }

        public int? OrderId { get; set; }

        public DateTime? dcreated { get; set; }

        public DateTime? dtaken { get; set; }
    }
}
