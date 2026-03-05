using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays checkout button on cart page
    /// </summary>
    [ViewComponent(Name = "CheckoutButton")]
    public class CheckoutButtonViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public CheckoutButtonViewComponent(IWorkContext workContext, ICustomerService customerService)
        {
            _workContext = workContext;
            _customerService = customerService;
        }

        #endregion

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
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isGuest = await _customerService.IsGuestAsync(customer);

            return View(isGuest); // Pass the result to the view
        }

        #endregion

    }
}