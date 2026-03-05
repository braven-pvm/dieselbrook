using Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ConsultantRegistrations
{
    public class ConsultantNewRegistrationService : IConsultantNewRegistrationService
    {
        #region Fields

        private readonly IRepository<RegistrationPageSettings> _registrationPageSettingsRepository;
        private readonly IRepository<NewRegistrations> _newRegistrationsRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;

        #endregion

        #region Ctor

        public ConsultantNewRegistrationService(IRepository<RegistrationPageSettings> registrationPageSettingsRepository,
            IRepository<NewRegistrations> newRegistrationsRepository,
            IHttpClientFactory httpClientFactory,
            ILogger logger,
            IStoreContext storeContext,
            ISettingService settingService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IStaticCacheManager staticCacheManager,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment)
        {
            _registrationPageSettingsRepository = registrationPageSettingsRepository;
            _newRegistrationsRepository = newRegistrationsRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settingService = settingService;
            _storeContext = storeContext;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _staticCacheManager = staticCacheManager;
            _hostingEnvironment = webHostEnvironment;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
            _localizationService = localizationService;
        }

        #endregion

        #region page settings

        /// <summary>
        /// Gets page setting
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of page settings
        /// </returns>
        public async Task<RegistrationPageSettings> GetPageSettings()
        {
            return await _registrationPageSettingsRepository.Table.FirstOrDefaultAsync() ?? new RegistrationPageSettings();
        }

        /// <summary>
        /// Insert page setting
        /// </summary>
        /// <param name="registrationPageSettings">Page Setting</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task creates page setting for consultant register page
        /// </returns>
        public async Task InsertPageSettings(RegistrationPageSettings registrationPageSettings)
        {
            await _registrationPageSettingsRepository.InsertAsync(registrationPageSettings);
        }

        /// <summary>
        /// Update page setting
        /// </summary>
        /// <param name="registrationPageSettings">Page Setting</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task updates page setting for consultant register page
        /// </returns>
        public async Task UpdatePageSettings(RegistrationPageSettings registrationPageSettings)
        {
            await _registrationPageSettingsRepository.UpdateAsync(registrationPageSettings);
        }

        #endregion

        #region Registration methods

        /// <summary>
        ///Return Nop based consultant registration is enable or disable
        /// </summary>
        public async Task<bool> IsNopBasedConsultantRegistrationEnabledAsync()
        {
            // Get active store
            var store = await _storeContext.GetCurrentStoreAsync();
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();
            if (!pluginEnable)
                return false;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                AnniqueCustomizationDefaults.NopBasedConsultantRegistrationKey, store.Id);

            // Try to get the result from the cache
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                // Get active store Annique settings
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                // Check if nop based consultant registration enabled
                return settings.IsNopConsultantRegistration;
            });
        }

        /// <summary>
        /// Gets all registration list
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the new consultant registration list
        /// </returns>
        public virtual async Task<IPagedList<NewRegistrations>> GetAllRegistrationsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var list = await _newRegistrationsRepository.GetAllPagedAsync(query =>
            {
                return query.OrderByDescending(l => l.CreatedOnUtc);
            }, pageIndex, pageSize);

            return list;
        }

        /// <summary>
        /// Get registration by id
        /// </summary>
        /// <param name="id">Resgistration id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task constains registration data get by id
        /// </returns>
        public async Task<NewRegistrations> GetRegistrationById(int id)
        {
            return await _newRegistrationsRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Insert new consultant registration
        /// </summary>
        /// <param name="newRegistrations">Resgistration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task creates new consultant registration
        /// </returns>
        public async Task InsertAsync(NewRegistrations newRegistrations)
        {
            await _newRegistrationsRepository.InsertAsync(newRegistrations);
        }

        /// <summary>
        /// update consultant registration
        /// </summary>
        /// <param name="newRegistrations">Resgistration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task updates consultant registration
        /// </returns>
        public async Task UpdateAsyc(NewRegistrations newRegistrations)
        {
            await _newRegistrationsRepository.UpdateAsync(newRegistrations);
        }

        #endregion

        #region Validation Method

        private async Task<ValidationResponse> BuildErrorResponse() =>
        new ValidationResponse
        {
            Status = "INVALID",
            Errors = new List<ValidationError>
            {
                new ValidationError { Rule = "System", Message = await _localizationService.GetResourceAsync("Consultant.Registration.FallbackError") }
            }
        };

        /// <summary>
        /// validate consultant registration data
        /// </summary>
        /// <param name="id">Resgistration id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task validates data using thrid party api
        /// </returns>
        public async Task<ValidationResponse> ValidateConsultantAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();

                //load settings
                var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                var token = anniqueSettings.RegistrationValidationApiKey;
                var baseUrl = anniqueSettings.RegistrationValidationApiEndPoint;

                var requestUrl = $"{baseUrl.TrimEnd('/')}/{id}";

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Assuming no payload required
                var content = new StringContent(string.Empty, Encoding.UTF8, MimeTypes.ApplicationJson);

                var apiResponse = await httpClient.PostAsync(requestUrl, content);
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                
                switch ((int)apiResponse.StatusCode)
                {
                    case 200:
                        return JsonConvert.DeserializeObject<ValidationResponse>(responseContent);

                    case 400:
                        return JsonConvert.DeserializeObject<ValidationResponse>(responseContent);

                    case 401:
                        await _logger.WarningAsync($"Validation API Unauthorized (401) for consultant {id}. Response: {responseContent}");
                        return await BuildErrorResponse();

                    case 404:
                        await _logger.WarningAsync($"Validation API Not Found (404) for consultant {id}. Response: {responseContent}");
                        return await BuildErrorResponse();

                    case 500:
                        await _logger.WarningAsync($"Validation API Server Error (500) for consultant {id}. Response: {responseContent}");
                        return await BuildErrorResponse();

                    default:
                        await _logger.WarningAsync($"Validation API Unexpected status {(int)apiResponse.StatusCode} for consultant {id}. Response: {responseContent}");
                        return await BuildErrorResponse();
                }
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync($"Validation API Exception: {ex.Message}", ex);
                return await BuildErrorResponse();
            }
        }

        /// <summary>
        /// Prepare new registrations
        /// </summary>
        /// <param name="model">Resgistration model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task prepares entity from model
        /// </returns>
        public Task<NewRegistrations> PrepareNewRegistrationsFromModel(ConsultantRegistrationModel model)
        {
            var entity = new NewRegistrations
            {
                Id = model.Id,
                cFname = model.FirstName,
                cLname = model.LastName,
                cEmail = model.Email,
                cPhone1 = model.Cell,
                cPhone2 = model.Whatsapp,
                ccountry = "South Africa",
                cZip = model.Postcode,
                cLanguage = model.SelectedLanguage,
                besttocall = model.SelectedCallTime,
                IPAddress = _webHelper.GetCurrentIpAddress(),
                Browser = model.Browser,
                CreatedOnUtc = DateTime.UtcNow,
                csponsor = model.Csponser,
            };

            return Task.FromResult(entity);
        }
        #endregion

        #region Render css , js and html methods

        /// <summary>
        /// Gets all file urls 
        /// </summary>
        /// <returns>
        /// The task result contains the file urls and updated report configuration block
        /// </returns>
        public string[] ExtractFileUrlsFromFirstLine(string reportBlock, string fileType, out string updatedBlock)
        {
            //base url of site
            var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            //split lines in report block
            string[] lines = reportBlock.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 0)
            {
                //take only firstline for [] with file name searching
                string firstLine = lines[0].Trim();

                //search [] within first line
                if (firstLine.StartsWith("[") && firstLine.EndsWith("]"))
                {
                    //comma seperated file list within []
                    string fileList = firstLine.Trim('[', ']');
                    string[] fileNames = fileList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    List<string> fileUrls = new();

                    foreach (string fileName in fileNames)
                    {
                        string trimmedFileName = fileName.Trim();

                        //filetype can be .css || .js || .html
                        if (trimmedFileName.EndsWith(fileType))
                        {
                            //remove unwanted spaces from filename
                            string encodedFileName = trimmedFileName.Replace(fileType, "").Trim() + fileType;

                            //prepare file url
                            string fileUrl = $"{baseUrl}/assets/{fileType.TrimStart('.')}/{encodedFileName}";
                            fileUrls.Add(fileUrl);
                        }
                    }

                    // skip the first line ['filenames'] since files are imported
                    reportBlock = string.Join(Environment.NewLine, lines.Skip(1));

                    // Update the Model.CustomJS property
                    updatedBlock = reportBlock;

                    return fileUrls.ToArray();
                }
            }
            // No files found, keep the report block as it is
            updatedBlock = reportBlock;
            return Array.Empty<string>();
        }

        #endregion
    }
}
