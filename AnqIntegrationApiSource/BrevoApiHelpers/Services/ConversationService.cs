using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

using BrevoApiHelpers.Models;
namespace BrevoApiHelpers.Services;


public class ConversationService : IConversationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(IHttpClientFactory httpClientFactory, ILogger<ConversationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("BrevoClient");
        _logger = logger;
    }

    public async Task<List<Conversation>> GetConversationsAsync(DateTime from, DateTime to)
    {
        try
        {
            string query = $"conversations/messages?startDate={from:yyyy-MM-ddTHH:mm:ssZ}&endDate={to:yyyy-MM-ddTHH:mm:ssZ}";
            var response = await _httpClient.GetAsync(query);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            return json["conversations"]?.Select(c => new Conversation
            {
                Id = c["_id"]?.ToString(),
                Subject = c["subject"]?.ToString()
            }).ToList() ?? new List<Conversation>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversations");
            return new List<Conversation>();
        }
    }
}