using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Annique.Plugins.Nop.Customization.Services.Helper
{
    public class SearchSanitizationService : ISearchSanitizationService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger _logger;
        private HashSet<string> _cleanUpWords;

        public SearchSanitizationService(IWebHostEnvironment env, ILogger logger)
        {
            _env = env;
            _logger = logger;
            LoadCleanupWords();
        }

        private void LoadCleanupWords()
        {
            try
            {
                var path = Path.Combine(_env.ContentRootPath, "Plugins", "Annique.Customization", "Content", "keyword-cleanup-list.json");
                var json = File.ReadAllText(path);
                var words = JsonConvert.DeserializeObject<List<string>>(json);
                _cleanUpWords = new HashSet<string>(words ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.Error("Error loading stop words", ex);
            }
        }

        public HashSet<string> GetCleanupWords() => _cleanUpWords;

        public string SanitizeSearchQuery(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            var words = message
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => !_cleanUpWords.Contains(word.Trim().ToLowerInvariant()));

            return string.Join(" ", words);
        }
    }
}
