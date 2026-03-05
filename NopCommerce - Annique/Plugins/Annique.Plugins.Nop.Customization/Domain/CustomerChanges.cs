using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain
{
    public class CustomerChanges : BaseEntity
    {
        public int ChangeId { get; set; }

        public string cTableName { get; set; }

        public int CustomerId { get; set; }

        public string cCustno { get; set; }

        public string cFieldname { get; set; }

        public string cOldvalue { get; set; }

        public string cNewvalue { get; set; }

        public DateTime InsUpdDate { get; set; }

        public DateTime? Updated { get; set; }
    }
}
