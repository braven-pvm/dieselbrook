using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Nop.Core.Domain.Catalog;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueReport
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class ReportParameterExtentions
    {
        /// <summary>
        /// A value indicating whether this parameter should have values
        /// </summary>
        /// <param name="reportParameter">Report Parameter</param>
        /// <returns>Result</returns>
        public static bool ShouldHaveValues(this ReportParameter reportParameter)
        {
            if (reportParameter == null)
                return false;

            if (reportParameter.AttributeControlType == AttributeControlType.TextBox)
                return false;

            //other attribute control types support values
            return true;
        }
    }
}
