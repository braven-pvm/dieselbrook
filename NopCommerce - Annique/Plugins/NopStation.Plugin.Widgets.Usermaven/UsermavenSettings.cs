using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Usermaven;

public class UsermavenSettings : ISettings
{
    public bool EnablePlugin { get; set; }

    public string Script { get; set; }
}