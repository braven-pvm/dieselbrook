using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper;
using Nop.Plugin.Misc.WebApi.Frontend.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.BackInStockSubscription;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Blog;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Country;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.News;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Newsletter;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Poll;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.PrivateMessages;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Profiles;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.ReturnRequests;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Seo;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Sitemap;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Topics;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Vendors;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Wishlist;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Boards;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Directory;
using Nop.Web.Models.Media;
using Nop.Web.Models.News;
using Nop.Web.Models.Newsletter;
using Nop.Web.Models.Order;
using Nop.Web.Models.Polls;
using Nop.Web.Models.PrivateMessages;
using Nop.Web.Models.Profile;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Models.Sitemap;
using Nop.Web.Models.Topics;
using Nop.Web.Models.Vendors;

namespace Nop.Plugin.Misc.WebApi.Frontend.Infrastructure.Mapper
{
    /// <summary>
    /// AutoMapper configuration for frontend Web API DTO
    /// </summary>
    public partial class WebApiFrontendMapperConfiguration : BaseMapperConfiguration
    {
        #region Ctor

        public WebApiFrontendMapperConfiguration()
        {
            //create specific maps
            CreateTopicsMaps();
            CreateVendorMaps();
            CreateProfileMaps();
            CreateReturnRequestsMaps();
            CreateOrdersMaps();
            CreatePrivateMessageMaps();
            CreatePollMaps();
            CreateNewsletterMaps();
            CreateCommonMaps();
            CreateBaseModelsMaps();
            CreateNewsMaps();
            CreateCountryMaps();
            CreateBackInStockSubscriptionMaps();
            CreateProductMaps();
            CreateShoppingCartMaps();
            CreateCategoryMaps();
            CreateBoardsMaps();
            CreateWishlistMaps();
            CreateCustomerMaps();
            CreateBlogMaps();
            CreateCheckoutMaps();
            CreateUrlRecordMaps();
        }

        #endregion

        #region Utilites

        /// <summary>
        /// Create topics maps 
        /// </summary>
        protected virtual void CreateTopicsMaps()
        {
            CreateDtoMap<TopicModel, TopicModelDto>();
        }

        /// <summary>
        /// Create vendor maps 
        /// </summary>
        protected virtual void CreateVendorMaps()
        {
            CreateDtoMap<VendorInfoModel, VendorInfoModelDto>();
            CreateDtoMap<VendorAttributeModel, VendorAttributeModelDto>();
            CreateDtoMap<VendorAttributeValueModel, VendorAttributeValueModelDto>();
            CreateDtoMap<ApplyVendorModel, ApplyVendorModelDto>();
        }

        /// <summary>
        /// Create profile maps 
        /// </summary>
        protected virtual void CreateProfileMaps()
        {
            CreateDtoMap<ProfileIndexModel, ProfileIndexModelDto>();
        }

        /// <summary>
        /// Create return request maps 
        /// </summary>
        protected virtual void CreateReturnRequestsMaps()
        {
            CreateDtoMap<SubmitReturnRequestModel, SubmitReturnRequestModelDto>();
            CreateDtoMap<CustomerReturnRequestsModel, CustomerReturnRequestsModelDto>();
        }

