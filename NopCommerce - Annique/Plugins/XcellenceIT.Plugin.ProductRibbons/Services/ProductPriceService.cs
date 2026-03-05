using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Tax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Models;

namespace XcellenceIT.Plugin.ProductRibbons.Services
{
    public class ProductPriceService : IProductPriceService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public ProductPriceService(IProductService productService,
            ITaxService taxService, ICurrencyService currencyService,
            IWorkContext workContext, IPriceCalculationService priceCalculationService,
            IStoreContext storeContext)
        {
            _productService = productService;
            _taxService = taxService;
            _currencyService = currencyService;
            _workContext = workContext;
            _priceCalculationService = priceCalculationService;
            _storeContext = storeContext;
        }

        #endregion

        #region Utilities

        private async Task<List<PriceModel>> GetPriceWithoutAndWithDiscount(Product product)
        {
            List<PriceModel> priceModelList = new();
            PriceModel priceModel = new();

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            var customer = await _workContext.GetCurrentCustomerAsync();

            // checking the groupProduct
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //get list of associated Products
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, currentStore.Id, 0, false);
                if (associatedProducts.Count > 0)
                {
                    // add the value in price model according to associated Products
                    foreach (var associatedProduct in associatedProducts)
                    {
                        Product current = associatedProduct;

                        var (_, presetOldPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(current, customer, currentStore, decimal.Zero, false, 1);
                        var (oldPrice, _) = await _taxService.GetProductPriceAsync(current, presetOldPrice);

                        var (_, presetNewPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(current, customer, currentStore, decimal.Zero, true, 1);
                        var (newPrice, _) = await _taxService.GetProductPriceAsync(current, presetNewPrice);

                        priceModel.productOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPrice, await _workContext.GetWorkingCurrencyAsync());
                        priceModel.productNewPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(newPrice, await _workContext.GetWorkingCurrencyAsync());
                        priceModelList.Add(priceModel);
                    }
                }
            }
            // checking the SimpleProduct and call for price
            if (product.ProductType == ProductType.SimpleProduct && !product.CustomerEntersPrice && !product.CallForPrice)
            {
                var (_, presetOldPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, currentStore, decimal.Zero, false, 1);
                var (oldPrice, _) = await _taxService.GetProductPriceAsync(product, presetOldPrice);

                var (_, presetNewPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, currentStore, decimal.Zero, true, 1);
                var (newPrice, _) = await _taxService.GetProductPriceAsync(product, presetNewPrice);

                priceModel.productOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPrice, await _workContext.GetWorkingCurrencyAsync());
                priceModel.productNewPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(newPrice, await _workContext.GetWorkingCurrencyAsync());

