using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.BackInStockSubscription
{
    public partial class CustomerBackInStockSubscriptionsModelDto : ModelDto
    {
        public IList<BackInStockSubscriptionModelDto> Subscriptions { get; set; }

        public PagerModelDto PagerModel { get; set; }

        #region Nested classes

        public partial class BackInStockSubscriptionModelDto : ModelWithIdDto
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string SeName { get; set; }
        }

        #endregion
    }
}
