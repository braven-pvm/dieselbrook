using ClosedXML.Excel;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;
using XcellenceIT.Plugin.ProductRibbons.Models;

namespace XcellenceIT.Plugin.ProductRibbons.Factories
{
    public interface IProductRibbonPublicFactory
    {
        Task<PublicRibbonModel> PreparedPublicRibbonModel(ProductPictureRibbon productPictureRibbon, Product product);

        Task<ProductRibbonsPublicModel> PreparedRibbon(Product product, IList<ProductRibbonRecord> ribbonList, Dictionary<int, ProductPictureRibbon> productPictures);
    }
}
