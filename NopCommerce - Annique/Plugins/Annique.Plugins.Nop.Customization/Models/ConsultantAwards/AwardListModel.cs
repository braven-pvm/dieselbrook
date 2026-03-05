using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.ConsultantAwards
{
    public record AwardListModel : BaseNopModel
    {
        #region Ctor

        public AwardListModel()
        {
            Awards = new List<AwardDetailsModel>();
            Products = new List<AwardProductListModel>();
        }

        #endregion

        #region Property

        public IList<AwardDetailsModel> Awards { get; set; }
        public IList<AwardProductListModel> Products { get; set; }

        public int SelectedAwardId { get; set; }

        #endregion

        #region Nested Class

        public record AwardDetailsModel : BaseNopModel
        {
            public int Id { get; set; }

            public string AwardType { get; set; }

            public string Description { get; set; }
            public string ExpiryDate { get; set; }

            public int MaxValue { get; set; }

            public decimal RemainingValue { get; set; }

            public bool ShowSelectedOnly { get; set; }
        }

        public record AwardProductListModel : BaseNopModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Price { get; set; }

            public decimal ProductPrice { get; set; }

            public PictureModel PictureModel { get; set; }

            public int Quantity { get; set; }
        }

        #endregion
    }
}
