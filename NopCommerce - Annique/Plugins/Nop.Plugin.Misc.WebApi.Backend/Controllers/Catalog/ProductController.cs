using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ProductController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IRepository<ShipmentItem> _shipmentItemRepository;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        #endregion

        #region Ctor

        public ProductController(IProductService productService,
            IRepository<ShipmentItem> shipmentItemRepository,
            ISpecificationAttributeService specificationAttributeService)
        {
            _productService = productService;
            _shipmentItemRepository = shipmentItemRepository;
            _specificationAttributeService = specificationAttributeService;
        }

        #endregion

        #region Methods

        #region Products

        /// <summary>
        /// Delete products
        /// </summary>
        /// <param name="ids">Array of product identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        public virtual async Task<IActionResult> DeleteProducts(string ids)
        {
            var productsId = ids.ToIdArray();
            var products = await _productService.GetProductsByIdsAsync(productsId);

            await _productService.DeleteProductsAsync(products);

            return Ok();
        }

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllProductsDisplayedOnHomepage()
        {
            var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Gets featured products by a category identifier
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        [HttpGet("{categoryId}")]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategoryFeaturedProducts(int categoryId, [FromQuery] int storeId = 0)
        {
            var products = await _productService.GetCategoryFeaturedProductsAsync(categoryId, storeId);
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Gets featured products by a manufacturer identifier
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        [HttpGet("{manufacturerId}")]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetManufacturerFeaturedProducts(int manufacturerId, [FromQuery] int storeId = 0)
        {
            var products = await _productService.GetManufacturerFeaturedProductsAsync(manufacturerId, storeId);
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Gets products which marked as new
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductsMarkedAsNew([FromQuery] int storeId = 0)
        {
            var products = await _productService.GetProductsMarkedAsNewAsync(storeId);
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Gets products by identifier
        /// </summary>
        /// <param name="ids">Array of product identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductsByIds(string ids)
        {
            var productsId = ids.ToIdArray();

            var products = await _productService.GetProductsByIdsAsync(productsId);
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Get number of product (published and visible) in certain category
        /// </summary>
        /// <param name="categoryIds">Array of category identifiers (separator - ;)</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        [HttpGet]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNumberOfProductsInCategory([FromQuery] string categoryIds = null, [FromQuery] int storeId = 0)
        {
            var categoriesId = categoryIds == null ? Array.Empty<int>() : categoryIds.ToIdArray();

            var count = await _productService.GetNumberOfProductsInCategoryAsync(categoriesId, storeId);

            return Ok(count);
        }

        /// <summary>
        /// Gets products by product attribute
        /// </summary>
        /// <param name="productAttributeId">Product attribute identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{productAttributeId}")]
        [ProducesResponseType(typeof(PagedListDto<Product, ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductsByProductAtributeId(int productAttributeId,
            [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = int.MaxValue)
        {
            var products = await _productService.GetProductsByProductAttributeIdAsync(productAttributeId,
                pageIndex, pageSize);

            return Ok(products.ToPagedListDto<Product, ProductDto>());
        }

        /// <summary>
        /// Gets associated products
        /// </summary>
        /// <param name="parentGroupedProductId">Parent product identifier (used with grouped products)</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{parentGroupedProductId}")]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAssociatedProducts(int parentGroupedProductId,
            [FromQuery] int storeId = 0, [FromQuery] int vendorId = 0, [FromQuery] bool showHidden = false)
        {
            var products = await _productService.GetAssociatedProductsAsync(parentGroupedProductId, storeId, vendorId, showHidden);
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Update product review totals
        /// </summary>
        /// <param name="productId">Product Id</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateProductReviewTotals(int productId)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            await _productService.UpdateProductReviewTotalsAsync(product);

            return Ok();
        }

        /// <summary>
        /// Get low stock products
        /// </summary>
        /// <param name="vendorId">Vendor identifier; pass null to load all records</param>
        /// <param name="loadPublishedOnly">Whether to load published products only; pass null to load all products, pass true to load only published products, pass false to load only unpublished products</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Product, ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLowStockProducts([FromQuery] int? vendorId = null,
            [FromQuery] bool? loadPublishedOnly = true,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool getOnlyTotalCount = false)
        {
            var products = await _productService.GetLowStockProductsAsync(vendorId, loadPublishedOnly,
                pageIndex, pageSize, getOnlyTotalCount);

            return Ok(products.ToPagedListDto<Product, ProductDto>());
        }

        /// <summary>
        /// Get low stock product combinations
        /// </summary>
        /// <param name="vendorId">Vendor identifier; pass null to load all records</param>
        /// <param name="loadPublishedOnly">Whether to load combinations of published products only; pass null to load all products, pass true to load only published products, pass false to load only unpublished products</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ProductAttributeCombination, ProductAttributeCombinationDto>),
            StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLowStockProductCombinations([FromQuery] int? vendorId = null,
            [FromQuery] bool? loadPublishedOnly = true,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool getOnlyTotalCount = false)
        {
            var products = await _productService.GetLowStockProductCombinationsAsync(vendorId, loadPublishedOnly,
                pageIndex, pageSize, getOnlyTotalCount);

            return Ok(products.ToPagedListDto<ProductAttributeCombination, ProductAttributeCombinationDto>());
        }

        /// <summary>
        /// Gets a product by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        [HttpGet]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetProductBySku([FromQuery][Required] string sku)
        {
            var product = await _productService.GetProductBySkuAsync(sku);

            if (product == null)
                return NotFound($"Product sku={sku} not found");

            return Ok(product.ToDto<ProductDto>());
        }

        /// <summary>
        /// Gets a products by SKU array
        /// </summary>
        /// <param name="skuArray">SKU array (separator - ;)</param>
        /// <param name="vendorId">Vendor ID; 0 to load all records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductsBySku([FromQuery][Required] string skuArray, [FromQuery] int vendorId = 0)
        {
            var products = await _productService.GetProductsBySkuAsync(skuArray.Split(';'), vendorId);
            var productsDto = products.Select(p => p.ToDto<ProductDto>());

            return Ok(productsDto);
        }

        /// <summary>
        /// Update HasTierPrices property (used for performance optimization)
        /// </summary>
        /// <param name="productId">Product Id</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateHasTierPricesProperty(int productId)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            await _productService.UpdateHasTierPricesPropertyAsync(product);

            return Ok();
        }

        /// <summary>
        /// Update HasDiscountsApplied property (used for performance optimization)
        /// </summary>
        /// <param name="productId">Product Id</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateHasDiscountsApplied(int productId)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            await _productService.UpdateHasDiscountsAppliedAsync(product);

            return Ok();
        }

        /// <summary>
        /// Gets number of products by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        [HttpGet("{vendorId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNumberOfProductsByVendorId(int vendorId)
        {
            return Ok(await _productService.GetNumberOfProductsByVendorIdAsync(vendorId));
        }

        /// <summary>
        /// Parse "required product Ids" property
        /// </summary>
        /// <param name="productId">Product Id</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseRequiredProductIds(int productId)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var ids = _productService.ParseRequiredProductIds(product);

            return Ok(ids);
        }

        /// <summary>
        /// Get a value indicating whether a product is available now (availability dates)
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="dateTime">Datetime to check; pass null to use current date</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProductIsAvailable(int productId, [FromQuery] DateTime? dateTime = null)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var isAvailable = _productService.ProductIsAvailable(product, dateTime);

            return Ok(isAvailable);
        }

        /// <summary>
        /// Get a list of allowed quantities (parse 'AllowedQuantities' property)
        /// </summary>
        /// <param name="productId">Product Id</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseAllowedQuantities(int productId)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var allowedQuantities = _productService.ParseAllowedQuantities(product);

            return Ok(allowedQuantities);
        }

        /// <summary>
        /// Get total quantity
        /// </summary>
        /// <param name="productId">Product id</param>
        /// <param name="useReservedQuantity">
        /// A value indicating whether we should consider "Reserved Quantity" property 
        /// when "multiple warehouses" are used
        /// </param>
        /// <param name="warehouseId">
        /// Warehouse identifier. Used to limit result to certain warehouse.
        /// Used only with "multiple warehouses" enabled.
        /// </param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetTotalStockQuantity(int productId, 
            [FromQuery] bool useReservedQuantity = true, [FromQuery] int warehouseId = 0)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var totals = await
                _productService.GetTotalStockQuantityAsync(product, useReservedQuantity, warehouseId);

            return Ok(totals);
        }

        /// <summary>
        /// Get number of rental periods (price ratio)
        /// </summary>
        /// <param name="productId">Product id</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRentalPeriods(int productId,
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var periods = _productService.GetRentalPeriods(product, startDate, endDate);

            return Ok(periods);
        }

        /// <summary>
        /// Formats the stock availability/quantity message
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="attributesXml">Selected product attributes in XML format (if specified)</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatStockMessage(int productId, [FromBody] string attributesXml)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var message = _productService.FormatStockMessageAsync(product, attributesXml);

            return Ok(message);
        }

        /// <summary>
        /// Formats SKU
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatSku(int productId, [FromBody] string attributesXml = null)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var sku = _productService.FormatSkuAsync(product, attributesXml);

            return Ok(sku);
        }

        /// <summary>
        /// Formats manufacturer part number
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatMpn(int productId, [FromBody] string attributesXml = null)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var mpn = _productService.FormatMpnAsync(product, attributesXml);

            return Ok(mpn);
        }

        /// <summary>
        /// Formats GTIN
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatGtin(int productId, [FromBody] string attributesXml = null)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var gtin = _productService.FormatGtinAsync(product, attributesXml);

            return Ok(gtin);
        }

        /// <summary>
        /// Formats start/end date for rental product
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="date">date</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatRentalDate(int productId, [FromQuery][Required] DateTime date)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var formatedDate = _productService.FormatRentalDate(product, date);

            return Ok(formatedDate);
        }

        /// <summary>
        /// Update product store mappings
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="storesIds">A list of store ids for mapping (separator - ;)</param>
        [HttpGet("{productId}/{storesIds}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateProductStoreMappings(int productId, string storesIds)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} not found");

            var storesId = storesIds.ToIdArray();

            await _productService.UpdateProductStoreMappingsAsync(product, storesId);

            return Ok();
        }

        /// <summary>
        /// Gets the value whether the sequence contains downloadable products
        /// </summary>
        /// <param name="productIds">Product identifiers (separator - ;)</param>
        [HttpGet("{productIds}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HasAnyDownloadableProduct(string productIds)
        {
            var productsId = productIds.ToIdArray();

            var flag = await _productService.HasAnyDownloadableProductAsync(productsId);

            return Ok(flag);
        }

        /// <summary>
        /// Gets the value whether the sequence contains gift card products
        /// </summary>
        /// <param name="productIds">Product identifiers (separator - ;)</param>
        [HttpGet("{productIds}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HasAnyGiftCardProduct(string productIds)
        {
            var productsId = productIds.ToIdArray();

            var flag = await _productService.HasAnyGiftCardProductAsync(productsId);

            return Ok(flag);
        }

        /// <summary>
        /// Gets the value whether the sequence contains recurring products
        /// </summary>
        /// <param name="productIds">Product identifiers (separator - ;)</param>
        [HttpGet("{productIds}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HasAnyRecurringProduct(string productIds)
        {
            var productsId = productIds.ToIdArray();

            var flag = await _productService.HasAnyRecurringProductAsync(productsId);

            return Ok(flag);
        }

        /// <summary>
        /// Returns a list of sku of not existing products
        /// </summary>
        /// <param name="productSku">The sku of the products to check</param>
        [HttpGet("{skuArray}")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNotExistingProducts(string productSku)
        {
            if (productSku == null)
                throw new ArgumentNullException(nameof(productSku));

            var notExistingProducts = await _productService.GetNotExistingProductsAsync(productSku.ToSkuArray());

            return Ok(notExistingProducts);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound($"Product Id={id} not found");

            await _productService.DeleteProductAsync(product);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound($"Product Id={id} not found");

            return Ok(product.ToDto<ProductDto>());
        }

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
        /// <param name="filteredSpecOptionIds">Specification options list to filter products; null to load all records (separator - ;)</param>
        /// <param name="orderByType">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Product, ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] IList<int> categoryIds = null,
            [FromQuery] IList<int> manufacturerIds = null,
            [FromQuery] int storeId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] int warehouseId = 0,
            [FromQuery] ProductType? productType = null,
            [FromQuery] bool visibleIndividuallyOnly = false,
            [FromQuery] bool excludeFeaturedProducts = false,
            [FromQuery] decimal? priceMin = null,
            [FromQuery] decimal? priceMax = null,
            [FromQuery] int productTagId = 0,
            [FromQuery] string keywords = null,
            [FromQuery] bool searchDescriptions = false,
            [FromQuery] bool searchManufacturerPartNumber = true,
            [FromQuery] bool searchSku = true,
            [FromQuery] bool searchProductTags = false,
            [FromQuery] int languageId = 0,
            [FromQuery] string filteredSpecOptionIds = null,
            [FromQuery] ProductSortingEnum orderByType = ProductSortingEnum.Position,
            [FromQuery] bool showHidden = false,
            [FromQuery] bool? overridePublished = null)
        {
            var specificationAttributeOptionIds = filteredSpecOptionIds.ToIdArray();
            var filteredSpecOption = await _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(specificationAttributeOptionIds);

            var products = await _productService.SearchProductsAsync(pageIndex, pageSize, categoryIds,
                manufacturerIds, storeId, vendorId, warehouseId,
                productType == null ? null : productType, visibleIndividuallyOnly,
                excludeFeaturedProducts, priceMin, priceMax, productTagId, keywords,
                searchDescriptions, searchManufacturerPartNumber, searchSku, searchProductTags,
                languageId, filteredSpecOption, orderByType, showHidden,
                overridePublished);

            return Ok(products.ToPagedListDto<Product, ProductDto>());
        }

        /// <summary>
        /// Get products for which a discount is applied
        /// </summary>
        /// <param name="discountId">Discount identifier; pass null to load all records</param>
        /// <param name="showHidden">A value indicating whether to load deleted products</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Product, ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductsWithAppliedDiscount([FromQuery] int? discountId = null,
            [FromQuery] bool showHidden = false,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var products = await _productService.GetProductsWithAppliedDiscountAsync(discountId, showHidden,
                pageIndex, pageSize);

            return Ok(products.ToPagedListDto<Product, ProductDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductDto model)
        {
            var product = model.FromDto<Product>();

            await _productService.InsertProductAsync(product);

            var productDto = product.ToDto<ProductDto>();

            return Ok(productDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductDto model)
        {
            var product = await _productService.GetProductByIdAsync(model.Id);

            if (product == null)
                return NotFound($"Product Id={model.Id} is not found");

            product = model.FromDto<Product>();

            await _productService.UpdateProductAsync(product);

            return Ok();
        }

        #endregion

        #region Inventory management methods

        /// <summary>
        /// Adjust inventory
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="quantityToChange">Quantity to increase or decrease</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="message">Message for the stock quantity history</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AdjustInventory(int productId,
            [FromQuery, Required] int quantityToChange,
            [FromBody] string attributesXml = "",
            [FromQuery] string message = "")
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} is not found");

            await _productService.AdjustInventoryAsync(product, quantityToChange, attributesXml);

            return Ok();
        }

        /// <summary>
        /// Book the reserved quantity
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="quantity">Quantity, must be negative</param>
        /// <param name="message">Message for the stock quantity history</param>
        [HttpGet("{productId}/{warehouseId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> BookReservedInventory(int productId,
            int warehouseId,
            [FromQuery, Required] int quantity,
            [FromQuery] string message = "")
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} is not found");

            await _productService.BookReservedInventoryAsync(product, warehouseId, quantity, message);

            return Ok();
        }

        /// <summary>
        /// Reverse booked inventory (if acceptable)
        /// </summary>
        /// <param name="productId">product identifier</param>
        /// <param name="shipmentItemId">Shipment item identifier</param>
        /// <param name="message">Message for the stock quantity history</param>
        [HttpGet("{productId}/{shipmentItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ReverseBookedInventory(int productId, int shipmentItemId, [FromQuery] string message = "")
        {
            if (productId <= 0 || shipmentItemId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} is not found");

            var shipmentItem = await _shipmentItemRepository.GetByIdAsync(shipmentItemId);

            if (shipmentItem == null)
                return NotFound($"Shipment item Id={shipmentItemId} is not found");

            var count = await _productService.ReverseBookedInventoryAsync(product, shipmentItem, message);

            return Ok(count);
        }

        #endregion

        #region Stock quantity history

        /// <summary>
        /// Add stock quantity change entry
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="quantityAdjustment">Quantity adjustment</param>
        /// <param name="stockQuantity">Current stock quantity</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="message">Message</param>
        /// <param name="combinationId">Product attribute combination identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AddStockQuantityHistoryEntry(int productId,
            [FromQuery, Required] int quantityAdjustment,
            [FromQuery, Required] int stockQuantity,
            [FromQuery] int warehouseId = 0,
            [FromQuery] string message = "",
            [FromQuery] int? combinationId = null)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} is not found");

            await _productService.AddStockQuantityHistoryEntryAsync(product, quantityAdjustment,
                stockQuantity, warehouseId, message, combinationId);

            return Ok();
        }

        /// <summary>
        /// Get the history of the product stock quantity changes
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to load all entries</param>
        /// <param name="combinationId">Product attribute combination identifier; pass 0 to load all entries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PagedListDto<StockQuantityHistory, StockQuantityHistoryDto>),
            StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStockQuantityHistory(int productId,
            [FromQuery] int warehouseId = 0,
            [FromQuery] int combinationId = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            if (productId <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound($"Product Id={productId} is not found");

            var history = await _productService.GetStockQuantityHistoryAsync(product, warehouseId, combinationId,
                pageIndex, pageSize);

            return Ok(history.ToPagedListDto<StockQuantityHistory, StockQuantityHistoryDto>());
        }

        #endregion

        #endregion
    }
}