        /// <summary>
        /// Create orders maps 
        /// </summary>
        protected virtual void CreateOrdersMaps()
        {
            CreateDtoMap<CustomerReturnRequestsModel.ReturnRequestModel, ReturnRequestModelDto>();
            CreateDtoMap<SubmitReturnRequestModel.ReturnRequestActionModel, ReturnRequestActionModelDto>();
            CreateDtoMap<SubmitReturnRequestModel.ReturnRequestReasonModel, ReturnRequestReasonModelDto>();
            CreateDtoMap<SubmitReturnRequestModel.OrderItemModel, SubmitReturnRequestModelDto.ReturnRequestOrderItemModelDto>();
            CreateDtoMap<ShipmentDetailsModel.ShipmentStatusEventModel, ShipmentStatusEventModelDto>();
            CreateDtoMap<ShipmentDetailsModel.ShipmentItemModel, ShipmentItemModelDto>();
            CreateDtoMap<ShipmentDetailsModel, ShipmentDetailsModelDto>();
            CreateDtoMap<CustomerRewardPointsModel.RewardPointsHistoryModel, RewardPointsHistoryModelDto>();
            CreateDtoMap<CustomerOrderListModel, CustomerOrderListModelDto>();

            //the PagerModel class doesn't have a base constructor.
            //And the CustomerRewardPointsModel class contains the PagerModel field.
            //That's why it's not possible to use Automapper to map CustomerRewardPointsModelDto to CustomerRewardPointsModel
            CreateMap<CustomerRewardPointsModel, CustomerRewardPointsModelDto>();
            
            CreateDtoMap<CustomerOrderListModel.RecurringOrderModel, RecurringOrderModelDto>();
            CreateDtoMap<CustomerOrderListModel.OrderDetailsModel, CustomerOrderListModelDto.CustomerOrderDetailsModelDto>();
            CreateDtoMap<OrderDetailsModel, OrderDetailsModelDto>();

            CreateDtoMap<OrderDetailsModel.OrderItemModel, OrderDetailsModelDto.OrderItemModelDto>();
            CreateDtoMap<OrderDetailsModel.TaxRate, OrderDetailsModelDto.OrderDetailsTaxRateDto>();
            CreateDtoMap<OrderDetailsModel.GiftCard, OrderDetailsModelDto.OrderDetailsGiftCardDto>();
            CreateDtoMap<OrderDetailsModel.OrderNote, OrderDetailsModelDto.OrderNoteDto>();
            CreateDtoMap<OrderDetailsModel.ShipmentBriefModel, OrderDetailsModelDto.ShipmentBriefModelDto>();
        }                                 

        /// <summary>
        /// Create private message maps 
        /// </summary>
        protected virtual void CreatePrivateMessageMaps()
        {
            CreateDtoMap<SendPrivateMessageModel, SendPrivateMessageModelDto>();
            CreateDtoMap<PrivateMessageIndexModel, PrivateMessageIndexModelDto>();
            CreateDtoMap<PrivateMessageModel, PrivateMessageModelDto>();
        }

        /// <summary>
        /// Create poll maps 
        /// </summary>
        protected virtual void CreatePollMaps()
        {
            CreateDtoMap<PollAnswerModel, PollAnswerModelDto>();
            CreateDtoMap<PollModel, PollModelDto>();
        }

        /// <summary>
        /// Create subscription maps 
        /// </summary>
        protected virtual void CreateNewsletterMaps()
        {
            CreateDtoMap<SubscriptionActivationModel, SubscriptionActivationModelDto>();
        }

        /// <summary>
        /// Create common maps 
        /// </summary>
        protected virtual void CreateCommonMaps()
        {
            //the PagerModel class doesn't have a base constructor.
            //That's why it's not possible to use Automapper to map PagerModelDto to it.
            CreateMap<PagerModel, PagerModelDto>();

            CreateDtoMap<IRouteValues, BaseRouteValuesModelDto>();

            CreateDtoMap<ContactUsModel, ContactUsModelDto>();

            CreateDtoMap<ContactVendorModel, ContactVendorModelDto>();

            CreateDtoMap<SitemapPageModel, SitemapPageModelDto>();

            CreateDtoMap<SitemapModel, SitemapModelDto>();

            CreateDtoMap<SitemapModel.SitemapItemModel, SitemapItemModelDto>();

            CreateDtoMap<SelectListItem, SelectListItemDto>();
            CreateDtoMap<SelectListGroup, SelectListGroupDto>();

            CreateDtoMap<AddressAttributeValueModel, AddressAttributeValueModelDto>();

            CreateDtoMap<AddressAttributeModel, AddressAttributeModelDto>();

            CreateDtoMap<AddressModel, AddressModelDto>();

            CreateDtoMap<LanguageSelectorModel, LanguageSelectorModelDto>();
            CreateDtoMap<LanguageModel, LanguageModelDto>();

            CreateDtoMap<CurrencySelectorModel, CurrencySelectorModelDto>();
            CreateDtoMap<CurrencyModel, CurrencyModelDto>();

            CreateDtoMap<TaxTypeSelectorModel, TaxTypeSelectorModelDto>();
        }

