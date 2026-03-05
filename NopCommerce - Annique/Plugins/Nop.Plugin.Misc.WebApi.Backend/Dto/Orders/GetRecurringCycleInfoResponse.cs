using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class GetRecurringCycleInfoResponse : BaseDto
    {
        /// <summary>
        /// The error (if exists); otherwise, empty string
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Cycle length
        /// </summary>
        public int CycleLength { get; set; }

        /// <summary>
        /// Cycle period
        /// </summary>
        public RecurringProductCyclePeriod CyclePeriod { get; set; }

        /// <summary>
        /// Total cycles
        /// </summary>
        public int TotalCycles { get; set; }
    }
}
