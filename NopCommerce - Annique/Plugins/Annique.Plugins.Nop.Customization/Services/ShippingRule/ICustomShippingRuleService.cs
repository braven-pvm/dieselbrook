using Annique.Plugins.Nop.Customization.Domain.ShippingRule;
using Nop.Core;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ShippingRule
{
    public interface ICustomShippingRuleService
    {
        /// <summary>
        ///Return Custom shipping rule is enable or disable
        /// </summary>
        Task<bool> IsCustomShippingRuleEnableAsync();

        /// <summary>
        /// Get all shipping by weight records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the custom shipping by weight record
        /// </returns>
        Task<IPagedList<CustomShippingByWeightByTotalRecord>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue);

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
        Task<CustomShippingByWeightByTotalRecord> FindRecordsAsync(int shippingMethodId, int storeId, int warehouseId,
            int countryId, int stateProvinceId, string zip, decimal weight, decimal orderSubtotal, int[] customerRoleIds);

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
        Task<IPagedList<CustomShippingByWeightByTotalRecord>> FindRecordsAsync(int shippingMethodId, int storeId, int warehouseId,
            int countryId, int stateProvinceId, string zip, decimal? weight, decimal? orderSubtotal, int pageIndex, int pageSize, int[] customerRoleIds);

        /// <summary>
        /// Get a shipping by weight record by identifier
        /// </summary>
        /// <param name="customShippingByWeightRecordId">Record identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the custom shipping by weight record
        /// </returns>
        Task<CustomShippingByWeightByTotalRecord> GetByIdAsync(int customShippingByWeightRecordId);

        /// <summary>
        /// Insert the Custom shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertCustomShippingByWeightRecordAsync(CustomShippingByWeightByTotalRecord customShippingByWeightRecord);

        /// <summary>
        /// Update the Custom shipping by weight record
        /// </summary>
        /// <param name="customShippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateCustomShippingByWeightRecordAsync(CustomShippingByWeightByTotalRecord customShippingByWeightRecord);

        /// <summary>
        /// Delete the Custom shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteCustomShippingByWeightRecordAsync(CustomShippingByWeightByTotalRecord customShippingByWeightRecord);
    }
}
