using Annique.Plugins.Nop.Customization.Models.OTP;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ApiServices
{
    public interface IApiService
    {
        //Get Api response
        Task<string> GetAPIResponse(string url);

        //get Api reponse with error message and status code
        Task<ApiResponse> GetAPIResponseAsync(string url);

        //post api method
        Task<ApiResponse> PostAPIMethodAsync(string url, object payload, string apiKey);
    }
}
