using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Profiles;
using Nop.Services.Customers;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class ProfileController : BaseNopWebApiFrontendController
    {
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IProfileModelFactory _profileModelFactory;

        public ProfileController(CustomerSettings customerSettings,
            ICustomerService customerService,
            IProfileModelFactory profileModelFactory)
        {
            _customerSettings = customerSettings;
            _customerService = customerService;
            _profileModelFactory = profileModelFactory;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProfileIndexModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Index([FromQuery] int? id, [FromQuery] int? pageNumber)
        {
            if (!_customerSettings.AllowViewingProfiles) 
                return BadRequest();

            var customerId = 0;
            if (id.HasValue) customerId = id.Value;

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null || await _customerService.IsGuestAsync(customer))
                return NotFound($"Customer by id={customerId} not found or not registered.");

            var model = await _profileModelFactory.PrepareProfileIndexModelAsync(customer, pageNumber);

            return Ok(model.ToDto<ProfileIndexModelDto>());
        }
    }
}