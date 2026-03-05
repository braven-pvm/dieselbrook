using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders
{
    public partial class RecurringOrderModelDto : ModelWithIdDto
    {
        public string StartDate { get; set; }

        public string CycleInfo { get; set; }

        public string NextPayment { get; set; }

        public int TotalCycles { get; set; }

        public int CyclesRemaining { get; set; }

        public int InitialOrderId { get; set; }

        public bool CanRetryLastPayment { get; set; }

        public string InitialOrderNumber { get; set; }

        public bool CanCancel { get; set; }
    }
}
