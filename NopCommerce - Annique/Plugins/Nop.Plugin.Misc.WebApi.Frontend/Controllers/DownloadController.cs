using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Media;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class DownloadController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPdfService _pdfService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public DownloadController(CustomerSettings customerSettings,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPdfService pdfService,
            IProductService productService,
            IWorkContext workContext)
        {
            _customerSettings = customerSettings;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _orderService = orderService;
            _pdfService = pdfService;
            _productService = productService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> WebPdfInvoice(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id = {orderId} not found");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order.Deleted || customer.Id != order.CustomerId)
                return BadRequest("The order is deleted or belongs to another customer");

            var orders = new List<Order> { order };
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                var language = await _workContext.GetWorkingLanguageAsync();

                await _pdfService.PrintOrdersToPdfAsync(stream, orders, language);
                bytes = stream.ToArray();
            }
            
            return new FileContentResult(bytes, MimeTypes.ApplicationPdf) { FileDownloadName = $"order_{order.CustomOrderNumber}.pdf" };
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DownloadResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PdfInvoice(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order Id = {orderId} not found");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order.Deleted || customer.Id != order.CustomerId)
                return BadRequest("The order is deleted or belongs to another customer");

            var orders = new List<Order> { order };
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                var language = await _workContext.GetWorkingLanguageAsync();

                await _pdfService.PrintOrdersToPdfAsync(stream, orders, language);
                bytes = stream.ToArray();
            }
            
            return Ok(new DownloadResponse
            {
                FileName = $"order_{order.CustomOrderNumber}.pdf",
                DownloadBinary = Convert.ToBase64String(bytes)
            });
        }

        /// <summary>
        /// Sample
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> WebSample(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"The product by id={productId} is not found.");

            if (!product.HasSampleDownload)
                return NotFound("Product doesn't have a sample download.");

            var download = await _downloadService.GetDownloadByIdAsync(product.SampleDownloadId);
            if (download == null)
                return NotFound("Sample download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return Redirect(download.DownloadUrl);

            if (download.DownloadBinary == null)
                return NotFound("Download data is not available any more.");

            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        /// <summary>
        /// Sample
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DownloadResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Sample(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound($"The product by id={productId} is not found.");

            if (!product.HasSampleDownload)
                return NotFound("Product doesn't have a sample download.");

            var download = await _downloadService.GetDownloadByIdAsync(product.SampleDownloadId);
            if (download == null)
                return NotFound("Sample download is not available any more.");

            if (download.UseDownloadUrl)
                return Ok(new DownloadResponse { RedirectToUrl = download.DownloadUrl });

            if (download.DownloadBinary == null)
                return NotFound("Download data is not available any more.");

            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            
            return Ok(new DownloadResponse
            {
                FileName = fileName + download.Extension,
                DownloadBinary = Convert.ToBase64String(download.DownloadBinary)
            });
        }

        /// <summary>
        /// Get download
        /// </summary>
        /// <param name="orderItemGuid">Order item GUID</param>
        /// <param name="agree">Is agree</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DownloadResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetDownload([FromQuery, Required] Guid orderItemGuid,
            [FromQuery] bool agree = false)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemGuid);
            if (orderItem == null)
                return NotFound($"The order item by GUID={orderItemGuid} is not found.");

            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

            if (!await _orderService.IsDownloadAllowedAsync(orderItem))
                return BadRequest("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                if (order.CustomerId != customer.Id)
                    return BadRequest("This is not your order");
            }

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            if (product == null)
                return NotFound("Product is not available any more.");

            var download = await _downloadService.GetDownloadByIdAsync(product.DownloadId);
            if (download == null)
                return NotFound("Download is not available any more.");

            if (product.HasUserAgreement && !agree)
                return BadRequest("Not agree with the user agreement");

            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return BadRequest(string.Format(
                    await _localizationService.GetResourceAsync("DownloadableProducts.ReachedMaximumNumber"),
                    product.MaxNumberOfDownloads));

            if (download.UseDownloadUrl)
            {
                //increase download
                orderItem.DownloadCount++;
                await _orderService.UpdateOrderItemAsync(orderItem);

                return Ok(new DownloadResponse { RedirectToUrl = download.DownloadUrl });
            }

            //binary download
            if (download.DownloadBinary == null)
                return NotFound("Download data is not available any more.");

            //increase download
            orderItem.DownloadCount++;
            await _orderService.UpdateOrderItemAsync(orderItem);

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            
            return Ok(new DownloadResponse
            {
                FileName = fileName + download.Extension,
                DownloadBinary = Convert.ToBase64String(download.DownloadBinary)
            });
        }

        /// <summary>
        /// Get download
        /// </summary>
        /// <param name="orderItemGuid">Order item GUID</param>
        /// <param name="agree">Is agree</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> WebDownload([FromQuery, Required] Guid orderItemGuid,
            [FromQuery] bool agree = false)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemGuid);
            if (orderItem == null)
                return NotFound($"The order item by GUID={orderItemGuid} is not found.");

            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

            if (!await _orderService.IsDownloadAllowedAsync(orderItem))
                return BadRequest("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                if (order.CustomerId != customer.Id)
                    return BadRequest("This is not your order");
            }

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            if (product == null)
                return NotFound("Product is not available any more.");

            var download = await _downloadService.GetDownloadByIdAsync(product.DownloadId);
            if (download == null)
                return NotFound("Download is not available any more.");

            if (product.HasUserAgreement && !agree)
                return BadRequest("Not agree with the user agreement");

            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return BadRequest(string.Format(
                    await _localizationService.GetResourceAsync("DownloadableProducts.ReachedMaximumNumber"),
                    product.MaxNumberOfDownloads));

            if (download.UseDownloadUrl)
            {
                //increase download
                orderItem.DownloadCount++;
                await _orderService.UpdateOrderItemAsync(orderItem);

                //return result
                //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
                //In this case, it is not relevant. Url may not be local.
                return Redirect(download.DownloadUrl);
            }

            //binary download
            if (download.DownloadBinary == null)
                return NotFound("Download data is not available any more.");

            //increase download
            orderItem.DownloadCount++;
            await _orderService.UpdateOrderItemAsync(orderItem);

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();

            var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : MimeTypes.ApplicationOctetStream;

            return new FileContentResult(download.DownloadBinary, contentType)
            {
                FileDownloadName = fileName + download.Extension
            };
        }

        /// <summary>
        /// Get license
        /// </summary>
        /// <param name="orderItemGuid">Order item GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> GetLicense([FromQuery][Required] Guid orderItemGuid)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemGuid);
            if (orderItem == null)
                return NotFound($"The order item by GUID={orderItemGuid} is not found.");

            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

            if (!await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
                return NotFound("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                if (customer == null || order.CustomerId != customer.Id)
                    return BadRequest("This is not your order");
            }

            var download = await _downloadService.GetDownloadByIdAsync(orderItem.LicenseDownloadId ?? 0);
            if (download == null)
                return NotFound("Download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return Redirect(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return NotFound("Download data is not available any more.");

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : orderItem.ProductId.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        /// <summary>
        /// Get file upload
        /// </summary>
        /// <param name="downloadGuid">Download GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> GetFileUpload([FromQuery][Required] Guid downloadGuid)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
            if (download == null)
                return NotFound("Download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return Redirect(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return NotFound("Download data is not available any more.");

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadGuid.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
           
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        /// <summary>
        /// Get order note file
        /// </summary>
        /// <param name="orderNoteId">Order note identifier</param>
        [HttpGet("{orderNoteId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> GetOrderNoteFile(int orderNoteId)
        {
            var orderNote = await _orderService.GetOrderNoteByIdAsync(orderNoteId);
            if (orderNote == null)
                return NotFound($"The order note by id={orderNoteId} is not found.");

            var order = await _orderService.GetOrderByIdAsync(orderNote.OrderId);

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (customer == null || order.CustomerId != customer.Id)
                return BadRequest("This is not your order");

            var download = await _downloadService.GetDownloadByIdAsync(orderNote.DownloadId);
            if (download == null)
                return NotFound("Download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return Redirect(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return NotFound("Download data is not available any more.");

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : orderNote.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        #endregion
    }
}
