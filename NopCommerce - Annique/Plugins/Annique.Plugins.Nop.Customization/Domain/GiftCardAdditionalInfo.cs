using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain
{
    public class GiftCardAdditionalInfo : BaseEntity
    {
        public int GiftCardId { get; set; }

        public string Username { get; set; }
    }
}
