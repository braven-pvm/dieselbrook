using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueReports.Public
{
    public record CustomReportParameterModel : BaseNopEntityModel
    {
        public CustomReportParameterModel()
        {
            Values = new List<CustomReportParameterValueModel>();
        }

        public string Name { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<CustomReportParameterValueModel> Values { get; set; }
    }

    public record CustomReportParameterValueModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}
