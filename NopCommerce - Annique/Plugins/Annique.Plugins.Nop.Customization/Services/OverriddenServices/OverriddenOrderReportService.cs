using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    public class OverriddenOrderReportService : OrderReportService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public OverriddenOrderReportService(CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository, 
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, 
            IStoreMappingService storeMappingService, 
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService) : base(currencySettings, 
                currencyService, 
                dateTimeHelper, 
                priceFormatter, 
                addressRepository, 
                orderRepository, 
                orderItemRepository, 
                orderNoteRepository, 
                productRepository, 
                productCategoryRepository, 
                productManufacturerRepository, 
                productWarehouseInventoryRepository, 
                storeMappingService, 
                workContext)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Gets a list of products (identifiers) purchased by other customers who purchased a specified product
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Records to return</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public override async Task<int[]> GetAlsoPurchasedProductsIdsAsync(int storeId, int productId,
            int recordsToReturn = 5, bool visibleIndividuallyOnly = true, bool showHidden = false)
        {
            if (productId == 0)
                throw new ArgumentException("Product ID is not specified");

            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //if settings null or annique plugin not enable then call base service method
            if (settings == null || !settings.IsEnablePlugin)
                return await base.GetAlsoPurchasedProductsIdsAsync(storeId, productId);

            //this inner query should retrieve all orders that contains a specified product ID
            var query1 = from orderItem in _orderItemRepository.Table
                         where orderItem.ProductId == productId
                         select orderItem.OrderId;

            var query2 = from orderItem in _orderItemRepository.Table
                         join p in _productRepository.Table on orderItem.ProductId equals p.Id
                         join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                         where query1.Contains(orderItem.OrderId) &&
                         p.Id != productId &&
                         (showHidden || p.Published) &&
                         !o.Deleted &&
                         (storeId == 0 || o.StoreId == storeId) &&
                         !p.Deleted &&
                         (!visibleIndividuallyOnly || p.VisibleIndividually)
                         select new { orderItem, p };

            var query3 = from orderItem_p in query2
                         group orderItem_p by orderItem_p.p.Id into g
                         select new
                         {
                             ProductId = g.Key,
                             ProductsPurchased = g.Sum(x => x.orderItem.Quantity)
                         };

            // Parse excluded category IDs from settings
            var excludedCategoryIds = settings.ExcludedCategoryIds?
                                                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(int.Parse)
                                                .ToList();

            if (excludedCategoryIds != null && excludedCategoryIds.Any() && query3.Any())
            {
                // Fetch Relevant Product-Category Mappings
                var productCategoryMappings = await _productCategoryRepository.Table
                    .Where(pc => query3.Any(q => q.ProductId == pc.ProductId))
                    .ToListAsync();

                // Filter product which belongs to Excluded Categories
                var excludedProductIds = productCategoryMappings?
                    .Where(pc => excludedCategoryIds.Contains(pc.CategoryId))
                    .Select(pc => pc.ProductId)
                    .Distinct()
                    .ToList();

                // Remove excluded products from Query3
                query3 = query3.Where(q => !excludedProductIds.Contains(q.ProductId));
            }

            query3 = query3.OrderByDescending(x => x.ProductsPurchased);

            if (recordsToReturn > 0)
                query3 = query3.Take(recordsToReturn);

            var report = await query3.ToListAsync();

            var ids = new List<int>();
            foreach (var reportLine in report)
                ids.Add(reportLine.ProductId);

            return ids.ToArray();
        }

        #endregion
    }
}
