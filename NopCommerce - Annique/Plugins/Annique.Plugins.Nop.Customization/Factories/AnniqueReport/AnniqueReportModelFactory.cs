using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin;
using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Public;
using Annique.Plugins.Nop.Customization.Services.AnniqueReport;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Routing;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.AnniqueReport
{
    /// <summary>
    /// Represents the Annique Report Model Factory 
    /// </summary>
    public class AnniqueReportModelFactory : IAnniqueReportModelFactory
    {
        #region Fields

        private readonly IAnniqueReportService _anniqueReportService;
        private readonly IReportParameterService _reportParameterService;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly INopUrlHelper _nopUrlHelper;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;
        private readonly IAffiliateService _affiliateService;
        private readonly IAddressService _addressService;

        #endregion

        #region Ctor

        public AnniqueReportModelFactory(IAnniqueReportService anniqueReportService,
            IReportParameterService reportParameterService,
            IAclSupportedModelFactory aclSupportedModelFactory,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            IUrlRecordService  urlRecordService,
            INopUrlHelper nopUrlHelper,
            IWebHelper webHelper,
            ICustomerService customerService,
            IAffiliateService affiliateService,
            IAddressService addressService)

        {
            _anniqueReportService = anniqueReportService;
            _reportParameterService = reportParameterService;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _localizationService = localizationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _urlRecordService = urlRecordService;
            _nopUrlHelper = nopUrlHelper;
            _webHelper = webHelper; 
            _customerService = customerService;
            _affiliateService = affiliateService;
            _addressService = addressService;
        }

        #endregion

        #region utility

        public ReportParameterValueSearchModel PrepareReportParameterValueSearchModel(ReportParameterValueSearchModel searchModel, ReportParameter reportParameter)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.ReportParameterId = reportParameter.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods for Admin Side

        #region Report Model method

        /// <summary>
        /// Prepare report search model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the report search model
        /// </returns>
        public Task<ReportSearchModel> PrepareReportSearchModelAsync(ReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged Report list model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report list model
        /// </returns>
        public async Task<ReportListModel> PrepareReportListModelAsync(ReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get reports
            var reports = await _anniqueReportService.GetAllReportsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new ReportListModel().PrepareToGrid(searchModel, reports, () =>
            {
                //fill in model values from the entity
                return reports.Select(report =>
                {
                    var reportModel = new ReportModel
                    {
                        Id = report.Id,
                        Name = report.Name,
                        CustomCSS = report.CustomCSS,
                        CustomJS = report.CustomJS,
                        TemplateBlock = report.TemplateBlock,
                        CustomText = report.CustomText,
                        Published = report.Published,
                        DisplayOrder = report.DisplayOrder,
                        IsMenuOption = report.IsMenuOption,
                        TabOrder = report.TabOrder,
                        IncludeReportCommonJs = report.IncludeReportCommonJs,   
                        IncludeReportParameters = report.IncludeReportParameters,
                    };
                    return reportModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare Report model
        /// </summary>
        /// <param name="model">Report model</param>
        /// <param name="report">Report</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report model
        /// </returns>
        public async Task<ReportModel> PrepareReportModelAsync(ReportModel model,
            Report report, bool excludeProperties = false)
        {
            if (report != null)
            {
                model ??= new ReportModel();
                model.Id = report.Id;
                model.Name = report.Name;
                model.CustomCSS = report.CustomCSS;
                model.CustomJS = report.CustomJS;
                model.TemplateBlock = report.TemplateBlock;
                model.CustomText = report.CustomText;
                model.Published = report.Published;
                model.DisplayOrder = report.DisplayOrder;

                #region task 629 New Features on Annique Reports

                model.IncludeReportCommonJs = report.IncludeReportCommonJs;
                model.IncludeReportParameters = report.IncludeReportParameters;
             
                // Get the SeName once
                var seName = await _urlRecordService.GetSeNameAsync(report, 0, true, false);
                model.SeName = seName;

                // If SeName is not null or empty, generate the URL
                if (!string.IsNullOrEmpty(seName))
                {
                    model.Url = await _nopUrlHelper
                        .RouteGenericUrlAsync<Report>(new { SeName = seName }, _webHelper.GetCurrentRequestProtocol());
                }
                else
                {
                    // Handle cases where SeName is null or empty, if needed (optional)
                    model.Url = string.Empty; 
                }

                model.AvailableHiddenFields = new List<SelectListItem>
                {
                    new("Customer ID", "CustomerId"),
                    new("Customer Roles", "CustomerRoles"),
                    new("Username", "Username"),
                    new("Customer Name", "CustomerName"),
                    new("Affiliate ID", "AffiliateId"),
                    new("Affiliate Name", "AffiliateName"),
                    new("Affiliate Friendly Name", "AffiliateFriendlyName")
                };

                model.SelectedHiddenFields = report.HiddenFields?.Split(',').ToList() ?? new();

                #endregion
            }

            if (report.IsMenuOption)
            {
                model.IsMenuOption = report.IsMenuOption;
                model.TabOrder = report.TabOrder;
            }

            model.PubliclyHostedPage = report.PubliclyHostedPage;

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, report, excludeProperties);

            PrepareReportParameterSearchModel(model.ReportParameterSearchModel, report);
            return model;
        }

        /// <summary>
        /// Prepare Report table Fields
        /// </summary>
        /// <param name="model">Report model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report table fields
        /// </returns>
        public Report PrepareReportFields(ReportModel model)
        {
            if (model != null)
            {
                var report = new Report()
                {
                    Id = model.Id,
                    Name = model.Name,
                    CustomCSS = model.CustomCSS,
                    CustomJS = model.CustomJS,
                    TemplateBlock = model.TemplateBlock,
                    CustomText = model.CustomText,
                    Published = model.Published,
                    DisplayOrder = model.DisplayOrder,
                    IsMenuOption = model.IsMenuOption,
                };

                #region task 629 New Features on Annique Reports

                report.IncludeReportCommonJs = model.IncludeReportCommonJs;
                report.IncludeReportParameters = model.IncludeReportParameters;

                string selectedHiddenFields = string.Empty;

                if (model.SelectedHiddenFields == null)
                {
                    report.HiddenFields = selectedHiddenFields;
                }
                else
                {
                    foreach (var item in model.SelectedHiddenFields)
                    {
                        selectedHiddenFields += item + ",";
                    }
                    report.HiddenFields = selectedHiddenFields.TrimEnd(',');
                }

                #endregion

                if (!model.IsMenuOption)
                    report.TabOrder = 0;
                else
                    report.TabOrder = model.TabOrder;

                report.PubliclyHostedPage = model.PubliclyHostedPage;

                return report;
            }
            return null;
        }

        #endregion

        #region Report Parameter Model Methods

        /// <summary>
        /// Prepare report parameter search model
        /// </summary>
        /// <param name="searchModel">Report Parametersearch model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the report Parameter search model
        /// </returns>
        public ReportParameterSearchModel PrepareReportParameterSearchModel(ReportParameterSearchModel searchModel, Report report)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.ReportId = report.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged Report Parameter list model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <param name="report">Report</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter list model
        /// </returns>
        public async Task<ReportParameterListModel> PrepareReportParameterListModelAsync(ReportParameterSearchModel searchModel, Report report)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (report == null)
                throw new ArgumentNullException(nameof(report));

            var parameters = await _reportParameterService.GetAllReportParametersAsync(report.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ReportParameterListModel().PrepareToGridAsync(searchModel, parameters, () =>
            {
                return parameters.SelectAwait(async parameter =>
                {
                    var reportParameterModel = new ReportParameterModel
                    {
                        Id = parameter.Id,
                        ReportId = parameter.ReportId,
                        Name = parameter.Name,
                        DisplayOrder = parameter.DisplayOrder,
                        AttributeControlTypeId = parameter.AttributeControlTypeId
                    };
                    reportParameterModel.AttributeControlTypeName = await _localizationService.GetLocalizedEnumAsync(parameter.AttributeControlType);

                    return reportParameterModel;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare Report Parameter model
        /// </summary>
        /// <param name="model">Report Parameter model</param>
        /// <param name="reportParameter">Report Parameter</param>
        /// <param name="reportId">Report identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter model
        /// </returns>
        public async Task<ReportParameterModel> PrepareReportParameterModelAsync(ReportParameterModel model,
                ReportParameter reportParameter, int reportId)
        {
            var report = await _anniqueReportService.GetReportByIdAsync(reportId);

            if (reportParameter != null)
            {
                model = model ?? new ReportParameterModel();

                model.Id = reportParameter.Id;
                model.ReportId = report.Id;
                model.Name = reportParameter.Name;
                model.AttributeControlTypeId = reportParameter.AttributeControlTypeId;
                model.AttributeControlTypeName = await _localizationService.GetLocalizedEnumAsync(reportParameter.AttributeControlType);
                model.DisplayOrder = reportParameter.DisplayOrder;
            }

            PrepareReportParameterValueSearchModel(model.ReportParameterValueSearchModel, reportParameter);

            return model;
        }

        /// <summary>
        /// Prepare Report Parameter table Fields
        /// </summary>
        /// <param name="model">Report Parameter model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter table fields
        /// </returns>
        public ReportParameter PrepareReportParameterFields(ReportParameterModel model)
        {
            if (model != null)
            {
                var reportParameter = new ReportParameter()
                {
                    Id = model.Id,
                    ReportId = model.ReportId,
                    Name = model.Name,
                    DisplayOrder = model.DisplayOrder,
                    AttributeControlTypeId = model.AttributeControlTypeId
                };
                return reportParameter;
            }
            return null;
        }

        #endregion

        #region Report Parameter Value Model Methods

        /// <summary>
        /// Prepare paged Report Parameter Value list model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter list model
        /// </returns>
        public async Task<ReportParameterValueListModel> PrepareReportParameterValueListModelAsync(ReportParameterValueSearchModel searchModel,
                ReportParameter reportParameter)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (reportParameter == null)
                throw new ArgumentNullException(nameof(reportParameter));

            var parameterValues = await _reportParameterService.GetReportParameterValuesAsync(reportParameter.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new ReportParameterValueListModel().PrepareToGrid(searchModel, parameterValues, () =>
            {
                return parameterValues.Select(parameterValue =>
                {
                    var parameterValueModel = new ReportParameterValueModel
                    {
                        Id = parameterValue.Id,
                        ReportParameterId = parameterValue.ReportParameterId,
                        Name = parameterValue.Name,
                        IsPreSelected = parameterValue.IsPreSelected,
                        DisplayOrder = parameterValue.DisplayOrder
                    };
                    return parameterValueModel;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare Report Parameter Value model
        /// </summary>
        /// <param name="model">Report Parameter Value model</param>
        /// <param name="reportParameterValue">Report Parameter Value</param>
        /// <param name="reportParameterId">Report Parameter identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter Value model
        /// </returns>
        public ReportParameterValueModel PrepareReportParameterValueModel(ReportParameterValueModel model,
                ReportParameterValue reportParameterValue, int reportParameterId)
        {
            if (reportParameterValue != null)
            {
                model = model ?? new ReportParameterValueModel();

                model.Id = reportParameterValue.Id;
                model.ReportParameterId = reportParameterId;
                model.Name = reportParameterValue.Name;
                model.IsPreSelected = reportParameterValue.IsPreSelected;
                model.DisplayOrder = reportParameterValue.DisplayOrder;
            }

            return model;
        }

        /// <summary>
        /// Prepare Report Parameter Value table Fields
        /// </summary>
        /// <param name="model">Report Parameter Value model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter Value table fields
        /// </returns>
        public ReportParameterValue PrepareReportParameterValueFields(ReportParameterValueModel model)
        {
            if (model != null)
            {
                var reportParameterValue = new ReportParameterValue()
                {
                    Id = model.Id,
                    ReportParameterId = model.ReportParameterId,
                    Name = model.Name,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder
                };
                return reportParameterValue;
            }
            return null;
        }

        #endregion

        #endregion

        #region Methods for FrontSide

        #region task 629 New Features on Annique Reports

        //build hidden fields to global form object 
        public async Task<string> BuildHiddenFieldsScriptAsync(IList<string> selectedFields)
        {
            if (selectedFields == null || selectedFields.Count == 0)
                return string.Empty;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var roleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var affiliate = customer.AffiliateId > 0
                ? await _affiliateService.GetAffiliateByIdAsync(customer.AffiliateId)
                : null;

            //get address for affiliate first name and last name
            var address = await _addressService.GetAddressByIdAsync(affiliate?.AddressId ?? 0);
            var affiliateName = string.Empty;
            if (address != null)
            {
                affiliateName = address.FirstName + " " + address.LastName;
            }

            var data = new Dictionary<string, object>();

            foreach (var field in selectedFields)
            {
                switch (field)
                {
                    case "CustomerId":
                        data["customerId"] = customer.Id;
                        break;

                    case "CustomerRoles":
                        data["customerRoleIds"] = roleIds.ToArray();
                        break;

                    case "Username":
                        data["username"] = customer.Username;
                        break;

                    case "CustomerName":
                        data["customerName"] = $"{customer.FirstName} {customer.LastName}";
                        break;

                    case "AffiliateId":
                        if (affiliate != null)
                            data["affiliateId"] = affiliate.Id;
                        else
                            data["affiliateId"] = 0;
                        break;

                    case "AffiliateName":
                         data["affiliateName"] = $"{affiliateName}";
                        break;

                    case "AffiliateFriendlyName":
                        if (affiliate != null)
                            data["affiliateFriendlyName"] = affiliate.FriendlyUrlName;
                        else
                            data["affiliateFriendlyName"] = string.Empty;
                        break;
                }
            }

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return $"window.ReportFormData = {json};";
        }

        #endregion

        /// <summary>
        /// Prepare the customer report list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer report list model
        /// </returns>
        public async Task<CustomerReportListModel> PrepareCustomerReportListModelAsync()
        {
            var model = new CustomerReportListModel();

            var reports = (await _anniqueReportService.GetAllCustomerReportsAsync())
                .Where(r => !r.IsMenuOption && !r.PubliclyHostedPage)
                .ToList();

            foreach (var report in reports)
            {
                var seName = await EnsureReportSlugAsync(report);

                model.Reports.Add(new CustomerReportListModel.ReportDetailsModel
                {
                    Id = report.Id,
                    Name = string.IsNullOrEmpty(report.CustomText) ? report.Name : report.CustomText,
                    SeName = seName
                });
            }

            return model;
        }

        private async Task<string> EnsureReportSlugAsync(Report report)
        {
            // Try to get existing active slug 
            var seName = await _urlRecordService.GetActiveSlugAsync(
                report.Id,
                nameof(Report),
                0);

            if (!string.IsNullOrEmpty(seName))
                return seName;

            // Generate & validate slug only if missing
            seName = await _urlRecordService.ValidateSeNameAsync(
                report,
                string.Empty,
                report.Name,
                true);

            // Persist once
            await _urlRecordService.SaveSlugAsync(report, seName, 0);

            return seName;
        }

        /// <summary>
        /// Prepare the report details model
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report details model
        /// </returns>
        public async Task<CustomerReportListModel.ReportDetailsModel> PrepareReportDetailsModelAsync(Report report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            var customer = await _workContext.GetCurrentCustomerAsync();

            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            var model = new CustomerReportListModel.ReportDetailsModel
            {
                Id = report.Id,
                Name = report.Name,
                Username = customer.Username,
                StoreId = customer.RegisteredInStoreId,
                ScriptsBlock = settings.ReportScripts,
                CommonMyAppJs = settings.ReportCommonJs,
                CustomCSS = report.CustomCSS,
                CustomJS = report.CustomJS,
                TemplateBlock = report.TemplateBlock,
                CustomText = string.IsNullOrEmpty(report.CustomText) ? report.Name : report.CustomText,
            };

            #region task 629 New Features on Annique Reports

            model.IncludeReportCommonJs = report.IncludeReportCommonJs;
            model.IncludeReportParameters = report.IncludeReportParameters;

            //model.HiddenFieldsHtml = await BuildHiddenFieldsHtmlAsync(report.HiddenFields?.Split(',') ?? new string[0]);
            model.HiddenFieldsJs = await BuildHiddenFieldsScriptAsync(report.HiddenFields?.Split(',') ?? new string[0]);

            // Check if IncludeReportParameters is enabled before preparing the parameters
            if (report.IncludeReportParameters)
            {
                await PrepareCustomReportParametersAsync(model.ReportParameters, report);
            }

            #endregion

            return model;
        }

        /// <summary>
        /// Prepare the report parameter Model List
        /// </summary>
        /// <param name="models">Report Parameter Model List</param>
        /// <param name="report">Report</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter model List
        /// </returns>
        public async Task PrepareCustomReportParametersAsync(IList<CustomReportParameterModel> models, Report report)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            var parameters = await _reportParameterService.GetAllReportParametersAsync(report.Id);
            foreach (var parameter in parameters)
            {
                var parameterModel = new CustomReportParameterModel
                {
                    Id = parameter.Id,
                    Name = parameter.Name,
                    AttributeControlType = parameter.AttributeControlType
                };

                if (parameter.ShouldHaveValues())
                {
                    //values
                    var parameterValues = await _reportParameterService.GetReportParameterValuesAsync(parameter.Id);
                    foreach (var parameterValue in parameterValues)
                    {
                        var parameterValueModel = new CustomReportParameterValueModel
                        {
                            Id = parameterValue.Id,
                            Name = parameterValue.Name,
                            IsPreSelected = parameterValue.IsPreSelected,
                        };
                        parameterModel.Values.Add(parameterValueModel);
                    }
                }
                models.Add(parameterModel);
            }
        }

        #endregion
    }
}
