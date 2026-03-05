using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.ReturnRequests;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class ReturnRequestController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IReturnRequestModelFactory _returnRequestModelFactory;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public ReturnRequestController(ICustomerService customerService,
            ICustomNumberFormatter customNumberFormatter,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IReturnRequestModelFactory returnRequestModelFactory,
            IReturnRequestService returnRequestService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings)
        {
            _customerService = customerService;
            _customNumberFormatter = customNumberFormatter;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _returnRequestModelFactory = returnRequestModelFactory;
            _returnRequestService = returnRequestService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(typeof(CustomerReturnRequestsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CustomerReturnRequests()
        {
            var model = await _returnRequestModelFactory.PrepareCustomerReturnRequestsModelAsync();
            
            return Ok(model.ToDto<CustomerReturnRequestsModelDto>());
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(SubmitReturnRequestModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ReturnRequest(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return BadRequest("This order does not belong to the current customer");

            if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
                return BadRequest("Return request is not allowed.");

            var model = new SubmitReturnRequestModel();
            model = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(model, order);

            return Ok(model.ToDto<SubmitReturnRequestModelDto>());
        }

        [HttpPost("{orderId}")]
        [ProducesResponseType(typeof(SubmitReturnRequestModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ReturnRequestSubmit([FromBody] BaseModelDtoRequest<SubmitReturnRequestModelDto> request, int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return BadRequest("This order does not belong to the current customer");

            if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
                return BadRequest("Return request is not allowed.");

            var count = 0;

            var downloadId = 0;
            var returnRequestModel = request.Model.FromDto<SubmitReturnRequestModel>();

            if (_orderSettings.ReturnRequestsAllowFiles)
            {
                var download = await _downloadService.GetDownloadByGuidAsync(returnRequestModel.UploadedFileGuid);
                if (download != null)
                    downloadId = download.Id;
            }

            //returnable products
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isNotReturnable: false);
            foreach (var orderItem in orderItems)
            {
                var quantity = 0; //parse quantity
                foreach (var formKey in request.Form.Keys)
                    if (formKey.Equals($"quantity{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = int.TryParse(request.Form[formKey], out quantity);
                        break;
                    }

                if (quantity <= 0) 
                    continue;

                var rrr = await _returnRequestService.GetReturnRequestReasonByIdAsync(returnRequestModel.ReturnRequestReasonId);
                var rra = await _returnRequestService.GetReturnRequestActionByIdAsync(returnRequestModel.ReturnRequestActionId);
                var store = await _storeContext.GetCurrentStoreAsync();


                var rr = new ReturnRequest
                {
                    CustomNumber = string.Empty,
                    StoreId = store.Id,
                    OrderItemId = orderItem.Id,
                    Quantity = quantity,
                    CustomerId = customer.Id,
                    ReasonForReturn = rrr != null ? await _localizationService.GetLocalizedAsync(rrr, x => x.Name) : "not available",
                    RequestedAction = rra != null ? await _localizationService.GetLocalizedAsync(rra, x => x.Name) : "not available",
                    CustomerComments = returnRequestModel.Comments,
                    UploadedFileId = downloadId,
                    StaffNotes = string.Empty,
                    ReturnRequestStatus = ReturnRequestStatus.Pending,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _returnRequestService.InsertReturnRequestAsync(rr);

                //set return request custom number
                rr.CustomNumber = _customNumberFormatter.GenerateReturnRequestCustomNumber(rr);
                await _customerService.UpdateCustomerAsync(customer);
                await _returnRequestService.UpdateReturnRequestAsync(rr);

                //notify store owner
                await _workflowMessageService.SendNewReturnRequestStoreOwnerNotificationAsync(rr, orderItem, order, _localizationSettings.DefaultAdminLanguageId);
                //notify customer
                await _workflowMessageService.SendNewReturnRequestCustomerNotificationAsync(rr, orderItem, order);

                count++;
            }

            returnRequestModel = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(returnRequestModel, order);
            if (count > 0)
                returnRequestModel.Result = await _localizationService.GetResourceAsync("ReturnRequests.Submitted");
            else
                returnRequestModel.Result = await _localizationService.GetResourceAsync("ReturnRequests.NoItemsSubmitted");

            return Ok(returnRequestModel.ToDto<SubmitReturnRequestModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UploadFileReturnRequest([FromBody] byte[] fileBinary,
            [FromQuery, Required] string fileName,
            [FromQuery, Required] string contentType)
        {
            if (!_orderSettings.ReturnRequestsEnabled || !_orderSettings.ReturnRequestsAllowFiles)
                return Ok(new UploadFileResponse {Success = false, DownloadGuid = Guid.Empty});

            if (!fileBinary.Any())
                return Ok(new UploadFileResponse
                {
                    Success = false, Message = "No file uploaded", DownloadGuid = Guid.Empty
                });

            var qqFileNameParameter = "qqfilename";
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            var validationFileMaximumSize = _orderSettings.ReturnRequestsFileMaximumSize;
            if (validationFileMaximumSize > 0)
            {
                //compare in bytes
                var maxFileSizeBytes = validationFileMaximumSize * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                    return Ok(new UploadFileResponse
                    {
                        Success = false,
                        Message = string.Format(
                            await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"),
                            validationFileMaximumSize),
                        DownloadGuid = Guid.Empty,
                    });
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = string.Empty,
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Ok(new UploadFileResponse
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
                DownloadUrl = Url.Action("GetFileUpload", "Download", new {downloadId = download.DownloadGuid}),
                DownloadGuid = download.DownloadGuid,
            });
        }

        #endregion
    }
}