using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Vendors;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Vendors;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class VendorController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public VendorController(
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IVendorModelFactory vendorModelFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            VendorSettings vendorSettings)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _htmlFormatter = htmlFormatter;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _vendorModelFactory = vendorModelFactory;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities
        
        protected virtual async Task UpdatePictureSeoNamesAsync(Vendor vendor)
        {
            var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
            if (picture != null)
                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(vendor.Name));
        }
        
        protected virtual async Task<string> ParseVendorAttributesAsync(IDictionary<string, string> form)
        {
            var attributesXml = string.Empty;

            if (form == null)
                return attributesXml;

            var attributes = await _vendorAttributeService.GetAllVendorAttributesAsync();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopVendorDefaults.VendorAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                        break;
                    case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                            foreach (var item in cblAttributes.Split(new[] { ',' },
                                StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                    }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _vendorAttributeService.GetVendorAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                            attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml, attribute,
                                selectedAttributeId.ToString());
                    }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.Trim();
                            attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }

                        break;
                }
            }

            return attributesXml;
        }

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(typeof(ApplyVendorModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ApplyVendor()
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return BadRequest();

            var model = new ApplyVendorModel();
            model = await _vendorModelFactory.PrepareApplyVendorModelAsync(model, true, false, null);

            return Ok(model.ToDto<ApplyVendorModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApplyVendorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ApplyVendorSubmit([FromBody] ApplyVendorRequest request,
            [FromQuery, Required] string contentType)
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return BadRequest(
                    $"The setting {nameof(_vendorSettings.AllowCustomersToApplyForVendorAccount)} is not enabled.");
            
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (await _customerService.IsAdminAsync(customer))
                return BadRequest(await _localizationService.GetResourceAsync("Vendors.ApplyAccount.IsAdmin"));

            var pictureId = 0;

            if (request.PictureBinary.Any())
                try
                {
                    var picture = await _pictureService.InsertPictureAsync(request.PictureBinary, contentType, null);

                    if (picture != null)
                        pictureId = picture.Id;
                }
                catch (Exception)
                {
                    return BadRequest(
                        await _localizationService.GetResourceAsync("Vendors.ApplyAccount.Picture.ErrorMessage"));
                }

            //vendor attributes
            var vendorAttributesXml = await ParseVendorAttributesAsync(request.Form);
            var warnings = (await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml)).ToList();
            foreach (var warning in warnings) 
                ModelState.AddModelError(string.Empty, warning);

            var vendorModel = request.Model.FromDto<ApplyVendorModel>();

            var description =
                _htmlFormatter.FormatText(vendorModel.Description, false, false, true, false, false, false);
            //disabled by default
            var vendor = new Vendor
            {
                Name = vendorModel.Name,
                Email = vendorModel.Email,
                //some default settings
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions,
                PictureId = pictureId,
                Description = description
            };
            await _vendorService.InsertVendorAsync(vendor);
            //search engine name (the same as vendor name)
            var seName = await _urlRecordService.ValidateSeNameAsync(vendor, vendor.Name, vendor.Name, true);
            await _urlRecordService.SaveSlugAsync(vendor, seName, 0);

            //associate to the current customer
            //but a store owner will have to manually add this customer role to "Vendors" role
            //if he wants to grant access to admin area
            customer.VendorId = vendor.Id;
            await _customerService.UpdateCustomerAsync(customer);

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(vendor);

            //save vendor attributes
            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes,
                vendorAttributesXml);

            //notify store owner here (email)
            await _workflowMessageService.SendNewVendorAccountApplyStoreOwnerNotificationAsync(
                customer,
                vendor, _localizationSettings.DefaultAdminLanguageId);

            vendorModel.DisableFormInput = true;
            vendorModel.Result = await _localizationService.GetResourceAsync("Vendors.ApplyAccount.Submitted");

            return Ok(vendorModel.ToDto<ApplyVendorModelDto>());
        }

        [HttpGet]
        [ProducesResponseType(typeof(VendorInfoModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Info()
        {
            if (await _workContext.GetCurrentVendorAsync() == null || !_vendorSettings.AllowVendorsToEditInfo)
                return BadRequest();

            var model = new VendorInfoModel();
            model = await _vendorModelFactory.PrepareVendorInfoModelAsync(model, false);

            return Ok(model.ToDto<VendorInfoModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(VendorInfoModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Info([FromBody] InfoRequest request, [FromQuery][Required] string contentType)
        {
            if (await _workContext.GetCurrentVendorAsync() == null || !_vendorSettings.AllowVendorsToEditInfo)
                return NotFound("The current vendor is null or vendor can not edit information about itself.");

            Picture picture = null;

            if (request.PictureBinary.Any())
                try
                {
                    picture = await _pictureService.InsertPictureAsync(request.PictureBinary, contentType, null);
                }
                catch (Exception)
                {
                    return BadRequest(new List<string>
                    {
                        await _localizationService.GetResourceAsync("Account.VendorInfo.Picture.ErrorMessage")
                    });
                }

            var vendor = await _workContext.GetCurrentVendorAsync();
            var prevPicture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);

            //vendor attributes
            var vendorAttributesXml = await ParseVendorAttributesAsync(request.Form);
            var attrributeWarnings = await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml);

            if (attrributeWarnings.Any())
                return BadRequest(attrributeWarnings);
            
            var vendorModel = request.Model.FromDto<VendorInfoModel>();

            var description =
                _htmlFormatter.FormatText(vendorModel.Description, false, false, true, false, false, false);

            vendor.Name = vendorModel.Name;
            vendor.Email = vendorModel.Email;
            vendor.Description = description;

            if (picture != null)
            {
                vendor.PictureId = picture.Id;

                if (prevPicture != null)
                    await _pictureService.DeletePictureAsync(prevPicture);
            }

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(vendor);

            await _vendorService.UpdateVendorAsync(vendor);

            //save vendor attributes
            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes,
                vendorAttributesXml);

            //notifications
            if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                await _workflowMessageService.SendVendorInformationChangeStoreOwnerNotificationAsync(vendor,
                    _localizationSettings.DefaultAdminLanguageId);

            return Ok(vendorModel.ToDto<VendorInfoModelDto>());
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> RemovePicture()
        {
            var vendor = await _workContext.GetCurrentVendorAsync();

            if (vendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return BadRequest();

            var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);

            if (picture != null)
                await _pictureService.DeletePictureAsync(picture);

            vendor.PictureId = 0;
            await _vendorService.UpdateVendorAsync(vendor);

            //notifications
            if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                await _workflowMessageService.SendVendorInformationChangeStoreOwnerNotificationAsync(vendor, _localizationSettings.DefaultAdminLanguageId);

            return Ok();
        }

        #endregion
    }
}