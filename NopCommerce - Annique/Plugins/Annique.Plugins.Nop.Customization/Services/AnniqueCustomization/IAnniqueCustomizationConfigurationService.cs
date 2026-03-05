using Nop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annique.Plugins.Nop.Customization.Domain.Enums;
using Nop.Core.Domain.Customers;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueCustomization
{
    /// <summary>
    /// AnniqueCustomizationConfigurationService interface
    /// </summary>
    public interface IAnniqueCustomizationConfigurationService
    {
        /// <summary>
        /// Returns Wheather Plugin is enabled/Disabled
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<bool> IsPluginEnableAsync();

        /// <summary>
        ///Return Pickup collection is enable or disable
        /// </summary>
        Task<bool> IsPickupCollectionEnableAsync();

        /// <summary>
        ///Return Full text search is enable or disable
        /// </summary>
        Task<bool> IsFullTextSearchEnableAsync();

        /// <summary>
        /// Returns Wheather Customer has consultant role
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<bool> IsConsultantRoleAsync();

        /// <summary>
        /// Returns shopping cart totals before discounts
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<(decimal cartTotal, string cartTotalValue)> GetShoppingCartTotalsBeforeDiscountAsync(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Validate and update Billing Address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ValidateBillingAddress(Address address);

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecOptions">Specification options list to filter products; null to load all records</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            AnniqueProductSortingEnum orderBy = AnniqueProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null);

        /// <summary>
        /// Returns customized customer name based on affiliate and customer role
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<string> GetCustomizedCustomerFullNameAsync(Customer customer);

        /// <summary>
        /// sets customer role
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task SetCustomerRoleToRegisteredUserAsync(int customerId);

        /// <summary>
        /// get username for share link
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<string> GetUsernameAsync();

        /// <summary>
        /// sets Client role
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task SetClientRoleToUserAsync(Customer customer);

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="keywords">Keywords</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IPagedList<Product>> SearchProductsWithFullTextAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            int storeId = 0,
            bool visibleIndividuallyOnly = false,
            string keywords = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            SearchOption? searchOption = null);

        /// <summary>
        /// Show trip promotion 
        /// </summary>
        /// <param name="totalRsp">Total rsp</param>
        /// <returns>This task returns to show promotion message or not , also return promotion message </returns>
        Task<(bool ShowBox, string PromotionMessage)> ShouldShowTripPromotionAsync(decimal totalRsp);
    }
}
