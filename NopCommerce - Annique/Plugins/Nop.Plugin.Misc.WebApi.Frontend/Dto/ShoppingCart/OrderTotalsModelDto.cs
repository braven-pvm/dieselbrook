using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class OrderTotalsModelDto : ModelDto
    {
        public bool IsEditable { get; set; }

        public string SubTotal { get; set; }

        public string SubTotalDiscount { get; set; }

        public string Shipping { get; set; }

        public bool RequiresShipping { get; set; }

        public string SelectedShippingMethod { get; set; }

        public bool HideShippingTotal { get; set; }

        public string PaymentMethodAdditionalFee { get; set; }

        public string Tax { get; set; }

        public IList<TaxRateDto> TaxRates { get; set; }

        public bool DisplayTax { get; set; }

        public bool DisplayTaxRates { get; set; }

        public IList<GiftCardDto> GiftCards { get; set; }

        public string OrderTotalDiscount { get; set; }

        public int RedeemedRewardPoints { get; set; }

        public string RedeemedRewardPointsAmount { get; set; }

        public int WillEarnRewardPoints { get; set; }

        public string OrderTotal { get; set; }
    }
}
