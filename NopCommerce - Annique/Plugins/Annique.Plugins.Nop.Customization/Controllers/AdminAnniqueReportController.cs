using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Annique.Plugins.Nop.Customization.Factories.AnniqueReport;
using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin;
using Annique.Plugins.Nop.Customization.Services.AnniqueReport;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class AdminAnniqueReportController : BaseAdminController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IAnniqueReportService _anniqueReportService;
        private readonly IAnniqueReportModelFactory _anniqueReportModelFactory;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IReportParameterService _reportParameterService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public AdminAnniqueReportController(IAclService aclService,
            IAnniqueReportService anniqueReportService,
            IAnniqueReportModelFactory anniqueReportModelFactory,
            ICustomerService customerService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IReportParameterService reportParameterService,
            IUrlRecordService urlRecordService)
        {
            _aclService = aclService;
            _anniqueReportService = anniqueReportService;
            _anniqueReportModelFactory = anniqueReportModelFactory;
            _customerService = customerService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _reportParameterService = reportParameterService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveReportAclAsync(Report report, ReportModel model)
        {
            report.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _anniqueReportService.UpdateReportAsync(report);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(report);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                        await _aclService.InsertAclRecordAsync(report, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        #endregion

        #region Report List / Create / Edit / Delete

        public virtual async Task<IActionResult> List()
        {
            //prepare model
            var model = await _anniqueReportModelFactory.PrepareReportSearchModelAsync(new ReportSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ReportSearchModel searchModel)
        {
            //prepare model
            var model = await _anniqueReportModelFactory.PrepareReportListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            //prepare model
            var model = await _anniqueReportModelFactory.PrepareReportModelAsync(new ReportModel(), new Report());

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ReportModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var report = _anniqueReportModelFactory.PrepareReportFields(model);
                report.CreatedOnUtc = DateTime.UtcNow;
                report.UpdatedOnUtc = DateTime.UtcNow;
                await _anniqueReportService.InsertReportAsync(report);

                #region task 629 New Features on Annique Reports

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(report, model.SeName, report.Name , true);
                await _urlRecordService.SaveSlugAsync(report, model.SeName, 0);

                #endregion

                //ACL (customer roles)
                await SaveReportAclAsync(report, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.AnniqueReports.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = report.Id });
            }
            //prepare model
            model = await _anniqueReportModelFactory.PrepareReportModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            //try to get a report with the specified id
            var report = await _anniqueReportService.GetReportByIdAsync(id);
            if (report == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _anniqueReportModelFactory.PrepareReportModelAsync(null, report);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ReportModel model, bool continueEditing)
        {
            //try to get a report with the specified id
            var report = await _anniqueReportService.GetReportByIdAsync(model.Id);
            if (report == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var updateReport = _anniqueReportModelFactory.PrepareReportFields(model);
                updateReport.UpdatedOnUtc = DateTime.UtcNow;
                await _anniqueReportService.UpdateReportAsync(updateReport);
                //ACL
                await SaveReportAclAsync(updateReport, model);

                #region task 629 New Features on Annique Reports

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(updateReport, model.SeName, updateReport.Name, true);
                await _urlRecordService.SaveSlugAsync(updateReport, model.SeName, 0);

                #endregion

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.AnniqueReports.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = updateReport.Id });
            }

            //prepare model
            model = await _anniqueReportModelFactory.PrepareReportModelAsync(model, report, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            //try to get a report with the specified id
            var report = await _anniqueReportService.GetReportByIdAsync(id);
            if (report == null)
                return RedirectToAction("List");

            await _anniqueReportService.DeleteReportAsync(report);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.AnniqueReports.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> ReportDelete(int id)
        {
            //try to get a report with the specified id
            var report = await _anniqueReportService.GetReportByIdAsync(id);
            if (report == null)
                return RedirectToAction("List");

            await _anniqueReportService.DeleteReportAsync(report);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.AnniqueReports.Deleted"));

            return new NullJsonResult();
        }

        #endregion

        #region Report Parameter List / Create / Edit / Delete

        [HttpPost]
        public virtual async Task<IActionResult> ParameterList(ReportParameterSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var report = await _anniqueReportService.GetReportByIdAsync(searchModel.ReportId);
            if (report == null)
                return NotFound();

            var model = await _anniqueReportModelFactory.PrepareReportParameterListModelAsync(searchModel, report);

            return Json(model);
        }

        public virtual async Task<IActionResult> ParameterCreate(int reportId)
        {
            var report = await _anniqueReportService.GetReportByIdAsync(reportId);
            if (report == null)
                return NotFound();

            var model = await _anniqueReportModelFactory.PrepareReportParameterModelAsync(new ReportParameterModel(),
                    new ReportParameter(), report.Id);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ParameterCreate(ReportParameterModel model)
        {
            if (ModelState.IsValid)
            {
                var reportParameter = _anniqueReportModelFactory.PrepareReportParameterFields(model);

                await _reportParameterService.InsertReportParamterAsync(reportParameter);

                ViewBag.RefreshPage = true;
            }
            return View(model);
        }

        public virtual async Task<IActionResult> ParameterEdit(int id)
        {
            var reportParameter = await _reportParameterService.GetReportParameterByIdAsync(id);
            if (reportParameter == null)
                return NotFound();

            var model = await _anniqueReportModelFactory.PrepareReportParameterModelAsync(null, reportParameter,
                    reportParameter.ReportId);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ParameterEdit(ReportParameterModel model)
        {
            if (ModelState.IsValid)
            {
                var reportParameter = _anniqueReportModelFactory.PrepareReportParameterFields(model);

                await _reportParameterService.UpdateReportParameterAsync(reportParameter);

                ViewBag.RefreshPage = true;
            }
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ParameterDelete(int id)
        {
            var reportParameter = await _reportParameterService.GetReportParameterByIdAsync(id);
            if (reportParameter == null)
                return NotFound();

            await _reportParameterService.DeleteReportParameterAsync(reportParameter);

            return new NullJsonResult();
        }

        #endregion

        #region Parameter Value List / Create / Edit / Delete

        [HttpPost]
        public virtual async Task<IActionResult> ParameterValueList(ReportParameterValueSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var reportParameter = await _reportParameterService.GetReportParameterByIdAsync(searchModel.ReportParameterId);
            if (reportParameter == null)
                return NotFound();

            var model = await _anniqueReportModelFactory.PrepareReportParameterValueListModelAsync(searchModel, reportParameter);

            return Json(model);
        }

        public virtual async Task<IActionResult> ParameterValueCreate(int reportParameterId)
        {
            var reportParameter = await _reportParameterService.GetReportParameterByIdAsync(reportParameterId);
            if (reportParameter == null)
                return NotFound();

            var model = _anniqueReportModelFactory.PrepareReportParameterValueModel(new ReportParameterValueModel(),
                new ReportParameterValue(), reportParameter.Id);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ParameterValueCreate(ReportParameterValueModel model)
        {
            if (ModelState.IsValid)
            {
                var parameterValues = _anniqueReportModelFactory.PrepareReportParameterValueFields(model);

                await _reportParameterService.InsertReportParameterValueAsync(parameterValues);

                ViewBag.RefreshPage = true;
            }

            return View(model);
        }

        public virtual async Task<IActionResult> ParameterValueEdit(int id)
        {
            var parameterValue = await _reportParameterService.GetReportParameterValueByIdAsync(id);
            if (parameterValue == null)
                return NotFound();

            var model = _anniqueReportModelFactory.PrepareReportParameterValueModel(null, parameterValue,
                parameterValue.ReportParameterId);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ParameterValueEdit(ReportParameterValueModel model)
        {
            if (ModelState.IsValid)
            {
                var parameterValue = _anniqueReportModelFactory.PrepareReportParameterValueFields(model);

                await _reportParameterService.UpdateReportParameterValueAsync(parameterValue);

                ViewBag.RefreshPage = true;
            }

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ParameterValueDelete(int id)
        {
            var parameterValue = await _reportParameterService.GetReportParameterValueByIdAsync(id);
            if (parameterValue == null)
                return NotFound();

            await _reportParameterService.DeleteReportParameterValueAsync(parameterValue);

            return new NullJsonResult();
        }

        #endregion
    }
}
