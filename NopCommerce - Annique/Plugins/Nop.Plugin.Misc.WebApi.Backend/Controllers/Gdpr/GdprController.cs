using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Gdpr;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Gdpr
{
    public partial class GdprController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IGdprService _gdprService;

        #endregion

        #region Ctor

        public GdprController(ICustomerService customerService,
            IGdprService gdprService)
        {
            _customerService = customerService;
            _gdprService = gdprService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Permanent delete of customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        [HttpDelete("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> PermanentDeleteCustomer(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            await _gdprService.PermanentDeleteCustomerAsync(customer);

            return Ok();
        }

        #endregion
    }
}