                // add the value in price model according to products
                priceModelList.Add(priceModel);
            }

            return priceModelList;
        }

        private async Task<List<PriceModel>> GetOldAndNewProductPrices(Product product)
        {
            List<PriceModel> priceModelList = new();
            PriceModel priceModel = new();

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            var customer = await _workContext.GetCurrentCustomerAsync();

            // checking the groupProduct
            if (product.ProductType == ProductType.GroupedProduct)
            {
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, currentStore.Id, 0, false);
                if (associatedProducts.Count() > 0)
                {
                    // add the value in price model according to associated Products
                    foreach (var associatedProduct in associatedProducts)
                    {
                        Product current = associatedProduct;
                        var (productPrice, _) = await _taxService.GetProductPriceAsync(current, current.OldPrice);
                        var (_, presetProductPrice2, _, _) = await _priceCalculationService.GetFinalPriceAsync(current, customer, currentStore, decimal.Zero, true, 1);
                        var (productPrice2, _) = await _taxService.GetProductPriceAsync(product, presetProductPrice2);

                        priceModel.productOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice, await _workContext.GetWorkingCurrencyAsync());
                        priceModel.productNewPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice2, await _workContext.GetWorkingCurrencyAsync());

                        priceModelList.Add(priceModel);
                    }
                }
            }

            // checking the SimpleProduct and call for price
            if (product.ProductType == ProductType.SimpleProduct && !product.CustomerEntersPrice && !product.CallForPrice)
            {
                var (productPrice3, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                var (_, presetProductPrice4, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, currentStore, decimal.Zero, true, 1);
                var (productPrice4, _) = await _taxService.GetProductPriceAsync(product, presetProductPrice4);


                priceModel.productOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice3, await _workContext.GetWorkingCurrencyAsync());
                priceModel.productNewPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice4, await _workContext.GetWorkingCurrencyAsync());

                // add the value in price model according to products
                priceModelList.Add(priceModel);
            }

            return priceModelList;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get Max Discount in Percentage
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Get Max Discount in Percentage
        /// </returns>
        // Get maximum Discount in percentage from product ribbon
        public async Task<decimal> GetMaxDiscountPercentageAsync(Product product)
        {
            decimal discountPercentage = default;
            List<decimal> discountPercentageList = new();

            //Get Discount price list by Product ID
            var discountList = await GetPriceWithoutAndWithDiscount(product);

            foreach (var item in discountList)
            {
                // Calculate the percentage of discount from new price and old price 
                discountPercentage = 100m - item.productNewPrice / item.productOldPrice * 100m;

                // check condition percentage contain the value and add in the list 
                if (discountPercentage > decimal.Zero)
                    discountPercentageList.Add(discountPercentage);
            }

            // Add discount percentage which has maximum discount from list 
            if (discountPercentageList.Count > 0)
                discountPercentage = Enumerable.Max(discountPercentageList);

            // return the decimal in round 
            return decimal.Round(discountPercentage);
        }

        /// <summary>
        /// Get Max Discount Value
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Get Max Discount in Value
        /// </returns>
        // Get maximum DiscountValue from product ribbon
        public async Task<decimal> GetMaxDiscountValueAsync(Product product)
        {
            decimal discountPrice = default;
            List<decimal> discountPriceList = new();

            //Get Discount price list by Product ID
            var discountList = await GetPriceWithoutAndWithDiscount(product);
            foreach (var item in discountList)
            {
                // Calculate the discount price from new price and old price 
                discountPrice = item.productOldPrice - item.productNewPrice;

                // check condition discount price contain the value and add in the list 
                if (discountPrice > decimal.Zero)
                    discountPriceList.Add(discountPrice);
            }

            // Add discount price which has maximum discount from list 
            if (discountPriceList.Count > 0)
                discountPrice = Enumerable.Max(discountPriceList);

            return decimal.Round(discountPrice, 2);
        }

        /// <summary>
        /// Get Product Quantity
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Product Quantity
        /// </returns>
        // Get  product quantity from product ribbon
        public async Task<int> GetProductQuantityAsync(Product product)
        {
            int productQuantity = -2147483648;

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            // checked condition for Group Product
            if (product.ProductType == ProductType.GroupedProduct)
            {
                // associated product by product ID
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, currentStore.Id);
                if (associatedProducts.Count() > 0)
                    //add product quantity according to associated Product
                    foreach (var associatedProduct in associatedProducts)
                        productQuantity += await _productService.GetTotalStockQuantityAsync(associatedProduct, true, 0);
            }
            else if (product.ProductType == ProductType.SimpleProduct)
                productQuantity = await _productService.GetTotalStockQuantityAsync(product, true, 0);

            return productQuantity;
        }

        /// <summary>
        /// Get Product Out Of Stock
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Product Out Of Stock
        /// </returns>
        // Get  product quantity from product ribbon
        public async Task<string> GetProductOutOfStockAsync(Product product)
        {
            int productQuantity = -2147483648;
            string productOutOfStock = string.Empty;

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            // checked condition for Group Product
            if (product.ProductType == ProductType.GroupedProduct)
            {
                // associated product by product ID
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, currentStore.Id);
                if (associatedProducts.Count() > 0)
                    //add product quantity according to associated Product
                    foreach (var associatedProduct in associatedProducts)
                        productQuantity += await _productService.GetTotalStockQuantityAsync(associatedProduct, true, 0);
            }
            else if (product.ProductType == ProductType.SimpleProduct)
                productQuantity = await _productService.GetTotalStockQuantityAsync(product, true, 0);

            if (productQuantity <= 0)
                productOutOfStock = "Out Of Stock";

            return productOutOfStock;
        }

        /// <summary>
        /// Get Old Price and New Price Difference in Percentage
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Old Price and New Price Difference in Percentage
        /// </returns>
        public async Task<decimal> GetOldPriceNewPriceDifferencePercentageAsync(Product product)
        {
            decimal priceDifferenceValue = default;
            var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
            if (oldPriceBase > 0)
            {
                //Get Old Price NewPrice DifferencePercentage list by Product ID
                var differenceValueList = await GetOldAndNewProductPrices(product);

                foreach (var item in differenceValueList)
                {
                    // checking old price contain value because for calculate the percentage
                    if (item.productOldPrice > decimal.Zero || item.productNewPrice > decimal.Zero)
                    {
                        priceDifferenceValue = (item.productNewPrice - item.productOldPrice) * 100m / item.productOldPrice;
                    }
                }
            }
            return decimal.Round(priceDifferenceValue, 2);
        }

        /// <summary>
        /// Get Old Price and New Price Difference in Value
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Old Price and New Price Difference in Value
        /// </returns>
        public async Task<decimal> GetOldPriceNewPriceDifferenceValueAsync(Product product)
        {
            decimal priceDifferenceValue = default;
            var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
            if (oldPriceBase > 0)
            {
                //Get Old Price NewPrice Difference value  list by Product ID
                var differenceValueList = await GetOldAndNewProductPrices(product);

                foreach (var item in differenceValueList)
                {
                    // Calculate the difference Value price from the prices
                    priceDifferenceValue = item.productOldPrice - item.productNewPrice;
                }
            }
            return decimal.Round(priceDifferenceValue, 2);
        }

        #endregion

    }
}
