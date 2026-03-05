using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Domain.SpecialOffers;
using Annique.Plugins.Nop.Customization.Models.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.CheckoutGifts
{
    public class GiftModelFactory : IGiftModelFactory
    {
        #region Fields

        private readonly IGiftService _giftService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ILogger _logger;
        private readonly ISpecialOffersService _specialOffersService;

        #endregion

        #region Ctor

        public GiftModelFactory(IGiftService giftService,
            IProductService productService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            MediaSettings mediaSettings,
            IPriceCalculationService priceCalculationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ITaxService taxService,
            IShoppingCartService shoppingCartService,
            IPriceFormatter priceFormatter,
            IWebHelper webHelper,
            IStaticCacheManager staticCacheManager,
            ILogger logger,
            ISpecialOffersService specialOffersService)
        {
            _giftService = giftService;
            _productService = productService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _priceCalculationService = priceCalculationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _taxService = taxService;
            _shoppingCartService = shoppingCartService;
            _priceFormatter = priceFormatter;
            _webHelper = webHelper;
            _staticCacheManager = staticCacheManager;
            _logger = logger;
            _specialOffersService = specialOffersService;
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

        //calculae product price for special offer popup products
        private decimal CalculateSpecialDiscountedPrice(Product product, Discount discount)
        {
            var _discountService = EngineContext.Current.Resolve<IDiscountService>();
            if (discount.UsePercentage && discount.DiscountPercentage == 100m)
            {
                return decimal.Zero;
            }

            var appliedDiscounts = new List<Discount> { discount };
            var preferredDiscount = _discountService.GetPreferredDiscount(appliedDiscounts, product.Price, out var discountAmount);
            if (preferredDiscount != null)
            {
                return product.Price - discountAmount;
            }
            return product.Price;
        }
        #endregion

        #region Method

        /// <summary>
        /// Prepare the gift model
        /// </summary>
        /// <param name="gifts">Blank Gifts</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Gift model
        /// </returns>
        public async Task<GiftModel> PrepareBlankGiftModelAsync(IList<Gift> gifts, IList<ExclusiveItems> exclusiveItems, IList<(Offers, Discount)> activeOffers, IList<ShoppingCartItem> cart)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;

            var model = new GiftModel();

            // donation gift
            var donationGift = gifts.FirstOrDefault(g => g.cGiftType == AnniqueCustomizationDefaults.GiftTypeDonation);

            if (donationGift != null)
            {
                //Get product by id
                var product = await _productService.GetProductByIdAsync(donationGift.ProductId);

                //Get total qty record from gift taken table
                var takenGiftsQtyTotal = await _giftService.GetGiftTakenQtyTotalAsync(donationGift.Id, customer.Id);

                var availableQty = 0;
                if (takenGiftsQtyTotal != 0)
                    availableQty = donationGift.nQtyLimit - takenGiftsQtyTotal;
                else
                    availableQty = donationGift.nQtyLimit;

                //If customer has not taken any gift or for gift product qty is available then add item into model to show on pop up
                if (takenGiftsQtyTotal == 0 || availableQty > 0)
                {
                    // Process donation gift separately and add it to the model first
                    var donationItemModel = new GiftModel.GiftItemsModel
                    {
                        ProductId = donationGift.ProductId,
                        Name = product.Name,
                        Description = product.FullDescription,
                        IsDonationProduct = true,
                        PictureModel = await PreparePictureModelAsync(product, defaultPictureSize)
                    };

                    // Set donation gift button price
                    model.DonationButtonPrice1 = await _priceFormatter.FormatPriceAsync(20, true, false);
                    model.DonationButtonPrice2 = await _priceFormatter.FormatPriceAsync(40, true, false);
                    model.DonationButtonPrice3 = await _priceFormatter.FormatPriceAsync(100, true, false);

                    var itemInCart = cart.Where(item => item.ProductId == product.Id).FirstOrDefault();
                    if (itemInCart != null)
                        model.DonationProductQtyInCart = itemInCart.Quantity;

                    model.GiftItems.Add(donationItemModel);
                }
            }

            var blankGifts = gifts.Where(g => g.cGiftType != AnniqueCustomizationDefaults.GiftTypeDonation);
            foreach (var gift in blankGifts)
            {
                //Get product by id
                var product = await _productService.GetProductByIdAsync(gift.ProductId);

                //Get total qty record from gift taken table
                var takenGiftsQtyTotal = await _giftService.GetGiftTakenQtyTotalAsync(gift.Id, customer.Id);

                //Calculation for available gift
                var availableQty = 0;
                if (takenGiftsQtyTotal != 0)
                    availableQty = gift.nQtyLimit - takenGiftsQtyTotal;
                else
                    availableQty = gift.nQtyLimit;

                //If customer has not taken any gift or for gift product qty is available then add item into model to show on pop up
                if (takenGiftsQtyTotal == 0 || availableQty > 0)
                {
                    var giftItemModel = new GiftModel.GiftItemsModel
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        GiftQtyLimit = gift.nQtyLimit,
                        PictureModel = await PreparePictureModelAsync(product, defaultPictureSize)
                    };

                    //first, try to find product in existing shopping cart 
                    // var itemInCart = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, ShoppingCartType.ShoppingCart, product);
                    var itemInCart = cart.Where(item => item.ProductId == product.Id).FirstOrDefault();

                    //Set gift product already in cart or not in pop up model
                    if (itemInCart != null && itemInCart.Quantity != availableQty)
                        availableQty = availableQty - itemInCart.Quantity;
                    else if (itemInCart != null && itemInCart.Quantity == availableQty)
                        giftItemModel.IsAlreadyInCart = true;
                    else
                        giftItemModel.IsAlreadyInCart = false;

                    giftItemModel.AvailableQuanitity = availableQty;

                    //Calculation for available qty dropdown
                    for (var i = 0; i <= giftItemModel.AvailableQuanitity; i++)
                    {
                        giftItemModel.AvailableQuantities.Insert(i, new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                    }

                    //Get product price with include tax
                    var (priceWithoutDiscount, finalPrice, discountAmt, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, decimal.Zero, true, 1);
                    var priceWithTax = finalPrice;

                    if (finalPrice != decimal.Zero)
                        (priceWithTax, _) = await _taxService.GetProductPriceAsync(product, finalPrice);

                    giftItemModel.Price = await _priceFormatter.FormatPriceAsync(priceWithTax, true, false);

                    if (discountAmt == decimal.Zero)
                    {
                        var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                        if (oldPriceBase > decimal.Zero)
                        {
                            var oldPrice = await _priceFormatter.FormatPriceAsync(oldPriceBase, true, false);
                            giftItemModel.OldPrice = oldPrice;
                        }
                        else
                            giftItemModel.OldPrice = string.Empty;
                    }
                    else
                        giftItemModel.OldPrice = await _priceFormatter.FormatPriceAsync(priceWithoutDiscount, true, false);

                    model.GiftItems.Add(giftItemModel);
                }
            }

            foreach (var exclusiveItem in exclusiveItems)
            {
                //Get product by id
                var product = await _productService.GetProductByIdAsync(Convert.ToInt32(exclusiveItem.ProductID));

                //Get already purchased quantity
                var takenQtyTotal = (exclusiveItem.nQtyPurchased == null) ? 0 : exclusiveItem.nQtyPurchased;

                //Calculation for available qty
                var availableQty = 0;
                if (takenQtyTotal != 0)
                    availableQty = (int)exclusiveItem.nQtyLimit - (int)takenQtyTotal;
                else
                    availableQty = (int)exclusiveItem.nQtyLimit;

                //If customer has not taken any item or for item product qty is available then add item into model to show on pop up
                if (takenQtyTotal == 0 || availableQty > 0)
                {
                    var exclusiveItemModel = new GiftModel.ExclusiveItemsModel
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Description = product.FullDescription,
                        QtyLimit = (int)exclusiveItem.nQtyLimit,
                        PictureModel = await PreparePictureModelAsync(product, defaultPictureSize)
                    };

                    //first, try to find product in existing shopping cart 
                    // var itemInCart = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, ShoppingCartType.ShoppingCart, product);
                    var itemInCart = cart.Where(item => item.ProductId == product.Id).FirstOrDefault();

                    //Set gift product already in cart or not in pop up model
                    if (itemInCart != null && itemInCart.Quantity != availableQty)
                        availableQty -= itemInCart.Quantity;
                    else if (itemInCart != null && itemInCart.Quantity == availableQty)
                        exclusiveItemModel.IsAlreadyInCart = true;
                    else
                        exclusiveItemModel.IsAlreadyInCart = false;

                    exclusiveItemModel.AvailableQuanitity = availableQty;

                    //Calculation for available qty dropdown
                    for (var i = 0; i <= exclusiveItemModel.AvailableQuanitity; i++)
                    {
                        exclusiveItemModel.AvailableQuantities.Insert(i, new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                    }

                    //Get product price with include tax
                    var (priceWithoutDiscount, finalPrice, discountAmt, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, decimal.Zero, true, 1);
                    var priceWithTax = finalPrice;

                    if (finalPrice != decimal.Zero)
                        (priceWithTax, _) = await _taxService.GetProductPriceAsync(product, finalPrice);

                    exclusiveItemModel.Price = await _priceFormatter.FormatPriceAsync(priceWithTax, true, false);

                    model.ExclusiveItems.Add(exclusiveItemModel);
                }
            }

            model.SpecialOffers = await PrepareSpecialOfferModelAsync(activeOffers, cart);

            foreach (var item in cart)
            {
                //get gift by product id
                var blankGift = await _giftService.GetGiftByProductIdAsync(item.ProductId);
                if (blankGift != null)
                {
                    //If blank type gift product
                    if (string.IsNullOrWhiteSpace(blankGift.cGiftType) && !gifts.Any(g => g.Id == blankGift.Id))
                        //remove Product from cart 
                        await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                                       item.Id, string.Empty, decimal.Zero,
                                       null, null, 0, true);
                }
            }

            return model;
        }

        /// <summary>
        /// Prepare the special offer
        /// </summary>
        /// <param name="cart">cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the special model
        /// </returns>
        public async Task<IList<GiftModel.SpecialOfferModel>> PrepareSpecialOfferModelAsync(IList<(Offers, Discount)> activeOffers, IList<ShoppingCartItem> cart)
        {
            var validSpecialOffers = new List<GiftModel.SpecialOfferModel>();
            try
            {
                if (activeOffers.Any())
                {
                    var store = await _storeContext.GetCurrentStoreAsync();

                    foreach (var (offer, discount) in activeOffers)
                    {
                        // Fetch product IDs from OfferList where type is 'f' and offerId matches
                        var productIdsF = await _specialOffersService.GetProductIdsByOfferTypeAsync(offer.Id, "F");

                        // Validate offer 
                        if (await _specialOffersService.IsOfferValidForCartAsync(offer, productIdsF, cart))
                        {
                            // Calculate allowed selections
                            var allowedSelections = await _specialOffersService.CalculateAllowedSelectionsAsync(offer, productIdsF, cart);
                            if (allowedSelections == 0)
                                continue;

                            // Adjust allowed selections based on G products in the cart
                            allowedSelections = _specialOffersService.AdjustAllowedSelectionsBasedOnCartGProducts(offer, allowedSelections, cart);

                            var specialOfferModel = new GiftModel.SpecialOfferModel
                            {
                                OfferId = offer.Id,
                                DiscountId = offer.DiscountId,
                                DiscountName = discount.Name,
                                AllowedSelections = allowedSelections,
                            };

                            if (offer.PictureId != 0)
                            {
                                //getting background image for special offer div using cache to reduce databasecalls
                                var key = _staticCacheManager.PrepareKeyForDefaultCache(AnniqueCustomizationDefaults.OfferBgImageCacheKey, offer.PictureId, store.Id, offer.Id);
                                specialOfferModel.BackgroundImageUrl = await _staticCacheManager.GetAsync(key, async () => await _pictureService.GetPictureUrlAsync(offer.PictureId));
                            }

                            validSpecialOffers.Add(specialOfferModel);
                        }
                    }
                }

                return validSpecialOffers;
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
                return validSpecialOffers;
            }
        }


        /// <summary>
        /// Prepare the special offer product model
        /// </summary>
        /// <param name="offerId">Offer Id</param>
        /// <param name="DiscountId">Discount Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the special product model
        /// </returns>
        public async Task<SpecialProductListModel> PrepareSpecialProductListModelAsync(int offerId, int discountId)
        {
            var _discountService = EngineContext.Current.Resolve<IDiscountService>();

            var productIds = await _specialOffersService.GetProductIdsByOfferTypeAsync(offerId, "G");
            var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());

            var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;

            var productList = new List<SpecialProductListModel.ProductListItemsModel>();
            var discount = await _discountService.GetDiscountByIdAsync(discountId);

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //Customer current cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            foreach (var product in products)
            {
                // Check if the product with the same custom attributes is already in the cart
                var existingCartItem = cart.FirstOrDefault(c => c.ProductId == product.Id);
                var quantityToValidate = existingCartItem != null ? existingCartItem.Quantity : 0;

                var productWarnings = await _specialOffersService.HasStandardWarningsAsync(customer, product, store.Id, quantityToValidate);
                if (productWarnings)
                    continue;

                var pictureModel = await PreparePictureModelAsync(product, defaultPictureSize);
                var discountedPrice = CalculateSpecialDiscountedPrice(product, discount);

                productList.Add(new SpecialProductListModel.ProductListItemsModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    PictureModel = pictureModel,
                    OldPrice = await _priceFormatter.FormatPriceAsync(product.Price, true, false),
                    Price = await _priceFormatter.FormatPriceAsync(discountedPrice, true, false)
                });
            }

            return new SpecialProductListModel
            {
                OfferId = offerId,
                ProductList = productList
            };
        }


        #endregion
    }
}
