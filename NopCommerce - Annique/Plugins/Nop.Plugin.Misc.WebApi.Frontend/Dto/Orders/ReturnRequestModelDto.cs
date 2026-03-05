using System;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders
{
    public partial class ReturnRequestModelDto : ModelWithIdDto
    {
        public string CustomNumber { get; set; }

        public string ReturnRequestStatus { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public int Quantity { get; set; }

        public string ReturnReason { get; set; }

        public string ReturnAction { get; set; }

        public string Comments { get; set; }

        public Guid UploadedFileGuid { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
