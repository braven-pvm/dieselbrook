using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class AssociatedExternalAuthModelDto : ModelWithIdDto
    {
        public string Email { get; set; }

        public string ExternalIdentifier { get; set; }

        public string AuthMethodName { get; set; }
    }
}
