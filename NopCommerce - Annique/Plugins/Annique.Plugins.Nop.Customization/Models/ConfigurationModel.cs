using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models
{
    /// <summary>
    /// configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableAddressCustomAttributes = new List<SelectListItem>();
            AvailableCustomerRoles = new List<SelectListItem>();
            AvailableAdminCustomers = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableMessageTemplates = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        //gets sets plugin enable property
        [NopResourceDisplayName("Annique.Plugin.Fields.IsEnablePlugin")]
        public bool IsEnablePlugin { get; set; }

        public bool IsEnablePlugin_OverrideForStore { get; set; }

        //gets sets pickup collection
        [NopResourceDisplayName("Annique.Plugin.Fields.IsPickUpCollection")]
        public bool IsPickUpCollection { get; set; }

        public bool IsPickUpCollection_OverrideForStore { get; set; }

        //gets sets pick up store radius
        [NopResourceDisplayName("Annique.Plugin.Fields.PickUpStoreRadius")]
        public int PickUpStoreRadius { get; set; }

        public bool PickUpStoreRadius_OverrideForStore { get; set; }

        //gets sets pick up store radius
        [NopResourceDisplayName("Annique.Plugin.Fields.SelectedCustomerRoleIdsForPickup")]
        public IList<int> SelectedCustomerRoleIdsForPickup { get; set; }

        public string CustomerRoleIdsForPickup { get; set; }
        public bool CustomerRoleIdsForPickup_OverrideForStore { get; set; }

        //gets sets pick up store radius
        [NopResourceDisplayName("Annique.Plugin.Fields.GeoLocationApiUsername")]
        public string GeoLocationApiUsername { get; set; }

        public bool GeoLocationApiUsername_OverrideForStore { get; set; }

        //gets sets pick up store custom attribute id
        [NopResourceDisplayName("Annique.Plugin.Fields.PickupCustomAttributeId")]
        public int PickupCustomAttributeId { get; set; }
        public bool PickupCustomAttributeId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableAddressCustomAttributes { get; set; }

        //gets sets customer role id
        [NopResourceDisplayName("Annique.Plugin.Fields.CustomerRoleId")]
        public int CustomerRoleId { get; set; }
        public bool CustomerRoleId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        //gets sets Total number of orders  
        [NopResourceDisplayName("Annique.Plugin.Fields.TotalOrderNo")]
        public int TotalOrderNo { get; set; }
        public bool TotalOrderNo_OverrideForStore { get; set; }

        //gets sets Order amount limit
        [NopResourceDisplayName("Annique.Plugin.Fields.OrderAmountLimit")]
        public decimal OrderAmountLimit { get; set; }
        public bool OrderAmountLimit_OverrideForStore { get; set; }

        //gets sets shipping address validation Api
        [NopResourceDisplayName("Annique.Plugin.Fields.ShippingAddressValidation")]
        public string ShippingAddressValidationApi { get; set; }

        public bool ShippingAddressValidationApi_OverrideForStore { get; set; }

        //gets sets exclusive items Category id
        [NopResourceDisplayName("Annique.Plugin.Fields.ExclusiveItemsCategoryId")]
        public int ExclusiveItemsCategoryId { get; set; }
        public bool ExclusiveItemsCategoryId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        //gets sets User Profile Role Id
        [NopResourceDisplayName("Annique.Plugin.Fields.ConsultantRoleId")]
        public int ConsultantRoleId { get; set; }
        public bool ConsultantRoleId_OverrideForStore { get; set; }

        //gets sets Injectable for report
        [NopResourceDisplayName("Annique.Plugin.Fields.ReportScripts")]
        public string ReportScripts { get; set; }
        public bool ReportScripts_OverrideForStore { get; set; }

        //gets sets Common js for report
        [NopResourceDisplayName("Annique.Plugin.Fields.CommonJs")]
        public string ReportCommonJs { get; set; }
        public bool ReportCommonJs_OverrideForStore { get; set; }

        //gets sets login time limit
        [NopResourceDisplayName("Annique.Plugin.Fields.LoginTimeLimit")]
        public int LoginTimeLimit { get; set; }
        public bool LoginTimeLimit_OverrideForStore { get; set; }

        //gets sets Otp enabled/disabled
        [NopResourceDisplayName("Annique.Plugin.Fields.IsOTP")]
        public bool IsOTP { get; set; }
        public bool IsOTP_OverrideForStore { get; set; }

        //gets sets Otp Api Url
        [NopResourceDisplayName("Annique.Plugin.Fields.OTPApiUrl")]
        public string OTPApiUrl { get; set; }
        public bool OTPApiUrl_OverrideForStore { get; set; }

        //gets sets env type
        [NopResourceDisplayName("Annique.Plugin.Fields.IsStageEnvType")]
        public bool IsStageEnvType { get; set; }
        public bool IsStageEnvType_OverrideForStore { get; set; }

        //gets sets admin customer id
        [NopResourceDisplayName("Annique.Plugin.Fields.AdminCustomerId")]
        public int AdminCustomerId { get; set; }

        public bool AdminCustomerId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableAdminCustomers { get; set; }


        [NopResourceDisplayName("Annique.Plugin.Fields.SelectedExcludeCategoryIds")]
        public IList<int> SelectedExcludeCategoryIds { get; set; }
        public string ExcludedCategoryIds { get; set; }

        public bool ExcludedCategoryIds_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.CustomCacheExpireTime")]
        public int CustomCacheExpireTime { get; set; }

        public bool CustomCacheExpireTime_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsEmailVerification")]
        public bool IsEmailVerification { get; set; }

        public bool IsEmailVerification_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.EmailableApi")]
        public string EmailableApi { get; set; }
        public bool EmailableApi_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.EmailableApiKey")]
        public string EmailableApiKey { get; set; }
        public bool EmailableApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsPasswordResetEnabled")]
        public bool IsPasswordResetEnabled { get; set; }
        public bool IsPasswordResetEnabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.PasswordResetApi")]
        public string PasswordResetApi { get; set; }
        public bool PasswordResetApi_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.PasswordResetApiKey")]
        public string PasswordResetApiKey { get; set; }
        public bool PasswordResetApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.PasswordResetMessageTemplateId")]
        public int PasswordResetSmsMessageTemplateId { get; set; }

        public bool PasswordResetSmsMessageTemplateId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableMessageTemplates { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsStagingModeForPasswordReset")]
        public bool IsStagingModeForPasswordReset { get; set; }
        public bool IsStagingModeForPasswordReset_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsDefaultCountryIdEnabled")]
        public bool IsDefaultCountryIdEnabled { get; set; }
        public bool IsDefaultCountryIdEnabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.DefaultCountryId")]
        public int DefaultCountryId { get; set; }
        public bool DefaultCountryId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }


        [NopResourceDisplayName("Annique.Plugin.Fields.IsCustomShippingRule")]
        public bool IsCustomShippingRule { get; set; }
        public bool IsCustomShippingRule_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsFullTextSearchEnabled")]
        public bool IsFullTextSearchEnabled { get; set; }

        public bool IsFullTextSearchEnabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsTripEnable")]
        public bool IsTripEnable { get; set; }
        public bool IsTripEnable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.TripStartDate")]
        public DateTime TripStartDate { get; set; }
        public bool TripStartDate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.TripEndDate")]
        public DateTime TripEndDate { get; set; }
        public bool TripEndDate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.TripMessageTemplate")]
        public string TripMessageTemplate { get; set; }
        public bool TripMessageTemplate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.QualifyingAmount")]
        public decimal QualifyingAmount { get; set; }
        public bool QualifyingAmount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsChatbotEnable")]
        public bool IsChatbotEnable { get; set; }
        public bool IsChatbotEnable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.OpenAIApiKey")]
        public string OpenAIApiKey { get; set; }
        public bool OpenAIApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.PromptTemplate")]
        public string PromptTemplate { get; set; }
        public bool PromptTemplate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.SelectedChatbotAccessRoleIds")]

        public IList<int> SelectedChatbotAccessRoleIds { get; set; }
        public string ChatbotAccessRoles { get; set; }
        public bool ChatbotAccessRoles_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.RegistrationValidationApiEndPoint")]
        public string RegistrationValidationApiEndPoint { get; set; }
        public bool RegistrationValidationApiEndPoint_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.RegistrationValidationApiKey")]
        public string RegistrationValidationApiKey { get; set; }
        public bool RegistrationValidationApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.PostRedirectUrl")]
        public string PostRedirectUrl { get; set; }

        public bool PostRedirectUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsNopConsultantRegistration")]
        public bool IsNopConsultantRegistration { get; set; }
        public bool IsNopConsultantRegistration_OverrideForStore { get; set; }

        [NopResourceDisplayName("Annique.Plugin.Fields.IsAdminAccessUrl")]
        public bool IsAdminAccessUrl { get; set; }
        public bool IsAdminAccessUrl_OverrideForStore { get; set; }
    }
}