        /// <summary>
        /// Create base models maps 
        /// </summary>
        protected virtual void CreateBaseModelsMaps()
        {
            CreateDtoMap<BasePageableModel, BasePageableModelDto>();
        }

        /// <summary>
        /// Create news maps 
        /// </summary>
        protected virtual void CreateNewsMaps()
        {
            CreateDtoMap<NewsPagingFilteringModel, NewsPagingFilteringModelDto>();
            CreateDtoMap<AddNewsCommentModel, AddNewsCommentModelDto>();
            CreateDtoMap<NewsCommentModel, NewsCommentModelDto>();
            CreateDtoMap<NewsItemListModel, NewsItemListModelDto>();
            CreateDtoMap<NewsItemModel, NewsItemModelDto>();
        }

        /// <summary>
        /// Create country maps 
        /// </summary>
        protected virtual void CreateCountryMaps()
        {
            CreateDtoMap<StateProvinceModel, StateProvinceModelDto>();
        }

        /// <summary>
        /// Create back in stock subscription maps
        /// </summary>
        protected virtual void CreateBackInStockSubscriptionMaps()
        {
            CreateDtoMap<BackInStockSubscribeModel, BackInStockSubscribeModelDto>();
            //the PagerModel class doesn't have a base constructor.
            //And the CustomerBackInStockSubscriptionsModel class contains the PagerModel field.
            //That's why it's not possible to use Automapper to map CustomerBackInStockSubscriptionsModelDto to CustomerBackInStockSubscriptionsModel
            CreateMap<CustomerBackInStockSubscriptionsModel, CustomerBackInStockSubscriptionsModelDto>();

            CreateDtoMap<CustomerBackInStockSubscriptionsModel.BackInStockSubscriptionModel, CustomerBackInStockSubscriptionsModelDto.BackInStockSubscriptionModelDto>();
        }

        /// <summary>
        /// Create product maps
        /// </summary>
        protected virtual void CreateProductMaps()
        {
            CreateDtoMap<ProductDetailsModel, ProductDetailsModelDto>(dtoIgnoreRule: map =>
                map.ForMember(p => p.HasUserAgreement, option => option.Ignore())
                .ForMember(p => p.UserAgreementText, option => option.Ignore())
                .ForMember(p => p.SampleDownloadId, option => option.Ignore()));
            CreateDtoMap<PictureModel, PictureModelDto>();
            CreateDtoMap<VideoModel, VideoModelDto>();
            CreateDtoMap<VendorBriefInfoModel, VendorBriefInfoModelDto>();
            CreateDtoMap<ProductDetailsModel.GiftCardModel, ProductDetailsModelDto.GiftCardModelDto>();
            CreateDtoMap<ProductDetailsModel.ProductPriceModel, ProductDetailsModelDto.ProductPriceModelDto>();
            CreateDtoMap<ProductDetailsModel.AddToCartModel, AddToCartModelDto>();
            CreateDtoMap<ProductDetailsModel.ProductBreadcrumbModel, ProductBreadcrumbModelDto>();
            CreateDtoMap<CategorySimpleModel, CategorySimpleModelDto>(dtoIgnoreRule: map =>
                map.ForMember(m => m.PictureModel, option => option.Ignore()));
            CreateDtoMap<ProductTagModel, ProductTagModelDto>();
            CreateDtoMap<ProductDetailsModel.ProductAttributeModel, ProductDetailsModelDto.ProductDetailsAttributeModelDto>();
            CreateDtoMap<ProductDetailsModel.ProductAttributeValueModel, ProductAttributeValueModelDto>();
            CreateDtoMap<ProductSpecificationModel, ProductSpecificationModelDto>();
            CreateDtoMap<ProductSpecificationAttributeGroupModel, ProductSpecificationAttributeGroupModelDto>();
            CreateDtoMap<ProductSpecificationAttributeModel, ProductSpecificationAttributeModelDto>();
            CreateDtoMap<ProductSpecificationAttributeValueModel, ProductSpecificationAttributeValueModelDto>();
            CreateDtoMap<ManufacturerBriefInfoModel, ManufacturerBriefInfoModelDto>();
            CreateDtoMap<ProductReviewOverviewModel, ProductReviewOverviewModelDto>();
            CreateDtoMap<ProductDetailsModel.ProductEstimateShippingModel, ProductEstimateShippingModelDto>();
            CreateDtoMap<ProductDetailsModel.TierPriceModel, TierPriceModelDto>();

            CreateDtoMap<ProductCombinationModel, ProductCombinationModelDto>();
            CreateDtoMap<ProductAttributeModel, ProductAttributeModelDto>();

            CreateDtoMap<ProductOverviewModel, ProductOverviewModelDto>();
            CreateDtoMap<ProductOverviewModel.ProductPriceModel, ProductOverviewModelDto.ProductOverviewProductPriceModelDto>();

            CreateDtoMap<ProductReviewsModel, ProductReviewsModelDto>();
            CreateDtoMap<ProductReviewModel, ProductReviewModelDto>();
            CreateDtoMap<ProductReviewHelpfulnessModel, ProductReviewHelpfulnessModelDto>();
            CreateDtoMap<ProductReviewReviewTypeMappingModel, ProductReviewReviewTypeMappingModelDto>();
            CreateDtoMap<AddProductReviewModel, AddProductReviewModelDto>();
            CreateDtoMap<ReviewTypeModel, ReviewTypeModelDto>();
            CreateDtoMap<AddProductReviewReviewTypeMappingModel, AddProductReviewReviewTypeMappingModelDto>();

            //the PagerModel class doesn't have a base constructor.
            //And the CustomerProductReviewsModel class contains the PagerModel field.
            //That's why it's not possible to use Automapper to map CustomerProductReviewsModelDto to CustomerProductReviewsModel
            CreateMap<CustomerProductReviewsModel, CustomerProductReviewsModelDto>();

            CreateDtoMap<CustomerProductReviewModel, CustomerProductReviewModelDto>();

            CreateDtoMap<ProductEmailAFriendModel, ProductEmailAFriendModelDto>();

            CreateDtoMap<CompareProductsModel, CompareProductsModelDto>();
        }

