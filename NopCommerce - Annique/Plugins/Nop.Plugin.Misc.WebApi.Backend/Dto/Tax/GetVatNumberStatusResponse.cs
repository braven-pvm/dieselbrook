using Nop.Core.Domain.Tax;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Tax
{
    public partial class GetVatNumberStatusResponse : BaseDto
    {
        public GetVatNumberStatusResponse((VatNumberStatus vatNumberStatus, string name, string address) response)
        {
            VatNumberStatus = response.vatNumberStatus;
            Name = response.name;
            Address = response.address;
        }

        /// <summary>
        /// The VAT number status enumeration
        /// </summary>
        public VatNumberStatus VatNumberStatus { get; set; }

        /// <summary>
        /// Company name
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Address
        /// </summary>
        public string Address { get; set; }
    }
}
