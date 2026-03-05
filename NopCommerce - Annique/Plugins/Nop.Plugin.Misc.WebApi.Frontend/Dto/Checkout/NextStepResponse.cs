using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class NextStepResponse<T> : BaseDto
        where T : ModelDto
    {
        public UpdateSectionJsonModelDto<T> UpdateSectionModel { get; set; }

        public bool WrongBillingAddress { get; set; }

        public string GotoSection { get; set; }
    }
}
