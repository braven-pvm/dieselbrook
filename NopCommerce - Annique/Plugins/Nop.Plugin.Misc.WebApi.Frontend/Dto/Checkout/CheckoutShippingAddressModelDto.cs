using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class CheckoutShippingAddressModelDto : ModelDto
    {
        public IList<AddressModelDto> ExistingAddresses { get; set; }
        public IList<AddressModelDto> InvalidExistingAddresses { get; set; }
        public AddressModelDto ShippingNewAddress { get; set; }
        public bool NewAddressPreselected { get; set; }

        public int SelectedBillingAddress { get; set; }
        public bool DisplayPickupInStore { get; set; }
        public CheckoutPickupPointsModelDto PickupPointsModel { get; set; }
    }
}
