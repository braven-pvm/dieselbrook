using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Directory;
using System;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    public class OverriddenAddressService : AddressService
    {
        #region Ctor

        public OverriddenAddressService(AddressSettings addressSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            ICountryService countryService,
            IRepository<Address> addressRepository,
            IStateProvinceService stateProvinceService) : base(addressSettings,
                addressAttributeParser,
                addressAttributeService, 
                countryService,
                addressRepository, 
                stateProvinceService)
        {
        }

        #endregion

        #region Method

        /// <summary>
        /// Find an address
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="phoneNumber">Phone number</param>
        /// <param name="email">Email</param>
        /// <param name="faxNumber">Fax number</param>
        /// <param name="company">Company</param>
        /// <param name="address1">Address 1</param>
        /// <param name="address2">Address 2</param>
        /// <param name="city">City</param>
        /// <param name="county">County</param>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <param name="zipPostalCode">Zip postal code</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="customAttributes">Custom address attributes (XML format)</param>
        /// <returns>Address</returns>
        public override Address FindAddress(List<Address> source, string firstName, string lastName, string phoneNumber, string email,
            string faxNumber, string company, string address1, string address2, string city, string county, int? stateProvinceId,
            string zipPostalCode, int? countryId, string customAttributes)
        {
            return source.Find(a =>
               string.Equals(a.FirstName?.Trim(), firstName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.LastName?.Trim(), lastName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.PhoneNumber?.Trim(), phoneNumber?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.Email?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.FaxNumber?.Trim(), faxNumber?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.Company?.Trim(), company?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.Address1?.Trim(), address1?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.Address2?.Trim(), address2?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.City?.Trim(), city?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.County?.Trim(), county?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               ((a.StateProvinceId == null && (stateProvinceId == null || stateProvinceId == 0)) || a.StateProvinceId == stateProvinceId) &&
               string.Equals(a.ZipPostalCode?.Trim(), zipPostalCode?.Trim(), StringComparison.OrdinalIgnoreCase) &&
               ((a.CountryId == null && countryId == null) || a.CountryId == countryId) &&
               string.Equals(a.CustomAttributes?.Trim(), customAttributes?.Trim(), StringComparison.OrdinalIgnoreCase)
           );
        }

        #endregion
    }
}
