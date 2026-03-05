using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Affiliates;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Blogs;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Customers;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Directory;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Forums;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Gdpr;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Localization;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Logging;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Media;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Messages;
using Nop.Plugin.Misc.WebApi.Backend.Dto.News;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Plugins;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Polls;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Security;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Seo;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping.Date;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Stores;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Tasks;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Tax;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Topics;
using Nop.Plugin.Misc.WebApi.Backend.Dto.TopicTemplates;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Vendors;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.WebApi.Backend.Infrastructure.Mapper
{
    /// <summary>
    /// AutoMapper configuration for web API Backend Dto
    /// </summary>
    public partial class WebApiBackendMapperConfiguration : BaseMapperConfiguration
    {
        #region Ctor

        public WebApiBackendMapperConfiguration()
        {
            //create specific maps
            CreateAffiliateMaps();
            CreateScheduleTaskMaps();
            CreateShippingMaps();
            CreateStoreMaps();
            CreateTaxCategoryMaps();
            CreateTopicsMaps();
            CreateVendorsMap();
            CreateBlogMaps();
            CreateCatalogMaps();
            CreateUrlRecordMaps();
            CreateSecurityMaps();
            CreatePollMaps();
            CreateDiscountsMaps();
            CreateOrderMaps();
            CreateNewsMaps();
            CreateMessagesMaps();
            CreateMediaMaps();
            CreateLoggingMaps();
            CreateCustomerMaps();
            CreateLocalizationMaps();
            CreateGdprMaps();
            CreateCommonMaps();
            CreateDirectoryMaps();
            CreatePluginMaps();
            CreateForumsMaps();
        }

        #endregion

        #region Utilites
        
        /// <summary>
        /// Create topics maps 
        /// </summary>
        protected virtual void CreateTopicsMaps()
        {
            CreateDtoMap<Topic, TopicDto>();

            CreateDtoMap<TopicTemplate, TopicTemplateDto>();
        }

        /// <summary>
        /// Create vendors maps
        /// </summary>
        protected virtual void CreateVendorsMap()
        {
            CreateDtoMap<Vendor, VendorDto>();
            CreateDtoMap<VendorNote, VendorNoteDto>();

            CreateDtoMap<VendorAttribute, VendorAttributeDto>(map =>
                map.ForMember(x => x.AttributeControlType, options => options.Ignore()));

            CreateDtoMap<VendorAttributeValue, VendorAttributeValueDto>();
        }

        /// <summary>
        /// Create affiliate maps 
        /// </summary>
        protected virtual void CreateAffiliateMaps()
        {
            CreateDtoMap<Affiliate, AffiliateDto>();
        }

        /// <summary>
        /// Create tax category maps 
        /// </summary>
        protected virtual void CreateTaxCategoryMaps()
        {
            CreateDtoMap<TaxCategory, TaxCategoryDto>();
        }

        /// <summary>
        /// Create blog maps 
        /// </summary>
        protected virtual void CreateBlogMaps()
        {
            CreateDtoMap<BlogPost, BlogPostDto>();

            CreateDtoMap<BlogComment, BlogCommentDto>();

            CreateDtoMap<BlogPostTag, BlogPostTagDto>();
        }

        /// <summary>
        /// Create catalog maps 
        /// </summary>
        protected virtual void CreateCatalogMaps()
        {
            CreateDtoMap<BackInStockSubscription, BackInStockSubscriptionDto>();

            CreateDtoMap<Category, CategoryDto>();

            CreateDtoMap<ProductCategory, ProductCategoryDto>();

            CreateDtoMap<CategoryTemplate, CategoryTemplateDto>();

            CreateDtoMap<Manufacturer, ManufacturerDto>();

            CreateDtoMap<ProductManufacturer, ProductManufacturerDto>();

            CreateDtoMap<ManufacturerTemplate, ManufacturerTemplateDto>();

            CreateDtoMap<ProductTag, ProductTagDto>();

            CreateDtoMap<ProductTemplate, ProductTemplateDto>();

            CreateDtoMap<Product, ProductDto>(map => map.ForMember(p => p.ProductType, options => options.Ignore())
                .ForMember(p => p.BackorderMode, options => options.Ignore())
                .ForMember(p => p.DownloadActivationType, options => options.Ignore())
                .ForMember(p => p.GiftCardType, options => options.Ignore())
                .ForMember(p => p.LowStockActivity, options => options.Ignore())
                .ForMember(p => p.ManageInventoryMethod, options => options.Ignore())
                .ForMember(p => p.RecurringCyclePeriod, options => options.Ignore())
                .ForMember(p => p.RentalPricePeriod, options => options.Ignore()));

            CreateDtoMap<ProductAttribute, ProductAttributeDto>();

            CreateDtoMap<ProductAttributeMapping, ProductAttributeMappingDto>(map => map.ForMember(p => p.AttributeControlType, options => options.Ignore()));

            CreateDtoMap<ProductAttributeValue, ProductAttributeValueDto>(map => map.ForMember(p => p.AttributeValueType, options => options.Ignore()));

            CreateDtoMap<PredefinedProductAttributeValue, PredefinedProductAttributeValueDto>();

            CreateDtoMap<ProductAttributeCombination, ProductAttributeCombinationDto>();

            CreateDtoMap<ReviewType, ReviewTypeDto>();

            CreateDtoMap<ProductReviewReviewTypeMapping, ProductReviewReviewTypeMappingDto>();

            CreateDtoMap<SpecificationAttributeGroup, SpecificationAttributeGroupDto>();

            CreateDtoMap<SpecificationAttribute, SpecificationAttributeDto>();

            CreateDtoMap<SpecificationAttributeOption, SpecificationAttributeOptionDto>();

            CreateDtoMap<ProductSpecificationAttribute, ProductSpecificationAttributeDto>(map => map.ForMember(psa => psa.AttributeType, options => options.Ignore()));

            CreateDtoMap<RelatedProduct, RelatedProductDto>();

            CreateDtoMap<CrossSellProduct, CrossSellProductDto>();

            CreateDtoMap<TierPrice, TierPriceDto>();

            CreateDtoMap<ProductPicture, ProductPictureDto>();

            CreateDtoMap<ProductVideo, ProductVideoDto>();

            CreateDtoMap<ProductReview, ProductReviewDto>();

            CreateDtoMap<ProductWarehouseInventory, ProductWarehouseInventoryDto>();

            CreateDtoMap<DiscountProductMapping, DiscountProductMappingDto>();

            CreateDtoMap<DiscountCategoryMapping, DiscountCategoryMappingDto>();

            CreateDtoMap<DiscountManufacturerMapping, DiscountManufacturerMappingDto>();

            CreateDtoMap<ProductProductTagMapping, ProductProductTagMappingDto>();

            CreateDtoMap<StockQuantityHistory, StockQuantityHistoryDto>();
        }

        /// <summary>
        /// Create schedule task maps 
        /// </summary>
        protected virtual void CreateScheduleTaskMaps()
        {
            CreateDtoMap<ScheduleTask, ScheduleTaskDto>();
        }

        /// <summary>
        /// Create store maps 
        /// </summary>
        protected virtual void CreateStoreMaps()
        {
            CreateDtoMap<Store, StoreDto>();

            CreateDtoMap<StoreMapping, StoreMappingDto>();
        }

        /// <summary>
        /// Create shipping maps 
        /// </summary>
        protected virtual void CreateShippingMaps()
        {
            CreateDtoMap<ShippingMethod, ShippingMethodDto>();

            CreateDtoMap<ShippingMethodCountryMapping, ShippingMethodCountryMappingDto>();

            CreateDtoMap<Warehouse, WarehouseDto>();

            CreateDtoMap<Shipment, ShipmentDto>();

            CreateDtoMap<ShipmentItem, ShipmentItemDto>();

            CreateDtoMap<DeliveryDate, DeliveryDateDto>();

            CreateDtoMap<ProductAvailabilityRange, ProductAvailabilityRangeDto>();

            CreateDtoMap<Nop.Services.Shipping.GetShippingOptionResponse, GetShippingOptionResponseDto>();
            CreateDtoMap<ShippingOption, ShippingOptionDto>();
            
            CreateDtoMap<Nop.Services.Shipping.Pickup.GetPickupPointsResponse, GetPickupPointsResponseDto>();

            CreateDtoMap<PickupPoint, PickupPointDto>();
        }

        protected virtual void CreateUrlRecordMaps()
        {
            CreateDtoMap<UrlRecord, UrlRecordDto>();
        }

        protected virtual void CreateSecurityMaps()
        {
            CreateDtoMap<PermissionRecord, PermissionRecordDto>();

            CreateDtoMap<PermissionRecordCustomerRoleMapping, PermissionRecordCustomerRoleMappingDto>();

            CreateDtoMap<AclRecord, AclRecordDto>();
        }

        protected virtual void CreatePollMaps()
        {
            CreateDtoMap<Poll, PollDto>();

            CreateDtoMap<PollAnswer, PollAnswerDto>();

            CreateDtoMap<PollVotingRecord, PollVotingRecordDto>();
        }

        /// <summary>
        /// Create discounts maps 
        /// </summary>
        protected virtual void CreateDiscountsMaps()
        {
            CreateDtoMap<Discount, DiscountDto>(map => map.ForMember(d => d.DiscountType, options => options.Ignore())
                .ForMember(d => d.DiscountLimitation, options => options.Ignore()));
            
            CreateDtoMap<DiscountRequirement, DiscountRequirementDto>(map => map.ForMember(d => d.InteractionType, options => options.Ignore()));

            CreateDtoMap<DiscountUsageHistory, DiscountUsageHistoryDto>();

            CreateDtoMap<Nop.Services.Discounts.DiscountValidationResult, DiscountValidationResult>();
        }

        /// <summary>
        /// Create order maps 
        /// </summary>
        protected virtual void CreateOrderMaps()
        {
            CreateDtoMap<ShoppingCartItem, ShoppingCartItemDto>(map => map.ForMember(sci => sci.ShoppingCartType, options => options.Ignore()));

            CreateDtoMap<RewardPointsHistory, RewardPointsHistoryDto>();

            CreateDtoMap<ReturnRequest, ReturnRequestDto>(map => map.ForMember(rr => rr.ReturnRequestStatus, options => options.Ignore()));

            CreateDtoMap<ReturnRequestReason, ReturnRequestReasonDto>();

            CreateDtoMap<ReturnRequestAction, ReturnRequestActionDto>();

            CreateDtoMap<Order, OrderDto>(map => map.ForMember(o => o.OrderStatus, options => options.Ignore())
                .ForMember(o => o.PaymentStatus, options => options.Ignore())
                .ForMember(o => o.ShippingStatus, options => options.Ignore())
                .ForMember(o => o.CustomerTaxDisplayType, options => options.Ignore()));

            CreateDtoMap<OrderItem, OrderItemDto>();

            CreateDtoMap<OrderNote, OrderNoteDto>();

            CreateDtoMap<RecurringPayment, RecurringPaymentDto>(map => map.ForMember(rp => rp.CyclePeriod, options => options.Ignore()));

            CreateDtoMap<RecurringPaymentHistory, RecurringPaymentHistoryDto>();

            CreateDtoMap<GiftCard, GiftCardDto>(map => map.ForMember(gc => gc.GiftCardType, options => options.Ignore()));
            
            CreateDtoMap<GiftCardUsageHistory, GiftCardUsageHistoryDto>();

            CreateDtoMap<CheckoutAttribute, CheckoutAttributeDto>(map => map.ForMember(attr => attr.AttributeControlType, options => options.Ignore()));

            CreateDtoMap<CheckoutAttributeValue, CheckoutAttributeValueDto>();

            CreateDtoMap<AppliedGiftCard, AppliedGiftCardResponseDto>();

            CreateDtoMap<UpdateOrderParameters, UpdateOrderParametersDto>();

            CreateDtoMap<PlaceOrderResult, PlaceOrderResultDto>();

            #region Reports

            CreateDtoMap<OrderByCountryReportLine, OrderByCountryReportLineResponse>();

            CreateDtoMap<OrderAverageReportLine, OrderAverageReportLineResponse>();

            CreateDtoMap<SalesSummaryReportLine, SalesSummaryReportLineDto>();

            CreateDtoMap<BestsellersReportLine, BestsellersReportLineDto>();

            #endregion

            CreateDtoMap<ProcessPaymentRequest, ProcessPaymentRequestDto>();
        }

        /// <summary>
        /// Create news maps 
        /// </summary>
        protected virtual void CreateNewsMaps()
        {
            CreateDtoMap<NewsItem, NewsItemDto>();

            CreateDtoMap<NewsComment, NewsCommentDto>();
        }

        /// <summary>
        /// Create messages maps
        /// </summary>
        protected virtual void CreateMessagesMaps()
        {
            CreateDtoMap<QueuedEmail, QueuedEmailDto>(map =>
                map.ForMember(qe => qe.Priority, options => options.Ignore()));

            CreateDtoMap<NewsLetterSubscription, NewsLetterSubscriptionDto>();

            CreateDtoMap<MessageTemplate, MessageTemplateDto>(map =>
                map.ForMember(mt => mt.DelayPeriod, options => options.Ignore()));

            CreateDtoMap<EmailAccount, EmailAccountDto>();

            CreateDtoMap<Campaign, CampaignDto>();
        }

        /// <summary>
        /// Create media maps
        /// </summary>
        protected virtual void CreateMediaMaps()
        {
            CreateDtoMap<Picture, PictureDto>();

            CreateDtoMap<PictureBinary, PictureBinaryDto>();

            CreateDtoMap<Download, DownloadDto>();
        }

        /// <summary>
        /// Create logging maps
        /// </summary>
        protected virtual void CreateLoggingMaps()
        {
            CreateDtoMap<ActivityLogType, ActivityLogTypeDto>();

            CreateDtoMap<ActivityLog, ActivityLogDto>();
        }

        /// <summary>
        /// Create localization maps
        /// </summary>
        protected virtual void CreateLocalizationMaps()
        {
            CreateDtoMap<LocaleStringResource, LocaleStringResourceDto>();

            CreateDtoMap<Language, LanguageDto>();

            CreateDtoMap<LocalizedProperty, LocalizedPropertyDto>();
        }

        /// <summary>
        /// Create common maps
        /// </summary>
        protected virtual void CreateCommonMaps()
        {
            CreateDtoMap<AddressAttribute, AddressAttributeDto>(map => map.ForMember(a => a.AttributeControlType, options => options.Ignore()));

            CreateDtoMap<AddressAttributeValue, AddressAttributeValueDto>();

            CreateDtoMap<Address, AddressDto>();

            CreateDtoMap<GenericAttribute, GenericAttributeDto>();

            CreateDtoMap<SearchTermReportLine, SearchTermReportLineDto>();

            CreateDtoMap<SearchTerm, SearchTermDto>();

            CreateDtoMap<CustomerRole, CustomerRoleDto>();

            CreateDtoMap<CustomerCustomerRoleMapping, CustomerCustomerRoleMappingDto>();

            CreateDtoMap<CustomerPassword, CustomerPasswordDto>(map => map.ForMember(cp => cp.PasswordFormat, options => options.Ignore()));
        }

        /// <summary>
        /// Create customer maps
        /// </summary>
        protected virtual void CreateCustomerMaps()
        {
            CreateDtoMap<CustomerAttribute, CustomerAttributeDto>(map => map.ForMember(c => c.AttributeControlType, options => options.Ignore()));

            CreateDtoMap<CustomerAttributeValue, CustomerAttributeValueDto>();

            CreateDtoMap<Customer, CustomerDto>(entityIgnoreRule: map =>
                map.ForMember(m => m.VatNumberStatus, options => options.Ignore())
                    .ForMember(m => m.TaxDisplayType, options => options.Ignore()));

            CreateDtoMap<BestCustomerReportLine, BestCustomerReportLineDto>();
        }

        /// <summary>
        /// Create GDPR maps
        /// </summary>
        protected virtual void CreateGdprMaps()
        {
            CreateDtoMap<GdprConsent, GdprConsentDto>();

            CreateDtoMap<GdprLog, GdprLogDto>(map => map.ForMember(g => g.RequestType, options => options.Ignore()));
        }

        /// <summary>
        /// Create directory maps
        /// </summary>
        protected virtual void CreateDirectoryMaps()
        {
            CreateDtoMap<Country, CountryDto>();

            CreateDtoMap<Currency, CurrencyDto>(map => map.ForMember(c => c.RoundingType, options => options.Ignore()));
            
            CreateDtoMap<ExchangeRate, ExchangeRateDto>();

            CreateDtoMap<MeasureDimension, MeasureDimensionDto>();

            CreateDtoMap<MeasureWeight, MeasureWeightDto>();
            
            CreateDtoMap<StateProvince, StateProvinceDto>();
        }

        /// <summary>
        /// Create plugin maps
        /// </summary>
        protected virtual void CreatePluginMaps()
        {
            CreateDtoMap<PluginDescriptor, PluginDescriptorDto>(map => map.ForMember(r => r.ShowInPluginsList, options => options.Ignore())
                .ForMember(r => r.ReferencedAssembly, options => options.Ignore())
                .ForMember(r => r.OriginalAssemblyFile, options => options.Ignore())
                .ForMember(r => r.PluginType, options => options.Ignore())
                .ForMember(r => r.Installed, options => options.Ignore())
                .ForMember(r => r.PluginFiles, oprions => oprions.Ignore()));
        }

        /// <summary>
        /// Create forums maps
        /// </summary>
        protected virtual void CreateForumsMaps()
        {
            CreateDtoMap<Forum, ForumDto>();

            CreateDtoMap<ForumGroup, ForumGroupDto>();

            CreateDtoMap<ForumTopic, ForumTopicDto>(map => map.ForMember(ft => ft.ForumTopicType, options => options.Ignore()));

            CreateDtoMap<ForumPost, ForumPostDto>();

            CreateDtoMap<PrivateMessage, PrivateMessageDto>();

            CreateDtoMap<ForumSubscription, ForumSubscriptionDto>();

            CreateDtoMap<ForumPostVote, ForumPostVoteDto>();
        }

        #endregion

        #region Properties

        public override string ProfileName => nameof(WebApiBackendMapperConfiguration);

        #endregion
    }
}
