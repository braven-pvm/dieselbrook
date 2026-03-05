using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class ProductDetailsAttributeChangeResponse : BaseDto
    {
        public int ProductId { get; set; }

        public string Gtin { get; set; }

        public string Mpn { get; set; }

        public string Sku { get; set; }

        public string Price { get; set; }

        public string BasePricePangv { get; set; }

        public string StockAvailability { get; set; }

        public int[] Enabledattributemappingids { get; set; }

        public int[] Disabledattributemappingids { get; set; }

        public string PictureFullSizeUrl { get; set; }

        public string PictureDefaultSizeUrl { get; set; }

        public bool IsFreeShipping { get; set; }

        public string[] Message { get; set; }
    }
}
