using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class AccountActivationModelDto : ModelDto
    {
        public string Result { get; set; }

        public string ReturnUrl { get; set; }
    }
}
