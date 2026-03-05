using Nop.Core.Configuration;
using System;

namespace Annique.Plugins.Nop.Customization
{
    public class AnniqueCustomizationSettings : ISettings
    {
        public bool IsEnablePlugin { get; set; }

        public bool IsPickUpCollection { get; set; }

        public int PickUpStoreRadius { get; set; }

        public string GeoLocationApiUsername { get; set; }

        public int PickupCustomAttributeId { get; set; }

        public string CustomerRoleIdsForPickup { get; set; }

        public int CustomerRoleId { get; set; }

        public int TotalOrderNo { get; set; }

        public decimal OrderAmountLimit { get; set; }

        public string ShippingAddressValidationApi { get; set; }

        public int ExclusiveItemsCategoryId { get; set; }

        public int ConsultantRoleId { get; set; }

        public string ReportScripts { get; set; }

        public string ReportCommonJs { get; set; }

        public int LoginTimeLimit { get; set; }

        public bool IsOTP { get; set; }
        public string OTPApiUrl { get; set; }

        public bool IsStageEnvType { get; set; }

        public int AdminCustomerId { get; set; }

        public string ExcludedCategoryIds { get; set; }

        public int CustomCacheExpireTime { get; set; }

        public bool IsEmailVerification { get; set; }

        public string EmailableApi { get; set; }

        public string EmailableApiKey { get; set; }

        public bool IsPasswordResetEnabled { get; set; }
        public string PasswordResetApi { get; set; }

        public string PasswordResetApiKey { get; set; }

        public int PasswordResetSmsMessageTemplateId { get; set; }

        public bool IsStagingModeForPasswordReset { get; set; }

        public bool IsDefaultCountryIdEnabled { get; set; }

        public int DefaultCountryId { get; set; }

        public bool IsCustomShippingRule { get; set; }

        public bool IsFullTextSearchEnabled { get; set; }

        public bool IsTripEnable { get; set; }

        public DateTime TripStartDate  { get; set; }

        public DateTime TripEndDate { get; set; }

        public string TripMessageTemplate {  get; set; }

        public decimal QualifyingAmount {  get; set; }

        public bool IsChatbotEnable { get; set; }

        public string OpenAIApiKey { get; set; }

        public string PromptTemplate { get; set; }

        public string ChatbotAccessRoles { get; set; }

        public string RegistrationValidationApiEndPoint { get; set; }

        public string RegistrationValidationApiKey { get; set; }

        public string PostRedirectUrl { get; set; }

        public bool IsNopConsultantRegistration { get; set; }

        public bool IsAdminAccessUrl { get; set; }
    }
}
