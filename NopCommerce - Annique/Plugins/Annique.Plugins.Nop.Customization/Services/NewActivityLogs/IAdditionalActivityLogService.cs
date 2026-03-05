using Nop.Core.Domain.Customers;
using System.Threading.Tasks;
using static Annique.Plugins.Nop.Customization.Services.NewActivityLogs.AdditionalActivityLogService;

namespace Annique.Plugins.Nop.Customization.Services.NewActivityLogs
{
    public interface IAdditionalActivityLogService
    {
        TrackingCookie GetCustomerActivityTrackingCookie();

        Task InsertActivityTrackingLogAsync(string activityLogType, string activityName, Customer customer);

        TrackingCookie CreateAndGetTrackingCookieObject();
    }
}
