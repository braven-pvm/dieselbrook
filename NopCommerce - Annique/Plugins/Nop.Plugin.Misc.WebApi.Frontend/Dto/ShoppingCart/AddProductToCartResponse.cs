using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class AddProductToCartResponse : BaseDto
    {
        public IList<string> Errors { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }

        public MiniShoppingCartModelDto Model { get; set; }
    }
}
