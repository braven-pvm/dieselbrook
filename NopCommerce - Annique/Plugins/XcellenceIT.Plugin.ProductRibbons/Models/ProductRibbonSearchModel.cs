using Nop.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace XcellenceIT.Plugin.ProductRibbons.Models
{
    public partial record ProductRibbonSearchModel : BaseSearchModel
    {
        #region Properties

        public int RibbonId { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Enabled.Search")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.MarkAsNew")]
        public bool MarkAsNew { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.StartDateUtc")]
        [UIHint("DateNullable")]
        public DateTime? StartDateUtc { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.EndDateUtc")]
        [UIHint("DateNullable")]
        public DateTime? EndDateUtc { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.RibbonName")]
        public string RibbonName { get; set; }

        #endregion
    }
}
