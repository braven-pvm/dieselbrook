using Nop.Core;
using Nop.Core.Domain.Discounts;
using System;

namespace Annique.Plugins.Nop.Customization.Domain.DiscountAllocation
{
    /// <summary>
    /// Represents a discount-customer mapping class
    /// </summary>
    public class DiscountCustomerMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        public int DiscountId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the discount mapping is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the discount start date and time
        /// </summary>
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the discount end date and time
        /// </summary>
        public DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the discount limitation identifier
        /// </summary>
        public int DiscountLimitationId { get; set; }

        /// <summary>
        /// Gets or sets the number of times discount used
        /// </summary>
        public int NoTimesUsed { get; set; }

        /// <summary>
        /// Gets or sets the discount limitation times (used when Limitation is set to "N Times Only" or "N Times Per Customer")
        /// </summary>
        public int LimitationTimes { get; set; }

        /// <summary>
        /// Gets or sets the discount limitation
        /// </summary>
        public DiscountLimitationType DiscountLimitation
        {
            get => (DiscountLimitationType)DiscountLimitationId;
            set => DiscountLimitationId = (int)value;
        }

        /// <summary>
        /// Gets or sets the notified flag for notified via inbox
        /// </summary>
        public bool Notified { get; set; }

        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the notify on whatsapp
        /// </summary>
        public bool NotifyWhatsApp { get; set; }
    }
}
