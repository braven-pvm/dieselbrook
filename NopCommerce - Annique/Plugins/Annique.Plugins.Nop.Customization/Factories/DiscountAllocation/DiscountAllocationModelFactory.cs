using Annique.Plugins.Nop.Customization.Models.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
using Nop.Core.Domain.Customers;
using Nop.Web.Models.ShoppingCart;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.DiscountAllocation
{
    public class DiscountAllocationModelFactory : IDiscountAllocationModelFactory
    {
        #region Fields

        private readonly IDiscountCustomerMappingService _discountCustomerMappingService;
        #endregion

        #region Ctor 

        public DiscountAllocationModelFactory(IDiscountCustomerMappingService discountCustomerMappingService)
        {
            _discountCustomerMappingService = discountCustomerMappingService;  
        }

        #endregion

        /// <summary>
        /// Prepare the discount Info list model
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartModel">shopping cart Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Discount list model
        /// </returns>
        public async Task<DiscountInfoListModel> PrepareDiscountInfoListModelAsync(Customer customer, ShoppingCartModel shoppingCartModel)
        {
            var model = new DiscountInfoListModel();

            // Get all customer discount mappings
            var discountCustomerMappings = await _discountCustomerMappingService.GetAllDiscountCustomerMappingsAsync(customer.Id);

            if (discountCustomerMappings.Any())
            {
                // Get available and applied discount names
                var (availableDiscountNames, appliedDiscountNames, hasAutoAppliedDiscount) = await _discountCustomerMappingService.GetDiscountNamesAsync(customer, shoppingCartModel, discountCustomerMappings);

                model.AvailableDiscounts = availableDiscountNames?.ToList() ?? new List<AvailableDiscountModel>();
                model.AppliedDiscountNames = appliedDiscountNames?.ToList() ?? new List<string>();
                model.HasAutoAppliedDiscount = hasAutoAppliedDiscount;
            }

            return model;
        }
    }
}
