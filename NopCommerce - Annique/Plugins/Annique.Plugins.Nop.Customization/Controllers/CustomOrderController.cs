using Annique.Plugins.Nop.Customization.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class CustomOrderController : BasePublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomOrderProcessingService _customOrderProcessingService;

        #endregion

        #region Ctor

        public CustomOrderController(IOrderService orderService,
            IWorkContext workContext,
            ICustomerService customerService,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomOrderProcessingService customOrderProcessingService)
        {
            _orderService = orderService;
            _workContext = workContext;
            _customerService = customerService;
            _storeContext = storeContext;
            _settingService = settingService;
            _customOrderProcessingService = customOrderProcessingService;
        }

        #endregion

        #region Method

        //My account / Order details page / re-order
        public virtual async Task<IActionResult> ReOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            await _customOrderProcessingService.ReOrderAsync(order);
            
            return RedirectToRoute("ShoppingCart");
        }

        #endregion
    }
}
