using Annique.Plugins.Nop.Customization.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.Discounts;
using Nop.Services.Customers;

namespace Annique.Plugins.Nop.Customization
{
    public static class AnniqueCustomizationDefaults
    {
        /// <summary>
        /// Gets a query parameter name to add affiliate friendly name to URL
        /// </summary>
        public static string AffiliateQueryParameter => "ref";

        /// <summary>
        /// Gets a google api to determine the geo location
        /// </summary>
        public static string GeoLocationUrl => "http://api.geonames.org/postalCodeLookupJSON";

        /// <summary>
        /// Gets a key for RC4 Encryption
        /// </summary>
        public static string RC4EncryptionKey => "4nnique4admin!";

        /// <summary>
        /// Gets a force gift type
        /// </summary>
        public static string GiftTypeForce => "FORCE";

        /// <summary>
        /// Gets a starter gift type
        /// </summary>
        public static string GiftTypeStarter => "STARTER";

        /// <summary>
        /// Gets a Donation gift type
        /// </summary>
        public static string GiftTypeDonation => "DONATION";

        /// <summary>
        /// Gets a key for customer table
        /// </summary>
        public static string CustomerTable => "Customer";

        /// <summary>
        /// Gets a key for UserProfileTable
        /// </summary>
        public static string UserProfileTable => "UserProfileAdditionalInfo";

        /// <summary>
        /// Gets a key for Address table
        /// </summary>
        public static string AddressTable => "Address";

        /// <summary>
        /// Gets a key for Default Billing Address Type
        /// </summary>
        public static string DefaultBillingAddressType => "DefaultBillingAddress";

        /// <summary>
        /// Gets a key for Default Shipping Address Type
        /// </summary>
        public static string DefaultShippingAddressType => "DefaultShippingAddress";

        /// <summary>
        /// Gets a key for Nationality Lookup
        /// </summary>
        public static string NationalityLookup => "NATIONALITY";

        /// <summary>
        /// Gets a key for Title Lookup
        /// </summary>
        public static string TitleLookup => "TITLE";

        /// <summary>
        /// Gets a key for Language Lookup
        /// </summary>
        public static string LanguageLookup => "LANGUAGE";

        /// <summary>
        /// Gets a key for Ethnicity Lookup
        /// </summary>
        public static string EthnicityLookup => "ETHNICITY";

        /// <summary>
        /// Gets a key for Bank Lookup
        /// </summary>
        public static string BankLookup => "BANK";

        /// <summary>
        /// Gets a key for Account Type Lookup
        /// </summary>
        public static string AccountType => "ACCOUNTTYPE";

