using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.CheckoutGifts
{
    public record SpecialProductListModel : BaseNopModel
    {
        public SpecialProductListModel()
        {
            ProductList = new List<ProductListItemsModel>();
        }

        public int OfferId { get; set; }

        public int DiscountId { get; set; }

        public IList<ProductListItemsModel> ProductList { get; set; }

        public record ProductListItemsModel : BaseNopModel
        {
            public int ProductId { get; set; }

            public string Name { get; set; }

            public string Price { get; set; }

            public string OldPrice { get; set; }

            public PictureModel PictureModel { get; set; }
        }
    }

	public class ProductSelectionModel
	{
		public int Id { get; set; }
		public int Quantity { get; set; }
	}
}
