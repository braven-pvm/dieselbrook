using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Nop.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueReport
{
    /// <summary>
    /// Report interface
    /// </summary>
    public interface IAnniqueReportService
    {
        /// <summary>
        /// Inserts a Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertReportAsync(Report report);

        /// <summary>
        /// Update a Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateReportAsync(Report report);

        /// <summary>
        /// Deletes a Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteReportAsync(Report report);

        /// <summary>
        /// Gets a Report
        /// </summary>
        /// <param name="reportId">Report identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report
        /// </returns>
        Task<Report> GetReportByIdAsync(int reportId);

        /// <summary>
        /// Gets all Reports
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report
        /// </returns>
        Task<IPagedList<Report>> GetAllReportsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        /// <summary>
        /// Gets all Reports by Current Customer
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Reports
        /// </returns>
        Task<IList<Report>> GetAllCustomerReportsAsync();

        /// <summary>
        /// Gets all file urls 
        /// </summary>
        /// <returns>
        /// The task result contains the file urls and updated report configuration block
        /// </returns>
        string[] ExtractFileUrlsFromFirstLine(string reportBlock, string fileType, out string updatedBlock);

        ///// <summary>
        ///// Gets all file urls of processed html temp file
        ///// </summary>
        ///// <returns>
        ///// The task result contains the url of processed temp file
        ///// </returns>
        string[] ProcessTokensAndGetTempFileUrls(string reportBlock, string fileType, string relativeBasePath, out string updatedBlock);

        ///// <summary>
        ///// Process html content 
        ///// </summary>
        ///// <returns>
        ///// The task result contains proccessed html content 
        ///// </returns>
        string ProcessTemplateBlock(string templateBlock, string basePath);
    }
}
