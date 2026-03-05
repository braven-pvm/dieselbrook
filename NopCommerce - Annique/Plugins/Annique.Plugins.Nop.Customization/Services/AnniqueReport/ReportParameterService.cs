using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Nop.Core;
using Nop.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DocumentFormat.OpenXml.Bibliography;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueReport
{
    /// <summary>
    /// Report parameter Service
    /// </summary>
    public class ReportParameterService : IReportParameterService
    {
        #region Fields

        private readonly IRepository<Report> _reportRepository;
        private readonly IRepository<ReportParameter> _reportParameterRepository;
        private readonly IRepository<ReportParameterValue> _reportParameterValueRepository;

        #endregion

        #region Ctor

        public ReportParameterService(IRepository<Report> reportRepository,
            IRepository<ReportParameter> reportParameterRepository,
            IRepository<ReportParameterValue> reportParameterValueRepository)
        {
            _reportRepository = reportRepository;
            _reportParameterRepository = reportParameterRepository;
            _reportParameterValueRepository = reportParameterValueRepository;
        }

        #endregion

        #region Report Parameter Methods

        /// <summary>
        /// Inserts a Report Parameter
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertReportParamterAsync(ReportParameter reportParameter)
        { 
            await _reportParameterRepository.InsertAsync(reportParameter);
        }

        /// <summary>
        /// Updates a Report Parameter
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateReportParameterAsync(ReportParameter reportParameter)
        {
            await _reportParameterRepository.UpdateAsync(reportParameter);
        }

        /// <summary>
        /// Deletes a Report Parameter
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteReportParameterAsync(ReportParameter reportParameter)
        {
            await _reportParameterRepository.DeleteAsync(reportParameter);
        }

        /// <summary>
        /// Gets a Report Parameter by Id
        /// </summary>
        /// <param name="reportParameterId">Report Parameter identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter
        /// </returns>
        public async Task<ReportParameter> GetReportParameterByIdAsync(int reportParameterId)
        { 
            return await _reportParameterRepository.GetByIdAsync(reportParameterId);
        }

        /// <summary>
        /// Gets all Report Parameters
        /// </summary>
        /// <param name="reportId">Report identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameters
        /// </returns>
        public async Task<IPagedList<ReportParameter>> GetAllReportParametersAsync(int reportId, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            if (reportId == 0)
                return new PagedList<ReportParameter>(new List<ReportParameter>(), pageIndex, pageSize);

            var query = from reportParameter in _reportParameterRepository.Table
                        join report in _reportRepository.Table on reportParameter.ReportId equals report.Id
                        where reportParameter.ReportId == reportId
                        orderby reportParameter.DisplayOrder
                        select reportParameter;

            var reportparameters = await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);

            return reportparameters;
        }

        #endregion

        #region Report Parameter value Methods

        /// <summary>
        /// Inserts a Report Parameter value
        /// </summary>
        /// <param name="reportParameterValue">Report Parameter Value</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertReportParameterValueAsync(ReportParameterValue reportParameterValue)
        { 
            await _reportParameterValueRepository.InsertAsync(reportParameterValue);
        }

        /// <summary>
        /// Updates a Report Parameter Value
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateReportParameterValueAsync(ReportParameterValue reportParameterValue)
        {
            await _reportParameterValueRepository.UpdateAsync(reportParameterValue);
        }

        /// <summary>
        /// Deletes a Report Parameter Value
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteReportParameterValueAsync(ReportParameterValue reportParameterValue)
        {
            await _reportParameterValueRepository.DeleteAsync(reportParameterValue);
        }

        /// <summary>
        /// Gets a Report Parameter Value by Id
        /// </summary>
        /// <param name="reportParameterValueId">Report Parameter Value identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter value
        /// </returns>
        public async Task<ReportParameterValue> GetReportParameterValueByIdAsync(int reportParameterValueId)
        {
            return await _reportParameterValueRepository.GetByIdAsync(reportParameterValueId);
        }

        /// <summary>
        /// Gets all Report Parameter Values
        /// </summary>
        /// <param name="reportParameterId">Report Parameter identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report Parameter value
        /// </returns>
        public async Task<IPagedList<ReportParameterValue>> GetReportParameterValuesAsync(int reportParameterId, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            if (reportParameterId == 0)
                return new PagedList<ReportParameterValue>(new List<ReportParameterValue>(), pageIndex, pageSize);

            var query = from reportParameterValue in _reportParameterValueRepository.Table
                        join reportParameter in _reportParameterRepository.Table on reportParameterValue.ReportParameterId equals reportParameter.Id
                        where reportParameterValue.ReportParameterId == reportParameterId
                        orderby reportParameterValue.DisplayOrder
                        select reportParameterValue;

            var parameterValues = await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);

            return parameterValues;
        }

        #endregion
    }
}
