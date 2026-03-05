using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;
using XcellenceIT.Plugin.ProductRibbons.Models;
using XcellenceIT.Plugin.ProductRibbons.Services;
using XcellenceIT.Plugin.ProductRibbons.Utilities;

namespace XcellenceIT.Plugin.ProductRibbons.Factories
{
    public class ProductRibbonPublicFactory : IProductRibbonPublicFactory
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IProductPriceService _productPriceService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly IProductRibbonsService _productRibbonsService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public ProductRibbonPublicFactory(ILocalizationService localizationService,
            IProductPriceService productPriceService,
            IPriceFormatter priceFormatter,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            IProductRibbonsService productRibbonsService,
            ISettingService settingService,
            IStoreContext storeContext,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager)
        {
            _localizationService = localizationService;
            _productPriceService = productPriceService;
            _priceFormatter = priceFormatter;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _productRibbonsService = productRibbonsService;
            _settingService = settingService;
            _storeContext = storeContext;
            _specificationAttributeService = specificationAttributeService;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utility

        //get product status attribute value
        public async Task<string> GetProductStockStatus(ProductSpecificationAttribute psa)
        {
            return psa.AttributeType switch
            {
                SpecificationAttributeType.Option => WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId), x => x.Name)),
                SpecificationAttributeType.CustomText => WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue)),
                SpecificationAttributeType.CustomHtmlText => await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue),
                SpecificationAttributeType.Hyperlink => $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>",
                _ => null
            };
        }

        // Used for Token replace of default nopCommerce 
        public async Task<string> GetRibbonTextDataString(Product product, string ribbonTextTemplateString)
        {
            var ribbonText = string.Empty;
            if (ribbonTextTemplateString != null)
            {
                List<ProductRibbonTokens> list = new();
                if (ribbonTextTemplateString.Contains(ProductRibbonTokens.TokenMaxDiscountPercentage))
                {
                    var maxDiscountPercentage = await _productPriceService.GetMaxDiscountPercentageAsync(product);
                    if(maxDiscountPercentage != 0)
                        ribbonText = ribbonTextTemplateString.Replace(ProductRibbonTokens.TokenMaxDiscountPercentage, maxDiscountPercentage + "%".ToString());
                    else
                        ribbonText = string.Empty;
                }
                else if (ribbonTextTemplateString.Contains(ProductRibbonTokens.TokenMaxDiscountValue))
                {
                    var maxDiscountValue = await _productPriceService.GetMaxDiscountValueAsync(product);
                    if (maxDiscountValue != decimal.Zero)
                    {
                        var DiscountValueFormatter = await _priceFormatter.FormatPriceAsync(maxDiscountValue, true, false);

                        ribbonText = ribbonTextTemplateString.Replace(ProductRibbonTokens.TokenMaxDiscountValue, DiscountValueFormatter);
                    }
                    else
                        ribbonText = string.Empty;
                }
                else if (ribbonTextTemplateString.Contains(ProductRibbonTokens.TokenOldPriceNewPriceDiffPercentage))
                {
                    var OldPriceNewPriceDiffPercentage = await _productPriceService.GetOldPriceNewPriceDifferencePercentageAsync(product);
                    if (OldPriceNewPriceDiffPercentage != 0)
                        ribbonText = ribbonTextTemplateString.Replace(ProductRibbonTokens.TokenOldPriceNewPriceDiffPercentage, OldPriceNewPriceDiffPercentage + "%".ToString());
                    else
                        ribbonText = string.Empty;
                }
                else if (ribbonTextTemplateString.Contains(ProductRibbonTokens.TokenOldPriceNewPriceDiffValue))
                {
                    decimal oldPriceNewPriceDifferenceValue = await _productPriceService.GetOldPriceNewPriceDifferenceValueAsync(product);
                    if (oldPriceNewPriceDifferenceValue != 0)
                    {
                        var oldPriceNewValueFormatter = await _priceFormatter.FormatPriceAsync(oldPriceNewPriceDifferenceValue, true, false);
                        ribbonText = ribbonTextTemplateString.Replace(ProductRibbonTokens.TokenOldPriceNewPriceDiffValue, oldPriceNewValueFormatter);
                    }
                    else
                        ribbonText = string.Empty;
                }

                else if (ribbonTextTemplateString.Contains(ProductRibbonTokens.TokenProductQuantity))
                {
                    var ProductQuantity = await _productPriceService.GetProductQuantityAsync(product);
                    if (ProductQuantity > 0)
                        ribbonText = ribbonTextTemplateString.Replace(ProductRibbonTokens.TokenProductQuantity, ProductQuantity.ToString());
                    else
                        ribbonText = string.Empty;
                }
                else if (ribbonTextTemplateString.Contains(ProductRibbonTokens.TokenProductOutOfStock))
                {
                    var ProductQuantity = await _productPriceService.GetProductOutOfStockAsync(product);
                    ribbonText = ribbonTextTemplateString.Replace(ProductRibbonTokens.TokenProductOutOfStock, ProductQuantity);
                }
                else if (ribbonTextTemplateString.Contains(ProductRibbonTokens.TokenProductEndAvailableDate))
                {
                    if (product.Price < product.OldPrice)
                    {
                        DateTime endDate = (DateTime)product.AvailableEndDateTimeUtc;
                        ribbonText = ribbonTextTemplateString.Replace(ProductRibbonTokens.TokenProductEndAvailableDate, endDate.ToString("yyyy/MM/dd"));
                    }
                    else
                        ribbonText = string.Empty;
                }
                else if (ribbonTextTemplateString.Contains(ProductRibbonTokens.TokenProductSpecialEndPrice))
                {
                    if (product.Price < product.OldPrice)
                    {
                        //formate product old price with currency
                        var OldPrice = await _priceFormatter.FormatPriceAsync(product.OldPrice, true, false);

                        //strike through product old price
                        var strikeThroughOldPrice = $"<span class=\"strikeThrough\">{OldPrice}</span>";

                        //formate product price with currency
                        var productPrice = await _priceFormatter.FormatPriceAsync(product.Price, true, false);

                        //take product end available date
                        DateTime endDate = (DateTime)product.AvailableEndDateTimeUtc;

                        var resourceStringBeforePrice = await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.SpecialEnd.SpecialPrice");

                        //replace token with product end available date with product old price and product price
                        ribbonText = ribbonTextTemplateString.Replace(ProductRibbonTokens.TokenProductSpecialEndPrice, endDate.ToString("yyyy/MM/dd") + "<br/>"
                        + resourceStringBeforePrice  + " "+ strikeThroughOldPrice  + " " + productPrice );
                    }
                    else
                        ribbonText = string.Empty;
                }
                else
                    ribbonText = ribbonTextTemplateString;
            }

            return ribbonText;
        }

        //Used to get product stock status image id,url
        public async Task<(int pictureId,string pictureUrl)> GetStockStatusImageId(Product product)
        {
            var storeScope = await _storeContext.GetCurrentStoreAsync();

            // Get product ribbon setting from store wise 
            var productRibbonSetting = await _settingService.LoadSettingAsync<ProductRibbonsSettings>(storeScope.Id);

            if (productRibbonSetting.SpecificationAttributeId != 0)
            {
                //Get product specification attributes
                var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id);
                foreach (var psa in productSpecificationAttributes)
                {
                    //Get selcted option of product specification attribute
                    var option = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);

                    //Check for only stock status option from all product specification options
                    if (option.SpecificationAttributeId == productRibbonSetting.SpecificationAttributeId)
                    {
                        var productStockStatus = await GetProductStockStatus(psa);

                        //Check Stock status and get appropriate stock status image id from settings
                        if (productStockStatus.Equals("R"))
                            return (productRibbonSetting.StockStatusRImage,productRibbonSetting.StockStatusRImageUrl);
                        else if (productStockStatus.Equals("G"))
                            return (productRibbonSetting.StockStatusGImage,productRibbonSetting.StockStatusGImageUrl);
                        else if (productStockStatus.Equals("B"))
                            return (productRibbonSetting.StockStatusBImage,productRibbonSetting.StockStatusBImageUrl);
                        else if (productStockStatus.Equals("O"))
                            return (productRibbonSetting.StockStatusOImage,productRibbonSetting.StockStatusOImageUrl);
                        else
                            return (0, string.Empty);
                    }
                }
            }
            return (0,string.Empty);
        }


        #endregion

        #region Method

        // Prepare public ribbon Model
        public async Task<PublicRibbonModel> PreparedPublicRibbonModel(ProductPictureRibbon productPictureRibbon, Product product)
        {
            PublicRibbonModel publicRibbonModel = new();

            var store = await _storeContext.GetCurrentStoreAsync();

            // Convert into Language wise 
            var RibbonText = await _localizationService.GetLocalizedAsync(productPictureRibbon, x => x.RibbonText);

            //If ribbon text contains Product stock status then get product stock status image id
            if (!string.IsNullOrEmpty(RibbonText) && RibbonText.Contains(ProductRibbonTokens.TokenProductStockStatus))
            {
                (publicRibbonModel.PictureId, publicRibbonModel.PictureUrl) = await GetStockStatusImageId(product);
            }
            else
            {
                publicRibbonModel.RibbonText = await GetRibbonTextDataString(product, RibbonText);
                if (productPictureRibbon.PictureId > 0)
                {
                    #region Implement cashing for picture to reduce database calls for same picture url

                    var key = _productRibbonsService.PrepareKeyForCustomCache(ProductRibbonDefaults.ProductRibbonPictureByIdCacheKey, productPictureRibbon.PictureId, store.Id);

                    publicRibbonModel.PictureUrl = await _staticCacheManager.GetAsync(key, async () => await _pictureService.GetPictureUrlAsync(productPictureRibbon.PictureId));

                    #endregion 

                    publicRibbonModel.PictureId = productPictureRibbon.PictureId;
                }
            }

            publicRibbonModel.RibbonId = productPictureRibbon.RibbonId;
            publicRibbonModel.Enabled = productPictureRibbon.Enabled;
            publicRibbonModel.ContainerStyleCss = productPictureRibbon.ContainerStyleCss;
            publicRibbonModel.ImageStyleCss = productPictureRibbon.ImageStyleCss;
            publicRibbonModel.TextStyleCss = productPictureRibbon.TextStyleCss;
            publicRibbonModel.Position = ((DisplayAvailablePositionsEnum)productPictureRibbon.Position).ToString();
            publicRibbonModel.ProductId = product.Id;
            publicRibbonModel.ProductSeName = await _urlRecordService.GetSeNameAsync(product);

            return publicRibbonModel;
        }

        public async Task<ProductRibbonsPublicModel> PreparedRibbon(Product product, IList<ProductRibbonRecord> ribbonList, Dictionary<int, ProductPictureRibbon> productPictures)
        {
            ProductRibbonsPublicModel productRibbonList = new();

            PublicRibbonModel publicRibbonModel = new();

            foreach (var ribbon in ribbonList)
            {
                ProductPictureRibbon productPicture = productPictures.TryGetValue(ribbon.Id, out ProductPictureRibbon productPictureRibbon) ? productPictureRibbon : null;

                if (productPicture!= null && productPicture.Id != 0)
                {
                    if (ribbon.ApplyToAllProduct)
                        publicRibbonModel = await PreparedPublicRibbonModel(productPicture, product);
                    else if (ribbon.MarkAsNew)
                    {
                        if (product.MarkAsNew && (product.MarkAsNewStartDateTimeUtc <= DateTime.UtcNow || product.MarkAsNewStartDateTimeUtc == null) && (product.MarkAsNewEndDateTimeUtc >= DateTime.UtcNow || product.MarkAsNewEndDateTimeUtc == null))
                            publicRibbonModel = await PreparedPublicRibbonModel(productPicture, product);
                    }
                    else
                    {
                        productPicture = await _productRibbonsService.GetProductPictureRibbonProductIdAsync(ribbon.Id, product.Id);
                        if (productPicture != null && productPicture.Enabled)
                            publicRibbonModel = await PreparedPublicRibbonModel(productPicture, product);
                    }
                   
                    productRibbonList.publicRibbonModel.Add(publicRibbonModel);
                }

                // if IsMoreRibbonDisplayAfterThis enabled then it will break the loop
                if (ribbon.IsMoreRibbonDisplayAfterThis)
                    break;
            }
            return productRibbonList;
        }

        #endregion
    }
}
