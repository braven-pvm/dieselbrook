using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public partial class SetProductReviewHelpfulnessResponse : BaseDto
    {
        public string Result { get; set; }

        public int TotalYes { get; set; }

        public int TotalNo { get; set; }
    }
}
