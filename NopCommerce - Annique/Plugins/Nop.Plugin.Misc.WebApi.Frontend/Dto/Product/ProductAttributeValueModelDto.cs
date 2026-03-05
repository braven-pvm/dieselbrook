using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public partial class ProductAttributeValueModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string ColorSquaresRgb { get; set; }

        //picture model is used with "image square" attribute type
        public PictureModelDto ImageSquaresPictureModel { get; set; }

        public string PriceAdjustment { get; set; }

        public bool PriceAdjustmentUsePercentage { get; set; }

        public decimal PriceAdjustmentValue { get; set; }

        public bool IsPreSelected { get; set; }

        //product picture ID (associated to this value)
        public int PictureId { get; set; }

        public bool CustomerEntersQty { get; set; }

        public int Quantity { get; set; }
    }
}
