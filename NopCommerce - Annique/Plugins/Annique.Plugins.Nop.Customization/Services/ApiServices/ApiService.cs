using Annique.Plugins.Nop.Customization.Models.OTP;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ApiServices
{
    public class ApiService : IApiService
    {
        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ApiService(IHttpClientFactory httpClientFactory,
            ILogger logger,
            IWorkContext workContext)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _workContext = workContext;
        }

        #endregion

        #region Method

        //Get Api response
        public async Task<string> GetAPIResponse(string url)
        {
            var httpClient = _httpClientFactory.CreateClient();
            try
            {
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                if (response.Content == null)
                    return null;

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
                return null;
            }
        }

        //get API response with error message and status code

        public async Task<ApiResponse> GetAPIResponseAsync(string url)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(url);

                if (response.Content == null)
                    return new ApiResponse { Content = null, StatusCode = (int)response.StatusCode };

                var content = await response.Content.ReadAsStringAsync();

                return new ApiResponse { Content = content, StatusCode = (int)response.StatusCode };
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
                return new ApiResponse { Content = null, StatusCode = 500 };  // 500 for internal error
            }
        }

        public async Task<ApiResponse> PostAPIMethodAsync(string url, object payload, string apiKey)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.Default, MimeTypes.ApplicationJson);

                var baseUrl = new Uri(url);

                // Set the API key in the Authorization header
                httpClient.DefaultRequestHeaders.Add("api_key", apiKey);

                var response = await httpClient.PostAsync(baseUrl, content);

                if (response.Content == null)
                    return new ApiResponse { Content = null, StatusCode = (int)response.StatusCode };

                var responseContent = await response.Content.ReadAsStringAsync();

                return new ApiResponse { Content = responseContent, StatusCode = (int)response.StatusCode };
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
                return new ApiResponse { Content = null, StatusCode = 500 };
            }
        }

        #endregion
    }
}
