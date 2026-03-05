using Annique.Plugins.Nop.Customization.Domain.ShippingRule;
using Annique.Plugins.Nop.Customization.Factories.ShippingRule;
using Annique.Plugins.Nop.Customization.Models.ShippingRule;
using Annique.Plugins.Nop.Customization.Services.ShippingRule;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class AdminCustomShippingRuleController : BaseAdminController
    {
        #region Fields

        private readonly ICustomShippingRuleFactory _customShippingRuleFactory;
        private readonly ICustomShippingRuleService _customShippingRuleService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IAclService _aclService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public AdminCustomShippingRuleController(ICustomShippingRuleFactory customShippingRuleFactory,
            ICustomShippingRuleService customShippingRuleService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IAclService aclService,
            ICustomerService customerService)
        {
            _customShippingRuleFactory = customShippingRuleFactory;
            _customShippingRuleService = customShippingRuleService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _aclService = aclService;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveCustomShippingRuleAclAsync(CustomShippingByWeightByTotalRecord customShippingByWeight, CustomShippingRuleModel model)
        {
            customShippingByWeight.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _customShippingRuleService.UpdateCustomShippingByWeightRecordAsync(customShippingByWeight);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(customShippingByWeight);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                        await _aclService.InsertAclRecordAsync(customShippingByWeight, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        #endregion
        #region Methods

        public virtual async Task<IActionResult> List()
        {
            //prepare model
            var model = await _customShippingRuleFactory.PrepareCustomShippingRuleSearchModelAsync(new CustomShippingRuleSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CustomShippingRuleSearchModel searchModel)
        {
            //prepare model
            var model = await _customShippingRuleFactory.PrepareCustomShippingRuleListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            //prepare model
            var model = await _customShippingRuleFactory.PrepareCustomShippingRuleModelAsync(new CustomShippingRuleModel(), new CustomShippingByWeightByTotalRecord());

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CustomShippingRuleModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customShippingRule = _customShippingRuleFactory.PrepareCustomShippingRuleFields(model);

                await _customShippingRuleService.InsertCustomShippingByWeightRecordAsync(customShippingRule);

                //ACL (customer roles)
                await SaveCustomShippingRuleAclAsync(customShippingRule, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.CustomShippingRule.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = customShippingRule.Id });
            }

            //prepare model
            model = await _customShippingRuleFactory.PrepareCustomShippingRuleModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            //try to get a shipping rule with the specified id
            var shippingRule = await _customShippingRuleService.GetByIdAsync(id);
            if (shippingRule == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _customShippingRuleFactory.PrepareCustomShippingRuleModelAsync(null, shippingRule);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CustomShippingRuleModel model, bool continueEditing)
        {
            //try to get a shipping rule with the specified id
            var shippingRule = await _customShippingRuleService.GetByIdAsync(model.Id);
            if (shippingRule == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var updateShippingRule = _customShippingRuleFactory.PrepareCustomShippingRuleFields(model);
              
                await _customShippingRuleService.UpdateCustomShippingByWeightRecordAsync(updateShippingRule);

                //ACL (customer roles)
                await SaveCustomShippingRuleAclAsync(updateShippingRule, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.CustomShippingRule.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = updateShippingRule.Id });
            }

            //prepare model
            model = await _customShippingRuleFactory.PrepareCustomShippingRuleModelAsync(model, shippingRule, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CustomRuleDelete(int id)
        {
            //try to get a rule with the specified id
            var rule = await _customShippingRuleService.GetByIdAsync(id);
            if (rule == null)
                return RedirectToAction("List");

            await _customShippingRuleService.DeleteCustomShippingByWeightRecordAsync(rule);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.CustomShippingRule.Deleted"));

            return new NullJsonResult();
        }

        #endregion
    }
}
