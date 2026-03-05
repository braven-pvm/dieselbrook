using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders
{
    public partial class CustomerOrderListModelDto : ModelDto
    {
        public IList<CustomerOrderDetailsModelDto> Orders { get; set; }

        public IList<RecurringOrderModelDto> RecurringOrders { get; set; }

        public IList<string> RecurringPaymentErrors { get; set; }

        #region Nested Classes

        public partial class CustomerOrderDetailsModelDto : ModelWithIdDto
        {
            public string CustomOrderNumber { get; set; }

            public string OrderTotal { get; set; }

            public bool IsReturnRequestAllowed { get; set; }

            public int OrderStatusEnum { get; set; }

            public string OrderStatus { get; set; }

            public string PaymentStatus { get; set; }

            public string ShippingStatus { get; set; }

            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }
}
