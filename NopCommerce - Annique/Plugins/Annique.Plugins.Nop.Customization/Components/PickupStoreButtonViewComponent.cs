using Annique.Plugins.Nop.Customization.Services.PickUpCollection;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays add pep store button on shipping address step
    /// </summary>
    [ViewComponent(Name = "PickupStoreButton")]
    public class PickupStoreButtonViewComponent : ViewComponent
    {
        private readonly IPickUpCollectionService _pickUpCollectionService;

        public PickupStoreButtonViewComponent(IPickUpCollectionService pickUpCollectionService)
        {
            _pickUpCollectionService = pickUpCollectionService;
        }

        #region Method

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Use the service method to check if the customer is allowed
            bool isAllowed = await _pickUpCollectionService.IsCustomerAllowedForPickupAsync();

            // Return view if allowed, else return empty content
            return isAllowed ? View() : Content(string.Empty);
        }

        #endregion
    }
}
