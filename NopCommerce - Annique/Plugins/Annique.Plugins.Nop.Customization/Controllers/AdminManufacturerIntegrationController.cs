using System.Threading.Tasks;
using Annique.Plugins.Nop.Customization.Factories;
using Annique.Plugins.Nop.Customization.Models.ManufacturerIntegration;
using Annique.Plugins.Nop.Customization.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class AdminManufacturerIntegrationController : BaseAdminController
    {
        #region Fields

        private readonly IManufacturerIntegrationModelFactory _manufacturerIntegrationModelFactory;
        private readonly IManufacturerIntegrationService _manufacturerIntegrationService;
        private readonly IManufacturerService _manufacturerService;

        #endregion

        #region Ctor

        public AdminManufacturerIntegrationController(IManufacturerIntegrationModelFactory manufacturerIntegrationModelFactory, 
            IManufacturerIntegrationService manufacturerIntegrationService, 
            IManufacturerService manufacturerService)
        {
            _manufacturerIntegrationModelFactory = manufacturerIntegrationModelFactory;
            _manufacturerIntegrationService = manufacturerIntegrationService;
            _manufacturerService = manufacturerService;
        }

        #endregion

        #region Manufacturer Integration List / Add / Delete

        [HttpPost]
        public virtual async Task<IActionResult> ManufacturerIntegrationList(ManufacturerIntegrationSearchModel searchModel)
        {
            var manufcaturer = await _manufacturerService.GetManufacturerByIdAsync(searchModel.ManufacturerId);
            if (manufcaturer == null)
                return NotFound();

            var manufacturerIntegrationList = await _manufacturerIntegrationModelFactory.PrepareManufacturerIntegrationListModelAsync(searchModel, manufcaturer.Id);

            return Json(manufacturerIntegrationList);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ManufacturerIntegrationAdd(ManufacturerIntegrationModel model)
        {
            if (ModelState.IsValid)
            {
                var manufacturerIntegration = _manufacturerIntegrationModelFactory.PrepareManufacturerIntegrationFields(model);

                await _manufacturerIntegrationService.InsertManufacturerIntegrationAsync(manufacturerIntegration);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ManufacturerIntegrationDelete(int id)
        {
            var manufacturerIntegration = await _manufacturerIntegrationService.GetManufacturerIntegrationAsync(id);
            if (manufacturerIntegration == null)
                return NotFound();

            await _manufacturerIntegrationService.DeleteManufacturerIntegrationAsync(manufacturerIntegration);

            return new NullJsonResult();
        }

        #endregion
    }
}
