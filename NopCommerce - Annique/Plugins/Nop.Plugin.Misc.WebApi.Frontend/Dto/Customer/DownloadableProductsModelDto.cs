using System;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class DownloadableProductsModelDto : ModelDto
    {
        public Guid OrderItemGuid { get; set; }

        public int OrderId { get; set; }

        public string CustomOrderNumber { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public string ProductAttributes { get; set; }

        public int DownloadId { get; set; }

        public int LicenseId { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
