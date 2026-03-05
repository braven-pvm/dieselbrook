using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Utilities
{
    public static class SettingHelpers
    {
        public static string ToCommaSeparatedString<T>(this IList<T> list)
        {
            return list == null ? string.Empty : string.Join(",", list);
        }
    }
}
