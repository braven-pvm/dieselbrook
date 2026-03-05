using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain.ConsultantAwards
{
    public class AwardShoppingCartItem : BaseEntity
    {
        public int AwardId { get; set; }

        public int ShoppingCartItemId { get; set; }

        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public int StoreId { get; set; }

        public int Quantity { get; set; }
    }
}
