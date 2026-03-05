using BrevoApiHelpers.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BrevoApiHelpers.Services
{
    public class DealService : IDealService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DealService> _logger;

        public DealService(IHttpClientFactory httpClientFactory, ILogger<DealService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("BrevoClient");
            _logger = logger;
        }

        public async Task<BrevoEmailResponse> CreateDealAsync(string contactEmail, DealModel deal)
        {
            try
            {
                var payload = new
                {
                    name = deal.Name,
                    attributes = deal.Attributes,
                    contactsIds = new[] { contactEmail }
                };

                var response = await _httpClient.PostAsJsonAsync("crm/deals", payload);
                var content = await response.Content.ReadAsStringAsync();

                return new BrevoEmailResponse
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Error = response.IsSuccessStatusCode ? null : content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deal for {email}", contactEmail);
                return new BrevoEmailResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<BrevoEmailResponse> UpdateDealAsync(string dealId, DealModel deal)
        {
            try
            {
                var payload = new
                {
                    name = deal.Name,
                    attributes = deal.Attributes
                };

                var response = await _httpClient.PatchAsync($"crm/deals/{dealId}", JsonContent.Create(payload));
                var content = await response.Content.ReadAsStringAsync();

                return new BrevoEmailResponse
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Error = response.IsSuccessStatusCode ? null : content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating deal {dealId}", dealId);
                return new BrevoEmailResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }
    }
}