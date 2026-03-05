using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Sitemap;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Themes;
using Nop.Web.Models.Common;
using Nop.Web.Models.Sitemap;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class CommonController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly ISitemapModelFactory _sitemapModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly SitemapSettings _sitemapSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public CommonController(
            CommonSettings commonSettings,
            ICommonModelFactory commonModelFactory,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            ISitemapModelFactory sitemapModelFactory,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            SitemapSettings sitemapSettings,
            StoreInformationSettings storeInformationSettings,
            VendorSettings vendorSettings)
        {
            _commonSettings = commonSettings;
            _commonModelFactory = commonModelFactory;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _htmlFormatter = htmlFormatter;
            _languageService = languageService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _sitemapModelFactory = sitemapModelFactory;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _sitemapSettings = sitemapSettings;
            _storeInformationSettings = storeInformationSettings;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get language selector
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(LanguageSelectorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLanguageSelector()
        {
            var model = await _commonModelFactory.PrepareLanguageSelectorModelAsync();

            return Ok(model.ToDto<LanguageSelectorModelDto>());
        }
        
        /// <summary>
        /// Set language
        /// </summary>
        [HttpPost("{langId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SetLanguage(int langId)
        {
            var language = await _languageService.GetLanguageByIdAsync(langId);
            if (!language?.Published ?? false)
                language = await _workContext.GetWorkingLanguageAsync();

            await _workContext.SetWorkingLanguageAsync(language);

            return Ok();
        }

        /// <summary>
        /// Get currency selector
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CurrencySelectorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCurrencySelector()
        {
            var model = await _commonModelFactory.PrepareCurrencySelectorModelAsync();

            return Ok(model.ToDto<CurrencySelectorModelDto>());
        }

        /// <summary>
        /// Set currency
        /// </summary>
        [HttpPost("{customerCurrencyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SetCurrency(int customerCurrencyId)
        {
            var currency = await _currencyService.GetCurrencyByIdAsync(customerCurrencyId);
            if (currency != null)
                await _workContext.SetWorkingCurrencyAsync(currency);

            return Ok();
        }

        /// <summary>
        /// Get tax selector
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(TaxTypeSelectorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetTaxSelector()
        {
            var model = await _commonModelFactory.PrepareTaxTypeSelectorModelAsync();

            return Ok(model.ToDto<TaxTypeSelectorModelDto>());
        }

        /// <summary>
        /// Set tax type
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SetTaxType([FromQuery, Required] TaxDisplayType customerTaxType)
        {
            await _workContext.SetTaxDisplayTypeAsync(customerTaxType);

            return Ok();
        }

        /// <summary>
        /// Contact us page
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ContactUsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ContactUs()
        {
            var model = new ContactUsModel();
            model = await _commonModelFactory.PrepareContactUsModelAsync(model, false);

            return Ok(model.ToDto<ContactUsModelDto>());
        }

        /// <summary>
        /// Contact us send
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ContactUsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ContactUsSend([FromBody] ContactUsModelDto model)
        {
            var contactUsModel = await _commonModelFactory.PrepareContactUsModelAsync(model.FromDto<ContactUsModel>(), true);

            var subject = _commonSettings.SubjectFieldOnContactUsForm ? contactUsModel.Subject : null;
            var body = _htmlFormatter.FormatText(contactUsModel.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactUsMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                contactUsModel.Email.Trim(), contactUsModel.FullName, subject, body);

            contactUsModel.SuccessfullySent = true;
            contactUsModel.Result = await _localizationService.GetResourceAsync("ContactUs.YourEnquiryHasBeenSent");

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ContactUs",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ContactUs"));

            return Ok(contactUsModel.ToDto<ContactUsModelDto>());

        }

        /// <summary>
        /// contact vendor page
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        [HttpGet("{vendorId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ContactVendorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ContactVendor(int vendorId)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return NotFound($"The setting {nameof(_vendorSettings.AllowCustomersToContactVendors)} is not enabled.");

            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return NotFound($"The manufacturer by id={vendorId} is not found.");

            var model = new ContactVendorModel();
            model = await _commonModelFactory.PrepareContactVendorModelAsync(model, vendor, false);

            return Ok(model.ToDto<ContactVendorModelDto>());
        }

        /// <summary>
        /// Contact vendor vend
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ContactVendorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ContactVendorSend([FromBody] ContactVendorModelDto model)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return NotFound($"The setting {nameof(_vendorSettings.AllowCustomersToContactVendors)} is not enabled.");

            var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return NotFound($"The manufacturer by id={model.VendorId} is not found.");

            var contactVendorModel = await _commonModelFactory.PrepareContactVendorModelAsync(model.FromDto<ContactVendorModel>(), vendor, true);

            var subject = _commonSettings.SubjectFieldOnContactUsForm ? contactVendorModel.Subject : null;
            var body = _htmlFormatter.FormatText(contactVendorModel.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactVendorMessageAsync(vendor, (await _workContext.GetWorkingLanguageAsync()).Id,
                contactVendorModel.Email.Trim(), contactVendorModel.FullName, subject, body);

            contactVendorModel.SuccessfullySent = true;
            contactVendorModel.Result = await _localizationService.GetResourceAsync("ContactVendor.YourEnquiryHasBeenSent");

            return Ok(contactVendorModel.ToDto<ContactVendorModelDto>());
        }

        /// <summary>
        /// sitemap page
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SitemapModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Sitemap([FromBody] SitemapPageModelDto pageModel)
        {
            if (!_sitemapSettings.SitemapEnabled)
                return NotFound($"The setting {nameof(_sitemapSettings.SitemapEnabled)} is not enabled.");

            var model = await _sitemapModelFactory.PrepareSitemapModelAsync(pageModel.FromDto<SitemapPageModel>());

            return Ok(model.ToDto<SitemapModelDto>());
        }

        /// <summary>
        /// SEO sitemap page
        /// </summary>
        /// <param name="id">Sitemap identifier; pass 0 to load the first sitemap or sitemap index file</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SitemapXmlResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SitemapXml(int id)
        {
            var siteMap = string.Empty;

            var model = await _sitemapModelFactory.PrepareSitemapXmlModelAsync(id);

            if (!string.IsNullOrEmpty(model.SitemapXmlPath)) 
                siteMap = await _fileProvider.ReadAllTextAsync(model.SitemapXmlPath, Encoding.UTF8);

            var response = new SitemapXmlResponse
            {
                MimeType = "text/xml",
                SiteMapXML = siteMap
            };
            return Ok(response);
        }

        /// <summary>
        /// Set store theme
        /// </summary>
        /// <param name="themeName">Theme name</param>
        [HttpGet]
        public virtual async Task<IActionResult> SetStoreTheme([FromQuery][Required] string themeName)
        {
            await _themeContext.SetWorkingThemeNameAsync(themeName);
            return Ok();
        }

        /// <summary>
        /// Eu cookie law accept
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> EuCookieLawAccept()
        {
            if (!_storeInformationSettings.DisplayEuCookieLawWarning)
                //disabled
                return Ok(false);

            //save setting
            var store = await _storeContext.GetCurrentStoreAsync();
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.EuCookieLawAcceptedAttribute, true, store.Id);
            return Ok(true);
        }

        /// <summary>
        /// robots.txt file
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(RobotsTextFileResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RobotsTextFile()
        {
            var robotsFileContent = await _commonModelFactory.PrepareRobotsTextFileAsync();
            var response = new RobotsTextFileResponse
            {
                RobotsFileContent = robotsFileContent,
                MimeType = MimeTypes.TextPlain
            };
            return Ok(response);
        }

        #endregion
    }
}
