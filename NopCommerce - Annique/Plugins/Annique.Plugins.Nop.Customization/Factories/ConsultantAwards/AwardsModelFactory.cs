using Annique.Plugins.Nop.Customization.Models.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.ConsultantAwards
{
    public class AwardsModelFactory : IAwardsModelFactory
    {
        #region Fields

        private readonly IAwardService _awardService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public AwardsModelFactory(IAwardService awardService,
            IProductService productService,
            IStoreContext storeContext,
            IPictureService pictureService,
            ILocalizationService localizationService,
            MediaSettings mediaSettings,
            IPriceFormatter priceFormatter,
            IWebHelper webHelper,
            IWorkContext workContext,
            IStaticCacheManager staticCacheManager)
        {
            _awardService = awardService;
            _productService = productService;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _priceFormatter = priceFormatter;
            _webHelper = webHelper;
            _workContext = workContext;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Prepare the Picture Model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the picture model for the default picture
        /// </returns>
        protected async Task<PictureModel> PreparePictureModelAsync(Product product, int defaultPictureSize)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

            //prepare picture model
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductOverviewPicturesModelKey,
                product, defaultPictureSize, true, false, await _workContext.GetWorkingLanguageAsync(),
                _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            var cachedPictures = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                async Task<PictureModel> preparePictureModelAsync(Picture picture)
                {
                    //we use the Task.WhenAll method to control that both image thumbs was created in same time.
                    //without this method, sometimes there were situations when one of the pictures was not generated on time
                    //this section of code requires detailed analysis in the future
                    var picResultTasks = await Task.WhenAll(_pictureService.GetPictureUrlAsync(picture, defaultPictureSize), _pictureService.GetPictureUrlAsync(picture));

                    var (imageUrl, _) = picResultTasks[0];

                    return new PictureModel
                    {
                        ImageUrl = imageUrl,
                        //"title" attribute
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                            ? picture.TitleAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"),
                                productName),
                        //"alt" attribute
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                            ? picture.AltAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"),
                                productName)
                    };
                }

                //all pictures
                var pictures = (await _pictureService
                    .GetPicturesByProductIdAsync(product.Id, 1))
                    .DefaultIfEmpty(null);
                var pictureModels = await pictures
                    .SelectAwait(async picture => await preparePictureModelAsync(picture))
                    .FirstOrDefaultAsync();
                return pictureModels;
            });

            return cachedPictures;
        }

        #endregion

        #region Method

        /// <summary>
        /// Prepare the Award list model
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Award list model
        /// </returns>
        public async Task<AwardListModel> PrepareAwardListModelAsync(int customerId)
        {
            var model = new AwardListModel();

            //Get all Awards by customerId
            var consultantAwards = _awardService.GetAwardsByCustomerId(customerId);
            if (!consultantAwards.Any())
                return model;

            foreach (var award in consultantAwards)
            {
                var awardDetailModel = new AwardListModel.AwardDetailsModel
                {
                    Id = award.Id,
                    AwardType = award.AwardType,
                    Description = award.Description,
                    ExpiryDate = $"{award.ExpiryDate:dd/MM/yyyy}",
                    MaxValue = award.MaxValue,
                    RemainingValue = award.MaxValue,
                    ShowSelectedOnly = false
                };
                model.Awards.Add(awardDetailModel);
            }

            var firstAwardId = 0;
            //get first award from available awards
            var firstAward = consultantAwards.FirstOrDefault();
            if (firstAward != null)
                firstAwardId = firstAward.Id;

            var store = await _storeContext.GetCurrentStoreAsync();

            #region Bug 617 Fast Start Award selection include items that are not for sale

            //to resolve this issue added visibleIndividuallyOnly:true parameter to search method to load only products where visibleIndividuallyOnly is true
            //product is not from exclusive , gift and event table

            //get store products
            var products = await _productService.SearchProductsAsync(storeId: store.Id,visibleIndividuallyOnly:true);

            #endregion
            //prepare product details with Quantities with first award
            model.Products = (await PrepareAwardProductListModelsAsync(products, firstAwardId)).ToList();

            return model;
        }

        /// <summary>
        /// Prepare the AwardProductListModel 
        /// </summary>
        /// <param name="products">Collection of products</param>
        /// <param name="selectedAwardId">Selected award id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of Award Product ListModel
        /// </returns>
        public async Task<IEnumerable<AwardListModel.AwardProductListModel>> PrepareAwardProductListModelsAsync(IEnumerable<Product> products, int selectedAwardId)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;

            var customer = await _workContext.GetCurrentCustomerAsync();

            // Retrieve award shopping cart items based on the selectedAwardId
            var awardShoppingCartItems = (await _awardService.GetAwardShoppingCartItemsByAwardIdAsync(selectedAwardId)).Where(item => item.CustomerId == customer.Id);

            // Create a dictionary for fast lookup
            var awardShoppingCartItemDict = awardShoppingCartItems.ToDictionary(item => item.ProductId);

            var models = products.Select(async product =>
            {
                var model = new AwardListModel.AwardProductListModel
                {
                    Id = product.Id,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                };
                model.Price = await _priceFormatter.FormatPriceAsync(product.Price, true, false);
                model.ProductPrice = product.Price;
                model.PictureModel = await PreparePictureModelAsync(product, defaultPictureSize);

                // Update the quantity based on the award shopping cart item
                if (awardShoppingCartItemDict.TryGetValue(model.Id, out var awardShoppingCartItem))
                {
                    model.Quantity = awardShoppingCartItem.Quantity;
                }
                else
                {
                    // Set quantity to 0 if product not found in awardShoppingCartItemDict
                    model.Quantity = 0;
                }

                return model;
            }).Select(task => task.Result).ToList();

            return models;
        }

        /// <summary>
        /// Prepare the AwardProductQuantityModel 
        /// </summary>
        /// <param name="awardId">Award Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Award list model
        /// </returns>
        public async Task<IList<AwardProductQuantityModel>> PrepareAwardProductQuantityModelAsync(int awardId)
        {
            // Fetch the AwardShoppingCartItem items for the specified awardId
            var awardShoppingCartItems = await _awardService.GetAwardShoppingCartItemsByAwardIdAsync(awardId);

            if (awardShoppingCartItems == null || !awardShoppingCartItems.Any())
            {
                // Handle the case where no records are found
                return new List<AwardProductQuantityModel>();
            }

            // Prepare AwardProductQuantityModel with productId and Quantity
            var updatedQuantities = awardShoppingCartItems
                .Select(cartItem => new AwardProductQuantityModel
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity
                })
                .ToList();

            return updatedQuantities;
        }

        #endregion
    }
}
