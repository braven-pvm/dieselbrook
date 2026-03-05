using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    /// <summary>
    /// Represents an order average report line
    /// </summary>
    public partial class OrderAverageReportLineResponse : BaseDto
    {
        /// <summary>
        /// Gets or sets the count
        /// </summary>
        public int CountOrders { get; set; }

        /// <summary>
        /// Gets or sets the shipping summary (excluding tax)
        /// </summary>
        public decimal SumShippingExclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment fee summary (excluding tax)
        /// </summary>
        public decimal OrderPaymentFeeExclTaxSum { get; set; }

        /// <summary>
        /// Gets or sets the tax summary
        /// </summary>
        public decimal SumTax { get; set; }

        /// <summary>
        /// Gets or sets the order total summary
        /// </summary>
        public decimal SumOrders { get; set; }

        /// <summary>
        /// Gets or sets the refunded amount summary
        /// </summary>
        public decimal SumRefundedAmount { get; set; }
    }
}
