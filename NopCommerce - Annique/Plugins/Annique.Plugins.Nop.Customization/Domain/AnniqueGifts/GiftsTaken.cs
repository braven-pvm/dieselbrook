using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain.AnniqueGifts
{
    public class GiftsTaken : BaseEntity
    {
        public int GiftId { get; set; }

        public int CustomerId { get; set; }

        public int OrderItemId { get; set; }

        public int Qty { get; set; }
    }
}
