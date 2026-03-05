using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class GiftCardBoxModelDto : ModelDto
    {
        public bool Display { get; set; }

        public string Message { get; set; }

        public bool IsApplied { get; set; }
    }
}