        /// <summary>
        /// Key for discount customer mapping of a certain discount
        /// </summary>
        /// <remarks>
        /// {0} : discount id
        /// </remarks>
        public static CacheKey DiscountCustomerMappingByDiscountCacheKey => new("annique.discountCustomerMapping.bydiscount.{0}", DiscountMappingsPrefix);

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : discount id
        /// {1} : customer id
        /// </remarks>
        public static CacheKey DiscountCustomerMappingsCacheKey => new("annique.discountCustomerMapping.byDiscountIdCustomerId.{0}-{1}", DiscountMappingsPrefix);

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer id 
        /// </remarks>
        public static CacheKey DiscountCustomerMappingAllCacheKey => new("annique.discountCustomerMapping.all.{0}", NopEntityCacheDefaults<DiscountCategoryMapping>.AllPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string DiscountMappingsPrefix => "annique.discountCustomerMapping";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer identifier
        /// </remarks>
        public static CacheKey CustomerInfoCacheKey => new("Nop.customer.info.{0}", NopCustomerServicesDefaults.CustomerRolesBySystemNamePrefix);

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : list type
        /// {1} : offer Id
        /// </remarks>
        public static CacheKey OfferListAllCacheKey => new("annique.specialOfferList.all.{0}-{1}");

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : Customer id
        /// </remarks>
        public static CacheKey ActiveSpecialOffersAllCacheKey => new("annique.activespecialOffer.all.{0}");

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : picture id
        /// {1} : store id
        /// {2} : offer id
        /// </remarks>
        public static CacheKey OfferBgImageCacheKey => new("annique.specialOfferBgImage.{0}-{1}-{2}");

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store id
        /// </remarks>
        public static CacheKey AnniqueCustomizationPluginEnableCacheKey => new("annique.customization.plugin.enabled.{0}");

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product Id
        /// {1} : customer id
        /// </remarks>
        public static CacheKey GetAllocatedExclusiveItemsCacheKey => new("annique.Allocated.exclusiveitems.{0}-{1}", NopEntityCacheDefaults<ExclusiveItems>.AllPrefix);

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product Id
        /// {1} : customer id
        /// </remarks>
        public static CacheKey IsAllocatedExclusiveItemCacheKey => new("annique.isAllocatedExclusiveitem.{0}-{1}", NopEntityCacheDefaults<ExclusiveItems>.AllPrefix);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        public static CacheKey ExclusiveProductCategoriesByProductCacheKey => new("Nop.exclusiveproductcategory.byproduct.{0}-{1}-{2}-{3}", NopEntityCacheDefaults<ExclusiveItems>.AllPrefix);

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : Report id
        /// </remarks>
        public static CacheKey GetReportByIdCacheKey => new("annique.reportById.{0}");

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static CacheKey GetPublishedReportsAllCacheKey => new("annique.published.reports");

        /// <summary>
        /// Gets a name of the clear temp html files schedule task
        /// </summary>
        public static string ClearTempHtmlFilesTaskName => "Clear Report's temp Html files";

        /// <summary>
        /// Gets a type of the clear temp html file schedule task
        /// </summary>
        public static string ClearTempHtmlFilesTask => "Annique.Plugins.Nop.Customization.Services.AnniqueReport.ClearTempHtmlFilesTask";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer id
        /// </remarks>
        public static CacheKey GetUsernameByCustomerIdCacheKey => new("annique.UserNameById.{0}");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current customer ID
        /// </remarks>
        public static CacheKey IsConsultantRoleCacheKey => new("Annique.IsConsultantRole.{0}");

        /// <summary>
        /// Gets a Role for non affiliated user
        /// </summary>
        public static string ClientRole => "Client";

        public static CacheKey CustomShippingByWeightByTotalAllKey => new("Annique.Plugins.Nop.Customization.shippingbyweightbytotal.all", CUSTOMSHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY);

        public static string CUSTOMSHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY => "Annique.Plugins.Nop.Customization.shippingbyweightbytotal.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store id
        /// </remarks>
        public static CacheKey CustomShippingRuleEnableCacheKey => new("annique.customization.plugin.customShippingRuleEnabled.{0}");

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store id
        /// </remarks>
        public static CacheKey FullTextSearchCacheKey => new("annique.customization.FullTextSearch.enabled.{0}");
        
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer id
        /// </remarks>
        public static CacheKey CustomerHasPendingOrdersCacheKey => new("annique.customization.CustomerHasPendingOrders.{0}");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current customer ID
        /// </remarks>
        public static CacheKey ChatbotCustomerAcesssCacheKey => new("Annique.Chatbot.CustomerAccess.{0}");

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store id
        /// </remarks>
        public static CacheKey NopBasedConsultantRegistrationKey => new("annique.customization.NopBasedConsultantRegistration.enabled.{0}");

        /// <summary>
        /// Gets a Consultant Welcome message template
        /// </summary>
        public static string NewConsultantWelcomeMeessageTemplate => "NewConsultant.WelcomeMessage";

        /// <summary>
        /// Gets a Consultant warning message template
        /// </summary>
        public static string ConsultantWarningMeessageTemplate => "NewConsultant.AlreadyRegisteredWarning";

        /// <summary>
        /// Gets a Custom cookie name for track user activity
        /// </summary>
        public static string TrackingCookieName => "CJ_V1";
    }
}
