using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public class NewProductsModelDto : ModelDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the catalog products model
        /// </summary>
        public CatalogProductsModelDto CatalogProductsModel { get; set; }

        #endregion

        #region Ctor

        public NewProductsModelDto()
        {
            CatalogProductsModel = new CatalogProductsModelDto();
        }

        #endregion
    }
}
