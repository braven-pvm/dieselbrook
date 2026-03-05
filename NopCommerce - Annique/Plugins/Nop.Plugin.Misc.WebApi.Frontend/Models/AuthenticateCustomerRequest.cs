using Nop.Plugin.Misc.WebApi.Framework.Models;

namespace Nop.Plugin.Misc.WebApi.Frontend.Models
{
    public partial class AuthenticateCustomerRequest : AuthenticateRequest
    {
        public bool IsGuest { get; set; }
    }
}
