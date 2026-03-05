using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class EditAddressResponse<T> : BaseDto
         where T : ModelDto
    {
        public string Redirect { get; set; }
        public int SelectedId { get; set; }
        public UpdateSectionJsonModelDto<T> UpdateSectionModel { get; set; }        
    }
}
