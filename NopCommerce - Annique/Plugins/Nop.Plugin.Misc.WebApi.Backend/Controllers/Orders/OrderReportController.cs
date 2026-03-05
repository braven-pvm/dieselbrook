using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class OrderReportController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IOrderReportService _orderReportService;

        #endregion

        #region Ctor

        public OrderReportController(IOrderReportService orderReportService)
        {
            _orderReportService = orderReportService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get "order by country" report
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="os">Order status</param>
        /// <param name="ps">Payment status</param>
        /// <param name="ss">Shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<OrderByCountryReportLineResponse>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCountryReport([FromQuery] int storeId = 0,
            [FromQuery] OrderStatus? os = null,
            [FromQuery] PaymentStatus? ps = null,
            [FromQuery] ShippingStatus? ss = null,
            [FromQuery] DateTime? startTimeUtc = null,
            [FromQuery] DateTime? endTimeUtc = null)
        {
            var result = await _orderReportService.GetCountryReportAsync(storeId, os, ps, ss, startTimeUtc, endTimeUtc);

            var response = result.Select(item => item.ToDto<OrderByCountryReportLineResponse>()).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="osIds">Order status identifiers</param>
        /// <param name="psIds">Payment status identifiers</param>
        /// <param name="ssIds">Shipping status identifiers</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        [HttpGet]
        [ProducesResponseType(typeof(OrderAverageReportLineResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetOrderAverageReportLine([FromQuery] int storeId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] int productId = 0,
            [FromQuery] int warehouseId = 0,
            [FromQuery] int billingCountryId = 0,
            [FromQuery] int orderId = 0,
            [FromQuery] string paymentMethodSystemName = null,
            [FromQuery] string osIds = null,
            [FromQuery] string psIds = null,
            [FromQuery] string ssIds = null,
            [FromQuery] DateTime? startTimeUtc = null,
            [FromQuery] DateTime? endTimeUtc = null,
            [FromQuery] string billingPhone = null,
            [FromQuery] string billingEmail = null,
            [FromQuery] string billingLastName = "",
            [FromQuery] string orderNotes = null)
        {
            var orderStatusIds = osIds.ToIdArray();
            var paymentStatusIds = psIds.ToIdArray();
            var shippingStatusIds = ssIds.ToIdArray();

            var result = await _orderReportService.GetOrderAverageReportLineAsync(storeId, vendorId, productId, warehouseId, billingCountryId,
                orderId, paymentMethodSystemName, orderStatusIds.ToList(), paymentStatusIds.ToList(), shippingStatusIds.ToList(), startTimeUtc, endTimeUtc, billingPhone, 
                billingEmail, billingLastName, orderNotes);

            return Ok(result.ToDto<OrderAverageReportLineResponse>());
        }

        /// <summary>
        /// Get sales summary report
        /// </summary>
        /// <param name="storeId">Store identifier (orders placed in a specific store); 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="productId">Product identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="groupBy">0 - group by day, 1 - group by week, 2 - group by total month</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<SalesSummaryReportLine, SalesSummaryReportLineDto>),
            StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SalesSummaryReport([FromQuery] int categoryId = 0,
            [FromQuery] int productId = 0,
            [FromQuery] int manufacturerId = 0,
            [FromQuery] int storeId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] OrderStatus? os = null,
            [FromQuery] PaymentStatus? ps = null,
            [FromQuery] int billingCountryId = 0,
            [FromQuery] GroupByOptions groupBy = GroupByOptions.Day,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var result = await _orderReportService.SalesSummaryReportAsync(storeId, vendorId, categoryId, productId,
                manufacturerId, createdFromUtc, createdToUtc, os, ps, billingCountryId,
                groupBy, pageIndex, pageSize);

            return Ok(result.ToPagedListDto<SalesSummaryReportLine, SalesSummaryReportLineDto>());
        }

        /// <summary>
        /// Get best sellers report
        /// </summary>
        /// <param name="storeId">Store identifier (orders placed in a specific store); 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="orderBy">1 - order by quantity, 2 - order by total amount</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<BestsellersReportLine, BestsellersReportLineDto>),
            StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BestSellersReport([FromQuery] int categoryId = 0,
            [FromQuery] int manufacturerId = 0,
            [FromQuery] int storeId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] OrderStatus? os = null,
            [FromQuery] PaymentStatus? ps = null,
            [FromQuery] ShippingStatus? ss = null,
            [FromQuery] int billingCountryId = 0,
            [FromQuery] OrderByEnum orderBy = OrderByEnum.OrderByQuantity,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false)
        {
            var result = await _orderReportService.BestSellersReportAsync(categoryId, manufacturerId, storeId, vendorId,
                createdFromUtc, createdToUtc, os, ps, ss, billingCountryId,
                orderBy, pageIndex, pageSize, showHidden);

            return Ok(result.ToPagedListDto<BestsellersReportLine, BestsellersReportLineDto>());
        }

        /// <summary>
        /// Get best sellers total amount
        /// </summary>
        /// <param name="storeId">Store identifier (orders placed in a specific store); 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BestSellersReportTotalAmount([FromQuery] int categoryId = 0,
            [FromQuery] int manufacturerId = 0,
            [FromQuery] int storeId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] OrderStatus? os = null,
            [FromQuery] PaymentStatus? ps = null,
            [FromQuery] ShippingStatus? ss = null,
            [FromQuery] int billingCountryId = 0,
            [FromQuery] bool showHidden = false)
        {
            var result = await _orderReportService.BestSellersReportTotalAmountAsync(categoryId, manufacturerId,
                storeId, vendorId,
                createdFromUtc, createdToUtc, os, ps, ss, billingCountryId, showHidden);

            return Ok(result);
        }

        /// <summary>
        /// Get best sellers total amount
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Records to return</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{storeId}/{productId}")]
        [ProducesResponseType(typeof(int[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAlsoPurchasedProductsIds(int storeId,
            int productId,
            [FromQuery] int recordsToReturn = 5,
            [FromQuery] bool visibleIndividuallyOnly = true,
            [FromQuery] bool showHidden = false)
        {
            var result = await _orderReportService.GetAlsoPurchasedProductsIdsAsync(storeId, productId, recordsToReturn,
                visibleIndividuallyOnly, showHidden);

            return Ok(result);
        }

        /// <summary>
        /// Gets a list of products that were never sold
        /// </summary>
        /// <param name="vendorId">Vendor identifier (filter products by a specific vendor); 0 to load all records</param>
        /// <param name="storeId">Store identifier (filter products by a specific store); 0 to load all records</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Product, ProductDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProductsNeverSold([FromQuery] int vendorId = 0,
            [FromQuery] int storeId = 0,
            [FromQuery] int categoryId = 0,
            [FromQuery] int manufacturerId = 0,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false)
        {
            var result = await _orderReportService.ProductsNeverSoldAsync(categoryId, manufacturerId, storeId, vendorId,
                createdFromUtc, createdToUtc, pageIndex, pageSize, showHidden);

            return Ok(result.ToPagedListDto<Product, ProductDto>());
        }

        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="osIds">Order status identifiers; null to load all records</param>
        /// <param name="psIds">Payment status identifiers; null to load all records</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all records</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProfitReport([FromQuery] int storeId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] int productId = 0,
            [FromQuery] int warehouseId = 0,
            [FromQuery] int billingCountryId = 0,
            [FromQuery] int orderId = 0,
            [FromQuery] string paymentMethodSystemName = null,
            [FromQuery] string osIds = null,
            [FromQuery] string psIds = null,
            [FromQuery] string ssIds = null,
            [FromQuery] DateTime? startTimeUtc = null,
            [FromQuery] DateTime? endTimeUtc = null,
            [FromQuery] string billingPhone = null,
            [FromQuery] string billingEmail = null,
            [FromQuery] string billingLastName = "",
            [FromQuery] string orderNotes = null)
        {
            var orderStatusIds = osIds.ToIdArray();
            var paymentStatusIds = psIds.ToIdArray();
            var shipmentStatusIds = ssIds.ToIdArray();

            var result = await _orderReportService.ProfitReportAsync(storeId, vendorId, productId, warehouseId,
                billingCountryId, orderId, paymentMethodSystemName, orderStatusIds.ToList(), paymentStatusIds.ToList(),
                shipmentStatusIds.ToList(), startTimeUtc, endTimeUtc,
                billingPhone, billingEmail, billingLastName, orderNotes);

            return Ok(result);
        }

        #endregion
    }
}
