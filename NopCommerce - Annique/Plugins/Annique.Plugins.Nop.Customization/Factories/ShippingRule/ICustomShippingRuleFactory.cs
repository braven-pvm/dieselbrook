using Annique.Plugins.Nop.Customization.Domain.ShippingRule;
using Annique.Plugins.Nop.Customization.Models.ShippingRule;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.ShippingRule
{
    public interface ICustomShippingRuleFactory
    {
        /// <summary>
        /// Prepare shipping search model
        /// </summary>
        /// <param name="searchModel">shipping rule search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipping rule search model
        /// </returns>
        Task<CustomShippingRuleSearchModel> PrepareCustomShippingRuleSearchModelAsync(CustomShippingRuleSearchModel searchModel);

        /// <summary>
        /// Prepare paged Report list model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the custom shipping rule list model
        /// </returns>
        Task<CustomShippingRuleListModel> PrepareCustomShippingRuleListModelAsync(CustomShippingRuleSearchModel searchModel);

        /// <summary>
        /// Prepare Report model
        /// </summary>
        /// <param name="model">custom shippingrule model</param>
        /// <param name="customShippingByWeightByTotalRecord">Custom shipping rule</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the custom shipping rule model
        /// </returns>
        Task<CustomShippingRuleModel> PrepareCustomShippingRuleModelAsync(CustomShippingRuleModel model,
            CustomShippingByWeightByTotalRecord customShippingByWeightByTotalRecord, bool excludeProperties = false);

        CustomShippingByWeightByTotalRecord PrepareCustomShippingRuleFields(CustomShippingRuleModel model);
    }
}
