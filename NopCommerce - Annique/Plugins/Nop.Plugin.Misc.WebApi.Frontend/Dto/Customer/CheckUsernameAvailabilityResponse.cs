using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class CheckUsernameAvailabilityResponse : BaseDto
    {
        public bool Available { get; set; }

        public string Text { get; set; }
    }
}
