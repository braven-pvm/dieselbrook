using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class PasswordRecoveryConfirmModelDto : ModelDto
    {
        public string NewPassword { get; set; }

        public string ConfirmNewPassword { get; set; }

        public bool DisablePasswordChanging { get; set; }

        public string Result { get; set; }

        public string ReturnUrl { get; set; }
    }
}