        /// <summary>
        /// Create category maps
        /// </summary>
        protected virtual void CreateCategoryMaps()
        {
            CreateDtoMap<CatalogProductsCommand, CatalogProductsCommandDto>();

            CreateDtoMap<CategoryModel, CategoryModelDto>();
            CreateDtoMap<CategoryModel.SubCategoryModel, SubCategoryModelDto>();
            CreateDtoMap<CatalogProductsModel, CatalogProductsModelDto>();
            CreateDtoMap<NewProductsModel, NewProductsModelDto>();
            CreateDtoMap<PriceRangeFilterModel, PriceRangeFilterModelDto>();
            CreateDtoMap<SpecificationFilterModel, SpecificationFilterModelDto>();
            CreateDtoMap<PriceRangeModel, PriceRangeModelDto>();
            CreateDtoMap<SpecificationAttributeFilterModel, SpecificationAttributeFilterModelDto>();
            CreateDtoMap<SpecificationAttributeValueFilterModel, SpecificationAttributeValueFilterModelDto>();
            CreateDtoMap<ManufacturerFilterModel, ManufacturerFilterModelDto>();
            CreateDtoMap<ManufacturerModel, ManufacturerModelDto>();
            CreateDtoMap<VendorModel, VendorModelDto>();
            CreateDtoMap<ProductsByTagModel, ProductsByTagModelDto>();
            CreateDtoMap<PopularProductTagsModel, PopularProductTagsModelDto>();
            
            CreateDtoMap<Web.Models.Catalog.SearchModel, SearchModelDto>();
            CreateDtoMap<Web.Models.Catalog.SearchModel.CategoryModel, SearchCategoryModelDto>();
        }

