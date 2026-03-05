using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueReport
{
    //this is schedular task to remove temp html files used for report template block
    public partial class ClearTempHtmlFilesTask : IScheduleTask
    {
        #region Fields

        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public ClearTempHtmlFilesTask(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Method
        public async Task ExecuteAsync()
        {
            try
            {
                //Task 637 consultant registration
                //consultant  registration page's html is inside assets folder so clear those temp file as welll
                // Define all directories to process
                var tempDirectories = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Reports/html"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/html")
                };

                foreach (var directory in tempDirectories)
                {
                    if (!Directory.Exists(directory))
                    {
                        await _logger.WarningAsync($"Directory not found: {directory}");
                        continue;
                    }

                    // Get all _temp.html files in the directory
                    var tempFiles = Directory.GetFiles(directory, "*_temp.html");

                    foreach (var tempFilePath in tempFiles)
                    {
                        var tempFileInfo = new FileInfo(tempFilePath);

                        // Get the corresponding main .html file (without _temp suffix)
                        string mainFilePath = Path.Combine(tempFileInfo.DirectoryName, tempFileInfo.Name.Replace("_temp", ""));

                        // Check if the main file exists
                        if (File.Exists(mainFilePath))
                        {
                            var mainFileInfo = new FileInfo(mainFilePath);

                            // Compare the last modified dates
                            if (mainFileInfo.LastWriteTimeUtc > tempFileInfo.LastWriteTimeUtc)
                            {
                                // Delete the _temp.html file if the main file is newer
                                await _logger.InformationAsync($"Deleting outdated temp file: {tempFileInfo.Name}");

                                await Task.Run(tempFileInfo.Delete);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("An error occurred while clearing stale _temp.html files.", ex);
            }

        }

        #endregion
    }
}