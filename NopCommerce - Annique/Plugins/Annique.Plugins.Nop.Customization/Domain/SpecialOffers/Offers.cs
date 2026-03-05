using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain.SpecialOffers
{
    /// <summary>
    /// Represents a offers table
    /// </summary>
    public class Offers : BaseEntity 
    {
        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        public int DiscountId { get; set; }

        /// <summary>
        /// Gets or sets the rule type
        /// </summary>
        public string RuleType { get; set; }

        /// <summary>
        /// Gets or sets the max qty
        /// </summary>
        public int MaxQty { get; set; }

        /// <summary>
        /// Gets or sets the min qty
        /// </summary>
        public int MinQty { get; set; }

        /// <summary>
        /// Gets or sets the max value
        /// </summary>
        public decimal MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the min value
        /// </summary>
        public decimal MinValue { get; set; }

        /// <summary>
        /// Gets or sets the picture id
        /// </summary>
        public int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the flag for offer on total rsp or not
        /// </summary>
        public bool MinValueOnTotalRsp { get; set; }

        /// <summary>
        /// Gets or sets the Max Allowed qty
        /// </summary>
        public int MaxAllowedQty { get; set; }
    }
}