        /// <summary>
        /// Create shopping cart maps
        /// </summary>
        protected virtual void CreateShoppingCartMaps()
        {
            CreateDtoMap<EstimateShippingResultModel, EstimateShippingResultModelDto>();
            CreateDtoMap<EstimateShippingResultModel.ShippingOptionModel, ShippingOptionModelDto>();
            CreateDtoMap<ShoppingCartModel.GiftCardBoxModel, GiftCardBoxModelDto>();
            CreateDtoMap<ShoppingCartModel.DiscountBoxModel, DiscountBoxModelDto>();
            CreateDtoMap<ShoppingCartModel.DiscountBoxModel.DiscountInfoModel, DiscountInfoModelDto>();
            CreateDtoMap<ShoppingCartModel.CheckoutAttributeValueModel, CheckoutAttributeValueModelDto>();
            CreateDtoMap<OrderTotalsModel.TaxRate, TaxRateDto>();
            CreateDtoMap<OrderTotalsModel.GiftCard, GiftCardDto>();
            CreateDtoMap<ShoppingCartModel, ShoppingCartModelDto>(entityIgnoreRule: map =>
                map.ForMember(m => m.ButtonPaymentMethodViewComponents, options => options.Ignore()));
            CreateDtoMap<MiniShoppingCartModel.ShoppingCartItemModel, MiniShoppingCartModelDto.MiniShoppingCartItemModelDto>();
            CreateDtoMap<OrderTotalsModel, OrderTotalsModelDto>();
            CreateDtoMap<MiniShoppingCartModel, MiniShoppingCartModelDto>();
            CreateDtoMap<ShoppingCartModel.ShoppingCartItemModel, ShoppingCartModelDto.ShoppingCartItemModelDto>();
            CreateDtoMap<EstimateShippingModel, EstimateShippingModelDto>();
            CreateDtoMap<ShoppingCartModel.CheckoutAttributeModel, CheckoutAttributeModelDto>();
            CreateDtoMap<ShoppingCartModel.OrderReviewDataModel, OrderReviewDataModelDto>();
        }

        /// <summary>
        /// Create boards maps
        /// </summary>
        protected virtual void CreateBoardsMaps()
        {
            CreateDtoMap<BoardsIndexModel, BoardsIndexModelDto>();
            CreateDtoMap<ForumGroupModel, ForumGroupModelDto>();
            CreateDtoMap<ForumRowModel, ForumRowModelDto>();
            CreateDtoMap<ActiveDiscussionsModel, ActiveDiscussionsModelDto>();
            CreateDtoMap<ForumTopicRowModel, ForumTopicRowModelDto>();
            CreateDtoMap<ForumPageModel, ForumPageModelDto>();
            CreateDtoMap<ForumTopicPageModel, ForumTopicPageModelDto>();
            CreateDtoMap<ForumPostModel, ForumPostModelDto>();
            CreateDtoMap<TopicMoveModel, TopicMoveModelDto>();
            CreateDtoMap<EditForumTopicModel, EditForumTopicModelDto>();

            CreateDtoMap<EditForumPostModel, EditForumPostModelDto>();

            CreateDtoMap<Web.Models.Boards.SearchModel, SearchBoardsModelDto>();

            //the PagerModel class doesn't have a base constructor.
            //And the CustomerForumSubscriptionsModel class contains the PagerModel field.
            //That's why it's not possible to use Automapper to map CustomerForumSubscriptionsModelDto to CustomerForumSubscriptionsModel
            CreateMap<CustomerForumSubscriptionsModel, CustomerForumSubscriptionsModelDto>();

            CreateDtoMap<CustomerForumSubscriptionsModel.ForumSubscriptionModel, ForumSubscriptionModelDto>();
        }

        /// <summary>
        /// Create wishlist maps
        /// </summary>
        protected virtual void CreateWishlistMaps()
        {
            CreateDtoMap<WishlistModel.ShoppingCartItemModel, WishlistModelDto.WishlistShoppingCartItemModelDto>();
            CreateDtoMap<WishlistEmailAFriendModel, WishlistEmailAFriendModelDto>();
            CreateDtoMap<WishlistModel, WishlistModelDto>();
        }   

