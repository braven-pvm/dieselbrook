using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class CheckGiftCardBalanceModelDto : ModelDto
    {
        public string Result { get; set; }

        public string Message { get; set; }

        public string GiftCardCode { get; set; }
    }
}
