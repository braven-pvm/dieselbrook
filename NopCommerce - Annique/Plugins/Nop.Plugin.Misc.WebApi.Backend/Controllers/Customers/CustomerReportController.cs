using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Customers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Customers
{
    public partial class CustomerReportController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerReportService _customerReportService;

        #endregion

        #region Ctor

        public CustomerReportController(ICustomerReportService customerReportService)
        {
            _customerReportService = customerReportService;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets a report of customers registered in the last days
        /// </summary>
        /// <param name="days">Customers registered in the last days</param>
        [HttpGet]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRegisteredCustomersReport([FromQuery][Required] int days)
        {
            var count = await _customerReportService.GetRegisteredCustomersReportAsync(days);

            return Ok(count);
        }

        /// <summary>
        /// Get best customers
        /// </summary>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="orderStatus">Order status; null to load all records</param>
        /// <param name="paymentStatus">Order payment status; null to load all records</param>
        /// <param name="shippingStatus">Order shipment status; null to load all records</param>
        /// <param name="orderBy">1 - order by order total, 2 - order by number of orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<BestCustomerReportLine, BestCustomerReportLineDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBestCustomersReport([FromQuery] DateTime? createdFromUtc,
            [FromQuery] DateTime? createdToUtc,
            [FromQuery] OrderStatus? orderStatus,
            [FromQuery] PaymentStatus? paymentStatus,
            [FromQuery] ShippingStatus? shippingStatus,
            [FromQuery, Required] OrderByEnum orderBy,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 214748364)
        {
            var report = await _customerReportService.GetBestCustomersReportAsync(createdFromUtc, createdToUtc,
                orderStatus,
                paymentStatus, shippingStatus, orderBy, pageIndex, pageSize);

            return Ok(report.ToPagedListDto<BestCustomerReportLine, BestCustomerReportLineDto>());
        }

        #endregion
    }
}