        /// <summary>
        /// Create customer maps
        /// </summary>
        protected virtual void CreateCustomerMaps()
        {
            CreateDtoMap<PasswordRecoveryModel, PasswordRecoveryModelDto>(dtoIgnoreRule: map =>
            {
                map.ForMember(m => m.Result, options => options.Ignore());
            });

            CreateDtoMap<PasswordRecoveryConfirmModel, PasswordRecoveryConfirmModelDto>();

            CreateDtoMap<RegisterModel, RegisterModelDto>();

            CreateDtoMap<CustomerAttributeModel, CustomerAttributeModelDto>();

            CreateDtoMap<CustomerAttributeValueModel, CustomerAttributeValueModelDto>();

            CreateDtoMap<GdprConsentModel, GdprConsentModelDto>();

            CreateDtoMap<RegisterResultModel, RegisterResultModelDto>();
            CreateDtoMap<AccountActivationModel, AccountActivationModelDto>();
            
            CreateDtoMap<CustomerInfoModel, CustomerInfoModelDto>();

            CreateDtoMap<CustomerInfoModel.AssociatedExternalAuthModel, AssociatedExternalAuthModelDto>();
            
            CreateDtoMap<EmailRevalidationModel, EmailRevalidationModelDto>();

            CreateDtoMap<CustomerAddressListModel, CustomerAddressListModelDto>();
            CreateDtoMap<CustomerAddressEditModel, CustomerAddressEditModelDto>();

            CreateDtoMap<CustomerDownloadableProductsModel, CustomerDownloadableProductsModelDto>();
            CreateDtoMap<CustomerDownloadableProductsModel.DownloadableProductsModel, DownloadableProductsModelDto>();

            CreateDtoMap<UserAgreementModel, UserAgreementModelDto>();
            CreateDtoMap<ChangePasswordModel, ChangePasswordModelDto>();

            CreateDtoMap<CustomerAvatarModel, CustomerAvatarModelDto>();

            CreateDtoMap<GdprToolsModel, GdprToolsModelDto>();

            CreateDtoMap<CheckGiftCardBalanceModel, CheckGiftCardBalanceModelDto>();
        }

        /// <summary>
        /// Create blog maps
        /// </summary>
        protected virtual void CreateBlogMaps()
        {            
            CreateDtoMap<AddBlogCommentModel, AddBlogCommentModelDto>();
            CreateDtoMap<BlogPostListModel, BlogPostListModelDto>();
            CreateDtoMap<BlogPagingFilteringModel, BlogPagingFilteringModelDto>();
            CreateDtoMap<BlogPostModel, BlogPostModelDto>();
            CreateDtoMap<BlogCommentModel, BlogCommentModelDto>();
        }

        /// <summary>
        /// Create checkout maps
        /// </summary>
        protected virtual void CreateCheckoutMaps()
        {
            CreateDtoMap<CheckoutCompletedModel, CheckoutCompletedModelDto>();

            CreateDtoMap<CheckoutBillingAddressModel, CheckoutBillingAddressModelDto>();

            CreateDtoMap<CheckoutShippingAddressModel, CheckoutShippingAddressModelDto>();
            CreateDtoMap<CheckoutPickupPointsModel, CheckoutPickupPointsModelDto>();
            CreateDtoMap<CheckoutPickupPointModel, CheckoutPickupPointModelDto>();

            CreateDtoMap<CheckoutShippingMethodModel, CheckoutShippingMethodModelDto>();
            CreateDtoMap<CheckoutShippingMethodModel.ShippingMethodModel, ShippingMethodModelDto>();
            CreateDtoMap<ShippingOption, ShippingOptionDto>();

            CreateDtoMap<CheckoutPaymentMethodModel, CheckoutPaymentMethodModelDto>();
            CreateDtoMap<CheckoutPaymentMethodModel.PaymentMethodModel, PaymentMethodModelDto>();

            CreateDtoMap<CheckoutPaymentInfoModel, CheckoutPaymentInfoModelDto>(entityIgnoreRule: map =>
                map.ForMember(m => m.PaymentViewComponent, options => options.Ignore()));
            CreateDtoMap<CheckoutConfirmModel, CheckoutConfirmModelDto>(
                entityIgnoreRule: map => map.ForMember(m => m.DisplayCaptcha, options => options.Ignore()),
                dtoIgnoreRule: map =>
                    map.ForMember(m => m.ShoppingCart, options => options.Ignore())
                        .ForMember(m => m.OrderTotals, options => options.Ignore()));
        }

        /// <summary>
        /// Create url record maps
        /// </summary>
        protected virtual void CreateUrlRecordMaps()
        {
            CreateDtoMap<UrlRecord, UrlRecordDto>();
        }

        #endregion

        #region Properties

        public override string ProfileName => nameof(WebApiFrontendMapperConfiguration);

        #endregion
    }
}
