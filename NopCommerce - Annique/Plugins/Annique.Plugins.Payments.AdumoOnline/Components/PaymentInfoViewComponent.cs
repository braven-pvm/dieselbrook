using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Annique.Plugins.Payments.AdumoOnline.Components
{
    /// <summary>
    /// Represents a view component that displays in payment info step
    /// </summary>
    [ViewComponent(Name = "PaymentInfo")]
    public class PaymentInfoViewComponent : NopViewComponent
    {
        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
