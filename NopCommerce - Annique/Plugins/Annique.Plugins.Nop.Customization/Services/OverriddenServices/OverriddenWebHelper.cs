using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;
using Nop.Core;
using System;
using System.Linq;
using System.Net;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    public class OverriddenWebHelper : WebHelper
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public OverriddenWebHelper(IActionContextAccessor actionContextAccessor,
            IHostApplicationLifetime hostApplicationLifetime,
            IHttpContextAccessor httpContextAccessor, 
            IUrlHelperFactory urlHelperFactory,
            Lazy<IStoreContext> storeContext) : base(actionContextAccessor, hostApplicationLifetime, httpContextAccessor, urlHelperFactory, storeContext)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Method

        public override string GetCurrentIpAddress()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            #region Task 644 new activity logs

            // Check for Cloudflare's real IP header (CF-Connecting-IP) and store real ip of customer
            var headerIp = _httpContextAccessor.HttpContext?.Request?.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerIp))
                return headerIp;

            #endregion

            // Fallback to remote IP address (connection directly or through proxy)
            if (_httpContextAccessor.HttpContext.Connection?.RemoteIpAddress is not IPAddress remoteIp)
                return string.Empty;

            if (remoteIp.Equals(IPAddress.IPv6Loopback))
                return IPAddress.Loopback.ToString();

            return remoteIp.MapToIPv4().ToString();
        }

        #endregion
    }
}
