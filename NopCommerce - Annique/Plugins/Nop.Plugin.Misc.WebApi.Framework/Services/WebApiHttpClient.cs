using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.WebApi.Framework.Services
{
    /// <summary>
    /// Represents the HTTP client to request nopCommerce services
    /// </summary>
    public partial class WebApiHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public WebApiHttpClient(HttpClient client,
            IWebHelper webHelper)
        {
            //configure client
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CURRENT_VERSION}");

            _httpClient = client;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Notification about the successful installation
        /// </summary>
        /// <param name="plugin">Plugin descriptor</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the asynchronous task whose result contains the installation details
        /// </returns>
        public async Task<string> InstallationCompletedAsync(PluginDescriptor plugin)
        {
            var url = "https://www.nopcommerce.com/web-api-installation/";
            var requestContent = new StringContent($"url={_webHelper.GetStoreLocation()}&api={plugin.SystemName}&version={plugin.Version}",
                Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
            var response = await _httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        #endregion
    }
}
