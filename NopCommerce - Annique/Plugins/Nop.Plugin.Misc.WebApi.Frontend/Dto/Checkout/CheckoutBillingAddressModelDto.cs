using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class CheckoutBillingAddressModelDto : ModelDto
    {
        public IList<AddressModelDto> ExistingAddresses { get; set; }
        public IList<AddressModelDto> InvalidExistingAddresses { get; set; }

        public AddressModelDto BillingNewAddress { get; set; }

        public bool ShipToSameAddress { get; set; }
        public bool ShipToSameAddressAllowed { get; set; }

        /// <summary>
        /// Used on one-page checkout page
        /// </summary>
        public bool NewAddressPreselected { get; set; }

        public bool EuVatEnabled { get; set; }
        public bool EuVatEnabledForGuests { get; set; }
        public string VatNumber { get; set; }
    }
}
