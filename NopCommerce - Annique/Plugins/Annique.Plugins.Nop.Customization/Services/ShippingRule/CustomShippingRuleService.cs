using Annique.Plugins.Nop.Customization.Domain.ShippingRule;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ShippingRule
{
    public class CustomShippingRuleService : ICustomShippingRuleService
    {

        #region Fields

        private readonly IRepository<CustomShippingByWeightByTotalRecord> _sbwtRepository;
        private readonly IAclService _aclService;
        private readonly ISettingService _settingService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public CustomShippingRuleService(IRepository<CustomShippingByWeightByTotalRecord> sbwtRepository,
            IAclService aclService,
            ISettingService settingService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IStoreContext storeContext,
            IStaticCacheManager staticCacheManager)
        {
            _sbwtRepository = sbwtRepository;
            _aclService = aclService;
            _settingService = settingService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        /// <summary>
        ///Return Custom shipping rule is enable or disable
        /// </summary>
        public async Task<bool> IsCustomShippingRuleEnableAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            bool isCustomShippingRule = false;

            // Use a cache key that represents the result of this operation.
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AnniqueCustomizationDefaults.CustomShippingRuleEnableCacheKey,store.Id);

            // Try to get the result from the cache
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();
                if (pluginEnable && settings.IsCustomShippingRule)
                    isCustomShippingRule = true;

                return isCustomShippingRule;
            });
        }

        /// <summary>
        /// Get all shipping by weight records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the custom shipping by weight record
        /// </returns>
        public virtual async Task<IPagedList<CustomShippingByWeightByTotalRecord>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _sbwtRepository.GetAllAsync(query =>
            {
                return from sbw in query
                       orderby sbw.StoreId, sbw.CountryId, sbw.StateProvinceId, sbw.Zip, sbw.ShippingMethodId,
                           sbw.WeightFrom, sbw.OrderSubtotalFrom
                       select sbw;
            }, cache => cache.PrepareKeyForDefaultCache(AnniqueCustomizationDefaults.CustomShippingByWeightByTotalAllKey));

            var records = new PagedList<CustomShippingByWeightByTotalRecord>(rez, pageIndex, pageSize);

            return records;
        }

        /// <summary>
        /// Get a shipping by weight record by passed parameters
        /// </summary>
        /// <param name="shippingMethodId">Shipping method identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="stateProvinceId">State identifier</param>
        /// <param name="zip">Zip postal code</param>
        /// <param name="weight">Weight</param>
        /// <param name="orderSubtotal">Order subtotal</param>
        /// <param name="customerRoleIds">Customer role ids</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipping by weight record
        /// </returns>
        public virtual async Task<CustomShippingByWeightByTotalRecord> FindRecordsAsync(int shippingMethodId, int storeId, int warehouseId,
            int countryId, int stateProvinceId, string zip, decimal weight, decimal orderSubtotal, int[] customerRoleIds)
        {
            var foundRecords = await FindRecordsAsync(shippingMethodId, storeId, warehouseId, countryId, stateProvinceId, zip, weight, orderSubtotal, 0, int.MaxValue, customerRoleIds);

            return foundRecords.FirstOrDefault();
        }

        /// <summary>
        /// Filter Shipping Weight Records
        /// </summary>
        /// <param name="shippingMethodId">Shipping method identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="stateProvinceId">State identifier</param>
        /// <param name="zip">Zip postal code</param>
        /// <param name="weight">Weight</param>
        /// <param name="orderSubtotal">Order subtotal</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="customerRoleIds">Customer role ids</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the shipping by weight record
        /// </returns>
        public virtual async Task<IPagedList<CustomShippingByWeightByTotalRecord>> FindRecordsAsync(int shippingMethodId, int storeId, int warehouseId,
            int countryId, int stateProvinceId, string zip, decimal? weight, decimal? orderSubtotal, int pageIndex, int pageSize, int[] customerRoleIds)
        {
            zip = zip?.Trim() ?? string.Empty;

            var existingRates = (await GetAllAsync())
                .Where(sbw => sbw.ShippingMethodId == shippingMethodId && (!weight.HasValue || weight >= sbw.WeightFrom && weight <= sbw.WeightTo))
                .AsQueryable();

            //Apply ACL filtering based on customer roles
            var customerRecords = await _aclService.ApplyAcl(existingRates, customerRoleIds);

            //filter by order subtotal
            var matchedBySubtotal = !orderSubtotal.HasValue ? customerRecords :
                customerRecords.Where(sbw => orderSubtotal >= sbw.OrderSubtotalFrom && orderSubtotal <= sbw.OrderSubtotalTo);

            //filter by store
            var matchedByStore = storeId == 0
                ? matchedBySubtotal
                : matchedBySubtotal.Where(r => r.StoreId == storeId || r.StoreId == 0);

            //filter by warehouse
            var matchedByWarehouse = warehouseId == 0
                ? matchedByStore
                : matchedByStore.Where(r => r.WarehouseId == warehouseId || r.WarehouseId == 0);

            //filter by country
            var matchedByCountry = countryId == 0
                ? matchedByWarehouse
                : matchedByWarehouse.Where(r => r.CountryId == countryId || r.CountryId == 0);

            //filter by state/province
            var matchedByStateProvince = stateProvinceId == 0
                ? matchedByCountry
                : matchedByCountry.Where(r => r.StateProvinceId == stateProvinceId || r.StateProvinceId == 0);

            //filter by zip
            var matchedByZip = string.IsNullOrEmpty(zip)
                ? matchedByStateProvince
                : matchedByStateProvince.Where(r => string.IsNullOrEmpty(r.Zip) || r.Zip.Equals(zip, StringComparison.InvariantCultureIgnoreCase));

            //sort from particular to general, more particular cases will be the first
            var foundRecords = matchedByZip.OrderBy(r => r.StoreId == 0)
                                .ThenBy(r => r.WarehouseId == 0)
                                .ThenBy(r => r.CountryId == 0)
                                .ThenBy(r => r.StateProvinceId == 0)
                                .ThenBy(r => string.IsNullOrEmpty(r.Zip));

            //var records = new PagedList<CustomShippingByWeightByTotalRecord>(foundRecords.ToList(), pageIndex, pageSize);
            var records = new PagedList<CustomShippingByWeightByTotalRecord>(foundRecords.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(), pageIndex, pageSize);

            return records;
        }

        /// <summary>
        /// Get a shipping by weight record by identifier
        /// </summary>
        /// <param name="customShippingByWeightRecordId">Record identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the custom shipping by weight record
        /// </returns>
        public async Task<CustomShippingByWeightByTotalRecord> GetByIdAsync(int customShippingByWeightRecordId)
        {
            return await _sbwtRepository.GetByIdAsync(customShippingByWeightRecordId);
        }

        /// <summary>
        /// Insert the Custom shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertCustomShippingByWeightRecordAsync(CustomShippingByWeightByTotalRecord customShippingByWeightRecord)
        {
            await _sbwtRepository.InsertAsync(customShippingByWeightRecord);
        }

        /// <summary>
        /// Update the Custom shipping by weight record
        /// </summary>
        /// <param name="customShippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateCustomShippingByWeightRecordAsync(CustomShippingByWeightByTotalRecord customShippingByWeightRecord)
        {
            await _sbwtRepository.UpdateAsync(customShippingByWeightRecord);
        }

        /// <summary>
        /// Delete the Custom shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteCustomShippingByWeightRecordAsync(CustomShippingByWeightByTotalRecord customShippingByWeightRecord)
        {
            await _sbwtRepository.DeleteAsync(customShippingByWeightRecord);
        }
    }
}
