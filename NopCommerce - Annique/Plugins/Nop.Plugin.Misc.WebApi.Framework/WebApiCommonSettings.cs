using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.WebApi.Framework
{
    /// <summary>
    /// Represents settings of the Web API
    /// </summary>
    public class WebApiCommonSettings : ISettings
    {
        public bool DeveloperMode { get; set; }

        public string SecretKey { get; set; }

        public int TokenLifetimeDays { get; set; }
    }
}
