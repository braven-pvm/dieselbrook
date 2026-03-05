using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Usermaven.Models;

namespace NopStation.Plugin.Widgets.Usermaven.Components;

public class UsermavenViewComponent : NopStationViewComponent
{
    #region Fields

    private readonly UsermavenSettings _usermavenSettings;

    #endregion

    #region Ctor

    public UsermavenViewComponent(UsermavenSettings usermavenSettings)
    {
        _usermavenSettings = usermavenSettings;
    }

    #endregion

    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (!_usermavenSettings.EnablePlugin)
            return Content("");

        var model = new PublicInfoModel()
        {
            Script = _usermavenSettings.Script
        };

        return View("~/Plugins/NopStation.Plugin.Widgets.Usermaven/Views/PublicInfo.cshtml", model);
    }
}