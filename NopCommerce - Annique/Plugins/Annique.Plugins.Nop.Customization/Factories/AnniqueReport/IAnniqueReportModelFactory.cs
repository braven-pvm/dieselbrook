using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Admin;
using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Public;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.AnniqueReport
{
    /// <summary>
    /// Represents the Annique Report Model Factory interface
    /// </summary>
    public interface IAnniqueReportModelFactory
    {
        /// <summary>
        /// Prepare report search model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the report search model
        /// </returns>
        Task<ReportSearchModel> PrepareReportSearchModelAsync(ReportSearchModel searchModel);

        /// <summary>
        /// Prepare paged Report list model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report list model
        /// </returns>
        Task<ReportListModel> PrepareReportListModelAsync(ReportSearchModel searchModel);

        /// <summary>
        /// Prepare Report model
        /// </summary>
        /// <param name="model">Report model</param>
        /// <param name="report">Report</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report model
        /// </returns>
        Task<ReportModel> PrepareReportModelAsync(ReportModel model,
            Report report, bool excludeProperties = false);

        /// <summary>
        /// Prepare Report table Fields
        /// </summary>
        /// <param name="model">Report model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report table fields
        /// </returns>
        Report PrepareReportFields(ReportModel model);

        /// <summary>
        /// Prepare report parameter search model
        /// </summary>
        /// <param name="searchModel">Report Parametersearch model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the report Parameter search model
        /// </returns>
        ReportParameterSearchModel PrepareReportParameterSearchModel(ReportParameterSearchModel searchModel, Report report);

        /// <summary>
        /// Prepare paged Report Parameter list model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <param name="report">Report</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter list model
        /// </returns>
        Task<ReportParameterListModel> PrepareReportParameterListModelAsync(ReportParameterSearchModel searchModel, Report report);

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
        Task<ReportParameterModel> PrepareReportParameterModelAsync(ReportParameterModel model,
                ReportParameter reportParameter, int reportId);

        /// <summary>
        /// Prepare Report Parameter table Fields
        /// </summary>
        /// <param name="model">Report Parameter model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter table fields
        /// </returns>
        ReportParameter PrepareReportParameterFields(ReportParameterModel model);

        /// <summary>
        /// Prepare paged Report Parameter Value list model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter Value list model
        /// </returns>
        Task<ReportParameterValueListModel> PrepareReportParameterValueListModelAsync(ReportParameterValueSearchModel searchModel,
                ReportParameter reportParameter);

        /// <summary>
        /// Prepare Report Parameter Value model
        /// </summary>
        /// <param name="model">Report Parameter Value model</param>
        /// <param name="reportParameterValue">Report Parameter Value</param>
        /// <param name="reportParameterId">Report Parameter identifier</param>
        /// <returns>
        /// The task result contains the Report Parameter Value model
        /// </returns>
        ReportParameterValueModel PrepareReportParameterValueModel(ReportParameterValueModel model,
                ReportParameterValue reportParameterValue, int reportParameterId);

        /// <summary>
        /// Prepare Report Parameter Value table Fields
        /// </summary>
        /// <param name="model">Report Parameter Value model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter Value table fields
        /// </returns>
        ReportParameterValue PrepareReportParameterValueFields(ReportParameterValueModel model);

        /// <summary>
        /// Prepare the customer report list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer report list model
        /// </returns>
        Task<CustomerReportListModel> PrepareCustomerReportListModelAsync();

        /// <summary>
        /// Prepare the report details model
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report details model
        /// </returns>
        Task<CustomerReportListModel.ReportDetailsModel> PrepareReportDetailsModelAsync(Report report);

        /// <summary>
        /// Prepare the report parameter Model List
        /// </summary>
        /// <param name="models">Report Parameter Model List</param>
        /// <param name="report">Report</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter model List
        /// </returns>
        Task PrepareCustomReportParametersAsync(IList<CustomReportParameterModel> models, Report report);
    }
}
