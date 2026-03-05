using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.PickUpCollection;
using Nop.Core.Domain.Common;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.PickUpCollection
{
    public interface IPickUpCollectionService
    {
        /// <summary>
        /// Get Postal Codes 
        /// </summary>
        /// <param name="postalCode">postalCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains postal codes from geo location api
        /// </returns>
        Task<PostalCodesResponseModel> GetPostalCodesAsync(string postalCode);

        /// <summary>
        /// Get pickup store
        /// </summary>
        /// <param name="pickUpStoreId">Pick up store id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result returns filtered store pickup point
        /// </returns>
        Task<FilterStorePickupPoint> GetPickUpStoreByIdAsync(int pickUpStoreId);

        /// <summary>
        /// Update address form data
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="cell">cell number</param>
        /// <returns>
        /// The task update address with form data
        /// </returns>
        void UpdateAddressWithFormData(Address address, string firstName, string lastName, string cell);

        /// <summary>
        /// Get custom attribute for PEP address
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="pickUpStoreId">Pick up store id</param>
        /// <returns>
        /// The task returns custom attributes for PEP address
        /// </returns>
        Task<string> GetCustomAttributesAsync(Address address, int pickUpStoreId);

        /// <summary>
        /// Prepare address fields
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="customAttributes">Custom attributes</param>
        /// <param name="customerEmail">Customer email</param>
        /// <param name="countryName">Country name</param>
        /// <returns>
        /// The task returns address
        Address PrepareAddressFields(Address address, string customAttributes, string customerEmail, string countryName);


        /// <summary>
        /// customer allowed for pick up store
        /// </summary>
        /// <returns>
        /// The task returns true or false based on customer roles
        Task<bool> IsCustomerAllowedForPickupAsync();
    }
}
