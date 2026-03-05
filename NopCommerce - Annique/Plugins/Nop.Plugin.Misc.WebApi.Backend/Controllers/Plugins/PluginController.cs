using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Plugins;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Plugins
{
    public partial class PluginController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPluginService _pluginService;

        #endregion

        #region Ctor

        public PluginController(
            ICustomerService customerService,
            IPluginService pluginService)
        {
            _customerService = customerService;
            _pluginService = pluginService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get plugin descriptors
        /// </summary>
        /// <param name="customerId">Filter by  customer; pass null to load all records</param>
        /// <param name="storeId">Filter by store; pass 0 to load all records</param>
        /// <param name="group">Filter by plugin group; pass null to load all records</param>
        /// <param name="friendlyName">Filter by plugin friendly name; pass null to load all records</param>
        /// <param name="author">Filter by plugin author; pass null to load all records</param>
        /// <param name="dependsOnSystemName">System name of the plugin to define dependencies</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<PluginDescriptorDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPluginDescriptors([FromQuery] int? customerId = null,
            [FromQuery] int storeId = 0,
            [FromQuery] string group = null,
            [FromQuery] string dependsOnSystemName = "",
            [FromQuery] string friendlyName = null,
            [FromQuery] string author = null)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId ?? 0);

            var pluginDescriptors = await _pluginService.GetPluginDescriptorsAsync<IPlugin>(
                LoadPluginsMode.InstalledOnly,
                customer, storeId, group, dependsOnSystemName, friendlyName, author);

            var pluginDescriptorDtos = pluginDescriptors.Select(plugin => plugin.ToDto<PluginDescriptorDto>()).ToList();

            return Ok(pluginDescriptorDtos);
        }

        #endregion
    }
}
