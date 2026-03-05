using Annique.Plugins.Nop.Customization.Filters;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    [WwwRequirement]
    [CheckLanguageSeoCode]
    [CheckAccessPublicStore]
    [CheckAccessClosedStore]
    [CheckDiscountCoupon]
    [CheckCustomAffiliate]
    public partial class BaseCustomPublicController : BasePublicController
    {
        protected override IActionResult InvokeHttp404()
        {
            Response.StatusCode = 404;
            return new EmptyResult();
        }
    }
}
