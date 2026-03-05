using System.Threading.Tasks;
using Nop.Plugin.Misc.WebApi.Framework.Models;

namespace Nop.Plugin.Misc.WebApi.Backend.Services
{
    public interface IAuthorizationAdminService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);
    }
}
