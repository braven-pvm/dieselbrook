using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class UpdateSectionJsonModelDto<T> : BaseDto
        where T : ModelDto
    {
        public string Name { get; set; }
        public string ViewName { get; set; }
        public T Model { get; set; }
    }
}
