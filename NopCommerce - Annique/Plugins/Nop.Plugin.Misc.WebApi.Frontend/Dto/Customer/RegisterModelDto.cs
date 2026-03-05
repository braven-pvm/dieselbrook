using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class RegisterModelDto : ModelDto
    {
        public string Email { get; set; }

        public bool EnteringEmailTwice { get; set; }

        public string ConfirmEmail { get; set; }

        public bool UsernamesEnabled { get; set; }

        public string Username { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public bool GenderEnabled { get; set; }

        public string Gender { get; set; }

        public bool FirstNameEnabled { get; set; }

        public string FirstName { get; set; }

        public bool FirstNameRequired { get; set; }

        public bool LastNameEnabled { get; set; }

        public string LastName { get; set; }

        public bool LastNameRequired { get; set; }

        public bool DateOfBirthEnabled { get; set; }

        public int? DateOfBirthDay { get; set; }

        public int? DateOfBirthMonth { get; set; }

        public int? DateOfBirthYear { get; set; }

        public bool DateOfBirthRequired { get; set; }
        
        public bool CompanyEnabled { get; set; }

        public bool CompanyRequired { get; set; }

        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }

        public bool StreetAddressRequired { get; set; }

        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }

        public bool StreetAddress2Required { get; set; }

        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }

        public bool ZipPostalCodeRequired { get; set; }

        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }

        public bool CityRequired { get; set; }

        public string City { get; set; }

        public bool CountyEnabled { get; set; }

        public bool CountyRequired { get; set; }

        public string County { get; set; }

        public bool CountryEnabled { get; set; }

        public bool CountryRequired { get; set; }

        public int CountryId { get; set; }

        public IList<SelectListItemDto> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }

        public bool StateProvinceRequired { get; set; }

        public int StateProvinceId { get; set; }

        public IList<SelectListItemDto> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }

        public bool PhoneRequired { get; set; }

        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }

        public bool FaxRequired { get; set; }

        public string Fax { get; set; }

        public bool NewsletterEnabled { get; set; }

        public bool Newsletter { get; set; }

        public bool AcceptPrivacyPolicyEnabled { get; set; }

        public bool AcceptPrivacyPolicyPopup { get; set; }

        //time zone
        public string TimeZoneId { get; set; }

        public bool AllowCustomersToSetTimeZone { get; set; }

        public IList<SelectListItemDto> AvailableTimeZones { get; set; }

        //EU VAT
        public string VatNumber { get; set; }

        public bool DisplayVatNumber { get; set; }

        public bool HoneypotEnabled { get; set; }

        public bool DisplayCaptcha { get; set; }

        public IList<CustomerAttributeModelDto> CustomerAttributes { get; set; }

        public IList<GdprConsentModelDto> GdprConsents { get; set; }
    }
}
