using Annique.Plugins.Nop.Customization.Factories.AnniqueReport;
using Annique.Plugins.Nop.Customization.Services.AnniqueReport;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Controllers;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class PublicAnniqueReportController : BasePublicController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IAnniqueReportModelFactory _anniqueReportModelFactory;
        private readonly IAnniqueReportService _anniqueReportService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PublicAnniqueReportController(IAclService aclService,
            IAnniqueReportModelFactory anniqueReportModelFactory,
            IAnniqueReportService anniqueReportService,
            ICustomerService customerService,
            IWorkContext workContext)
        {
            _aclService = aclService;
            _anniqueReportModelFactory = anniqueReportModelFactory;
            _anniqueReportService = anniqueReportService;
            _customerService = customerService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        //My account / Reports
        public virtual async Task<IActionResult> CustomerReports()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = await _anniqueReportModelFactory.PrepareCustomerReportListModelAsync();
            return View(model);
        }

        //My account / report details page
        public virtual async Task<IActionResult> ReportDetails(int reportId)
        {
            var report = await _anniqueReportService.GetReportByIdAsync(reportId);
            if (report == null)
                return InvokeHttp404();

            var notAvailable = !report.Published ||
                //ACL (access control list)
                !await _aclService.AuthorizeAsync(report);

            if (notAvailable)
                return InvokeHttp404();

            var model = await _anniqueReportModelFactory.PrepareReportDetailsModelAsync(report);
            return View(model);
        }

        #endregion
    }
}
