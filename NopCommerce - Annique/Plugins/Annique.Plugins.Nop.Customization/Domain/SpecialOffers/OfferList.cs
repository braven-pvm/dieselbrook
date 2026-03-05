using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain.SpecialOffers
{
    /// <summary>
    /// Represents a offer List table
    /// </summary>
    public class OfferList : BaseEntity
    {
        /// <summary>
        /// Gets or sets the offer Id
        /// </summary>
        public int OfferId { get; set; }

        /// <summary>
        /// Gets or sets the List Type
        /// </summary>
        public string ListType { get; set; }

        /// <summary>
        /// Gets or sets the productId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the citemno
        /// </summary>
        public string citemno { get; set; }
    }
}
