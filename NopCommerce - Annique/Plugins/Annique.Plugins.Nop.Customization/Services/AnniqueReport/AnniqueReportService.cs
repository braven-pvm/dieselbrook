using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueReport
{
    /// <summary>
    /// Report Service
    /// </summary>
    public class AnniqueReportService : IAnniqueReportService
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IRepository<Report> _reportRepository;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        #endregion

        #region Ctor

        public AnniqueReportService(IAclService aclService,
            IRepository<Report> reportRepository,
            IWorkContext workContext,
           ICustomerService customerService,
           IHttpContextAccessor httpContextAccessor,
           IStaticCacheManager staticCacheManager,
           IWebHostEnvironment webHostEnvironment)
        {
            _aclService = aclService;
            _reportRepository = reportRepository;
            _workContext = workContext;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
            _staticCacheManager = staticCacheManager;
            _hostingEnvironment = webHostEnvironment;
        }

        #endregion

        #region CRUD Methods

        /// <summary>
        /// Inserts a Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertReportAsync(Report report)
        {
            await _reportRepository.InsertAsync(report);
        }

        /// <summary>
        /// Update a Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateReportAsync(Report report)
        {
            await _reportRepository.UpdateAsync(report);
        }

        /// <summary>
        /// Deletes a Report
        /// </summary>
        /// <param name="report">Report</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteReportAsync(Report report)
        {
            await _reportRepository.DeleteAsync(report);
        }

        /// <summary>
        /// Gets a Report
        /// </summary>
        /// <param name="reportId">Report identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report
        /// </returns>
        public async Task<Report> GetReportByIdAsync(int reportId)
        {
            var getReportByIdCachekey = _staticCacheManager.PrepareKeyForDefaultCache(AnniqueCustomizationDefaults.GetReportByIdCacheKey, reportId);

            return await _staticCacheManager.GetAsync(getReportByIdCachekey, async () =>
            {
                // If not found in cache, retrieve from the database
                return await _reportRepository.GetByIdAsync(reportId);
            });
        }

        /// <summary>
        /// Gets all Reports
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Report
        /// </returns>
        public async Task<IPagedList<Report>> GetAllReportsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _reportRepository.Table;

            query = query.OrderBy(st => st.DisplayOrder);

            var reports = await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);

            return reports;
        }

        /// <summary>
        /// Gets all published Reports
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the published Report
        /// </returns>
        public async Task<IList<Report>> GetAllPublishedReportsAsync()
        {
            return await _reportRepository.GetAllAsync(query =>
            {
                // Only published reports
                query = query.Where(r => r.Published);

                // Order by display order
                query = query.OrderBy(r => r.DisplayOrder);

                return query;
            }, _ => AnniqueCustomizationDefaults.GetPublishedReportsAllCacheKey);
        }


        /// <summary>
        /// Gets all Reports by Current Customer 
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Reports
        /// </returns>
        public async Task<IList<Report>> GetAllCustomerReportsAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            // Fetch all published reports from cache
            var reports = await GetAllPublishedReportsAsync();

            // Apply ACL filtering for this customer
            var queryableReports = reports.AsQueryable();
            queryableReports = await _aclService.ApplyAcl(queryableReports, customerRoleIds);

            return queryableReports.ToList();
        }

        /// <summary>
        /// Gets all file urls 
        /// </summary>
        /// <returns>
        /// The task result contains the file urls and updated report configuration block
        /// </returns>
        public string[] ExtractFileUrlsFromFirstLine(string reportBlock, string fileType, out string updatedBlock)
        {
            //base url of site
            var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            //split lines in report block
            string[] lines = reportBlock.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 0)
            {
                //take only firstline for [] with file name searching
                string firstLine = lines[0].Trim();

                //search [] within first line
                if (firstLine.StartsWith("[") && firstLine.EndsWith("]"))
                {
                    //comma seperated file list within []
                    string fileList = firstLine.Trim('[', ']');
                    string[] fileNames = fileList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    List<string> fileUrls = new();

                    foreach (string fileName in fileNames)
                    {
                        string trimmedFileName = fileName.Trim();

                        //filetype can be .css || .js || .html
                        if (trimmedFileName.EndsWith(fileType))
                        {
                            //remove unwanted spaces from filename
                            string encodedFileName = trimmedFileName.Replace(fileType, "").Trim() + fileType;

                            //prepare file url
                            string fileUrl = $"{baseUrl}/Reports/{fileType.TrimStart('.')}/{encodedFileName}";
                            fileUrls.Add(fileUrl);
                        }
                    }

                    // skip the first line ['filenames'] since files are imported
                    reportBlock = string.Join(Environment.NewLine, lines.Skip(1));

                    // Update the Model.CustomJS property
                    updatedBlock = reportBlock;

                    return fileUrls.ToArray();
                }
            }
            // No files found, keep the report block as it is
            updatedBlock = reportBlock;
            return Array.Empty<string>();
        }

        #region Task 602 Including HTML in Reports HTML from %filename.html% by creating temp file with whole content and returning temp file url

        ///// <summary>
        ///// Gets all file urls of processed html temp file
        ///// </summary>
        ///// <returns>
        ///// The task result contains the url of processed temp file
        ///// </returns>
        public string[] ProcessTokensAndGetTempFileUrls(string reportBlock, string fileType, string relativeBasePath, out string updatedBlock)
        {
            // Extract file URLs from the report block
            string[] fileUrls = ExtractFileUrlsFromFirstLine(reportBlock, fileType, out updatedBlock);

            // Convert the relative base path to an absolute path on the server
            string basePath = Path.Combine(_hostingEnvironment.WebRootPath, relativeBasePath.TrimStart('~', '/'));

            // Iterate over the file URLs and process them
            for (int i = 0; i < fileUrls.Length; i++)
            {
                // Extract just the file name from the URL (ignoring the base URL part)
                string fileName = Path.GetFileName(fileUrls[i]);

                // Map the file name to the server-side absolute path (e.g., "/wwwroot/Reports/Templates/vds.html")
                string filePath = Path.Combine(basePath, fileName);

                // Check if the file exists on the server and process the content if it's an HTML file
                if (File.Exists(filePath) && fileType == ".html")
                {
                    // Create a temporary file with _temp.html suffix
                    string tempFileName = Path.Combine(basePath, $"{Path.GetFileNameWithoutExtension(fileName)}_temp{fileType}");

                    // Check if the temporary file already exists to avoid overwriting
                    if (!File.Exists(tempFileName))
                    {
                        // Read the original file content
                        string fileContent = File.ReadAllText(filePath);

                        // Process the content by replacing placeholders with actual HTML content
                        string processedContent = ProcessTemplateBlock(fileContent, basePath);

                        // Write the processed content to the temporary file
                        File.WriteAllText(tempFileName, processedContent);
                    }

                    // Construct the relative URL for the temporary file (relative to the Reports folder)
                    string relativeUrl = Path.Combine(relativeBasePath.TrimStart('~', '/'), $"{Path.GetFileNameWithoutExtension(fileName)}_temp{fileType}");
                    fileUrls[i] = "/" + relativeUrl.Replace("\\", "/"); // Ensure the URL uses forward slashes
                }
            }

            // Return the URLs of the temporary files
            return fileUrls;
        }

        public string ProcessTemplateBlock(string templateBlock, string basePath)
        {
            // Regular expression to match placeholders like %file.html%
            var placeholderPattern = new Regex(@"%([\w\-\.]+\.html)%");

            // Base URL or path where the HTML files are stored
            if (string.IsNullOrEmpty(basePath))
                basePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "html");

            // Keep processing the templateBlock until no placeholders are found
            while (placeholderPattern.IsMatch(templateBlock))
            {
                // Find all matches in the templateBlock
                var matches = placeholderPattern.Matches(templateBlock);

                foreach (Match match in matches)
                {
                    // Extract the filename from the match, e.g., file.html
                    string fileName = match.Groups[1].Value;

                    // Full path to the referenced file
                    string filePath = Path.Combine(basePath, fileName);

                    // Ensure the file exists before attempting to read it
                    if (File.Exists(filePath))
                    {
                        // Read the content of the file
                        string fileContent = File.ReadAllText(filePath);

                        // Recursively process the content of the included file in case it has its own %file.html%
                        string processedFileContent = ProcessTemplateBlock(fileContent, basePath);

                        // Replace the placeholder %file.html% with the actual content of the file
                        templateBlock = templateBlock.Replace(match.Value, processedFileContent);
                    }
                }
            }

            // Return the processed templateBlock with all placeholders replaced
            return templateBlock;
        }

        #endregion

        #endregion
    }
}
