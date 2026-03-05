namespace Annique.Plugins.Nop.Customization.Models.Catalog
{
    public class CatalogProductDetails
    {
        public int ProductId { get; set; }

        //quantity in cart
        public int Quantity { get; set; }

        //product price before discount
        public string Price { get; set; }
    }
}
