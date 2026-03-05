using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Usermaven.Models;

public class ConfigurationModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Usermaven.Configuration.Fields.EnablePlugin")]
    public bool EnablePlugin { get; set; }
    public bool EnablePlugin_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Usermaven.Configuration.Fields.Script")]
    public string Script { set; get; }
    public bool Script_OverrideForStore { set; get; }
}
