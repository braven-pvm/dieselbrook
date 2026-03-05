using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class AddProductReviewModelDto : ModelDto
    {
        public string Title { get; set; }

        public string ReviewText { get; set; }

        public int Rating { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool CanCurrentCustomerLeaveReview { get; set; }

        public bool SuccessfullyAdded { get; set; }

        public bool CanAddNewReview { get; set; }

        public string Result { get; set; }
    }
}
