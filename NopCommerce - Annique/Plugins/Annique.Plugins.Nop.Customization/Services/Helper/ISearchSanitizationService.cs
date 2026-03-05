using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Services.Helper
{
    public interface ISearchSanitizationService
    {
        HashSet<string> GetCleanupWords();

        string SanitizeSearchQuery(string message);
    }
}
