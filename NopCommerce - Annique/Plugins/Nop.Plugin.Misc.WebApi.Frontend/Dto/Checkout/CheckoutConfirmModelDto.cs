using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class CheckoutConfirmModelDto : ModelDto
    {
        public bool TermsOfServiceOnOrderConfirmPage { get; set; }
        public bool TermsOfServicePopup { get; set; }
        public string MinOrderTotalWarning { get; set; }
        public ShoppingCartModelDto ShoppingCart { get; set; }
        public IList<string> Warnings { get; set; }
        public OrderTotalsModelDto OrderTotals { get; set; }
    }
}
