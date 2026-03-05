using System.Collections.Generic;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Customers
{
    public partial class CustomerRegistrationResult
    {
        public CustomerRegistrationResult(Nop.Services.Customers.CustomerRegistrationResult result)
        {
            Success = result.Success;
            Errors = new List<string>(result.Errors);
        }

        /// <summary>
        /// Gets a value indicating whether request has been completed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Errors
        /// </summary>
        public IList<string> Errors { get; set; }
    }
}
