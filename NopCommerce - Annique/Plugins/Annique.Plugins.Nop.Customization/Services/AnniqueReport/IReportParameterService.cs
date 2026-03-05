using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Nop.Core;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueReport
{
    /// <summary>
    /// Report Parameter interface
    /// </summary>
    public interface IReportParameterService
    {
        /// <summary>
        /// Inserts a Report Parameter
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertReportParamterAsync(ReportParameter reportParameter);

        /// <summary>
        /// Updates a Report Parameter
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateReportParameterAsync(ReportParameter reportParameter);

        /// <summary>
        /// Deletes a Report Parameter
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteReportParameterAsync(ReportParameter reportParameter);

        /// <summary>
        /// Gets a Report Parameter by Id
        /// </summary>
        /// <param name="reportParameterId">Report Parameter identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter
        /// </returns>
        Task<ReportParameter> GetReportParameterByIdAsync(int reportParameterId);

        /// <summary>
        /// Gets all Report Parameters
        /// </summary>
        /// <param name="reportId">Report identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameters
        /// </returns>
        Task<IPagedList<ReportParameter>> GetAllReportParametersAsync(int reportId, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        /// <summary>
        /// Inserts a Report Parameter value
        /// </summary>
        /// <param name="reportParameterValue">Report Parameter Value</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertReportParameterValueAsync(ReportParameterValue reportParameterValue);

        /// <summary>
        /// Updates a Report Parameter Value
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateReportParameterValueAsync(ReportParameterValue reportParameterValue);

        /// <summary>
        /// Deletes a Report Parameter Value
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteReportParameterValueAsync(ReportParameterValue reportParameterValue);

        /// <summary>
        /// Gets a Report Parameter Value by Id
        /// </summary>
        /// <param name="reportParameterValueId">Report Parameter Value identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter value
        /// </returns>
        Task<ReportParameterValue> GetReportParameterValueByIdAsync(int reportParameterValueId);

        /// <summary>
        /// Gets all Report Parameter Values
        /// </summary>
        /// <param name="reportParameterId">Report Parameter identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter value
        /// </returns>
        Task<IPagedList<ReportParameterValue>> GetReportParameterValuesAsync(int reportParameterId, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
    }
}
