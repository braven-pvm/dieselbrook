using Annique.Plugins.Nop.Customization.Models.ShippingAddressValidation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ShippingAddressValidation
{
    public interface IShippingAddressValidationService
    {
        /// <summary>
        /// Get Subrub Combinations
        /// </summary>
        /// <param name="term">term</param>
        /// <param name="stateId">stateId</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains subrub combinations codes from  api
        /// </returns>      
        Task<List<SubrubResponseModel>> GetSubrubCombinationsAsync(string term, int stateId);
    }
}
