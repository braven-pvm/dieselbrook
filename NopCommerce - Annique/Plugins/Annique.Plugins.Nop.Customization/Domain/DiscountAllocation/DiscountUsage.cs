using Nop.Core;

namespace Annique.Plugins.Nop.Customization.Domain.DiscountAllocation
{
    /// <summary>
    /// Represents a discount usage class
    /// </summary>
    public class DiscountUsage : BaseEntity
    {
        /// <summary>
        /// Gets or sets the discount usage history Id
        /// </summary>
        public int DiscountUsageHistoryId { get; set; }

        /// <summary>
        /// Gets or sets the discount customer mapping ID
        /// </summary>
        public int? DiscountCustomerMappingId { get; set; }

        /// <summary>
        /// Gets or sets the Order Id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the Order item Id
        /// </summary>
        public int? OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the Discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }
    }
}
