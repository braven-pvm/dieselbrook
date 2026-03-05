using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain.AnniqueGifts
{
    public class Gift : BaseEntity
    {
        public string Sku { get; set; }

        public int ProductId { get; set; }

        public int nQtyLimit { get; set; }

        public decimal nMinSales { get; set; }

        public string cGiftType { get; set; }

        public int CampaignId { get; set; }

        public DateTime StartDateUtc { get; set; }

        public DateTime EndDateUtc { get; set;}
    }
}
