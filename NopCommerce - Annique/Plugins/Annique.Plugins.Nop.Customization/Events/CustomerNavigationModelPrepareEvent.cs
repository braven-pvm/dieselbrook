using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Annique.Plugins.Nop.Customization.Models.AnniqueReports.Public;
using Annique.Plugins.Nop.Customization.Services.AnniqueReport;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Customer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    public class CustomerNavigationModelPrepareEvent : IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAnniqueReportService _anniqueReportService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public CustomerNavigationModelPrepareEvent(ILocalizationService localizationService,
           ISettingService settingService,
           IStoreContext storeContext,
           IAnniqueReportService anniqueReportService,
           IWorkContext workContext,
           ICustomerService customerService,
           IUrlRecordService urlRecordService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _anniqueReportService = anniqueReportService;
            _workContext = workContext;
            _customerService = customerService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Method

        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            //get active store
            var storeScope = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

            //Get current customer and customer roles
            var customer = await _workContext.GetCurrentCustomerAsync();

            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            if (eventMessage.Model is CustomerNavigationModel)
            {
                var model = eventMessage.Model as CustomerNavigationModel;

                //if plugin not enabled or customer has not consultant role then return do not create my account and reports tab
                if (settings.IsEnablePlugin && customerRoleIds.Contains(settings.ConsultantRoleId))
                {
                    var myaccountItem = model.CustomerNavigationItems.Any(m => m.Tab == (int)CustomMyAccountNavigationEnum.MyAccount);
                    if (!myaccountItem)
                    {
                        var customerNavigationItemModel = new CustomerNavigationItemModel
                        {
                            RouteName = "MyAccountInfo",
                            Title = await _localizationService.GetResourceAsync("Account.MyAccount"),
                            Tab = (int)CustomMyAccountNavigationEnum.MyAccount,
                            ItemClass = "customer-info"
                        };
                        model.CustomerNavigationItems.Insert(0, customerNavigationItemModel);
                    }

                    //Get Customer reports
                    var reports = await _anniqueReportService.GetAllCustomerReportsAsync();

                    if (reports.Any())
                    {
                        //check other report list available or not
                        if (reports.Where(r => !r.IsMenuOption).Any())
                        {
                            //if other report list available then show reports menu 
                            var item = model.CustomerNavigationItems.Any(m => m.Tab == (int)CustomMyAccountNavigationEnum.Reports);
                            if (!item)
                            {
                                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                                {
                                    RouteName = "CustomerReports",
                                    Title = await _localizationService.GetResourceAsync("Account.Reports"),
                                    Tab = (int)CustomMyAccountNavigationEnum.Reports,
                                    ItemClass = "customer-reports"
                                });
                            } 
                        }


                        // get the reports which has menu option enabled and not publicly hosted
                        var menuOptionReports = reports
                            .Where(r => r.IsMenuOption && !r.PubliclyHostedPage)
                            .OrderBy(r => r.TabOrder)
                            .ToList();

                        if (!menuOptionReports.Any())
                            return;

                        foreach (var report in menuOptionReports)
                        {
                            // ONE lightweight DB call instead of GetSeName + Validate
                            var seName = await _urlRecordService.GetActiveSlugAsync(
                                report.Id,
                                nameof(Report),
                                0
                            );

                            // generate slug only if missing
                            if (string.IsNullOrEmpty(seName))
                            {
                                seName = await _urlRecordService.ValidateSeNameAsync(
                                    report,
                                    string.Empty,
                                    report.Name,
                                    true
                                );

                                await _urlRecordService.SaveSlugAsync(report, seName, 0);
                            }

                            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                            {
                                RouteName = "Report_" + seName,
                                Title = string.IsNullOrEmpty(report.CustomText)
                                    ? report.Name
                                    : report.CustomText,
                                Tab = report.TabOrder,
                                ItemClass = "customer-reports"
                            });
                        }
                    }
                }
            }
        }

        #endregion
    }
}

