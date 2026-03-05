using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// CategoryNavigationModel prepare event
    /// </summary>
    public class CategoryNavigationModelPrepareEvent : IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IExclusiveItemsService _exclusiveItemsService;

        #endregion

        #region Ctor

        public CategoryNavigationModelPrepareEvent(IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            IExclusiveItemsService exclusiveItemsService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _exclusiveItemsService = exclusiveItemsService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Represents an event that occurs after CategoryNavigationModel  prepare
        /// </summary>
        /// <typeparam name="eventMessage">eventMessage</typeparam>
        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            //get active store
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //If plugin is enable
            if (settings.IsEnablePlugin)
            {
                if (eventMessage.Model is CategoryNavigationModel)
                {
                    var model = eventMessage.Model as CategoryNavigationModel;

                    //Get current customer
                    var customer = await _workContext.GetCurrentCustomerAsync();

                    //Get exclusive categories from all categories
                    var exclusiveItemsCategory = model.Categories.Where(c => c.Id == settings.ExclusiveItemsCategoryId).FirstOrDefault();
                   
                    //If exclusive category exist
                    if (exclusiveItemsCategory != null)
                    {
                        //Check customer has access of exclusive category or not
                        var canAccessExclusiveCategory = _exclusiveItemsService.CanAccessExclusiveCategory(customer.Id);

                        //Get Exclusive product count
                        var exclusiveItemsCount = _exclusiveItemsService.SearchExclusiveProductsAsync(customer.Id).Result.Count();

                        //If customer has no access or customer has access but do not have any exclusive products then remove exclusive category from menu
                        if (!canAccessExclusiveCategory || exclusiveItemsCount == 0)
                           model.Categories = model.Categories.Where(c => c.Id != settings.ExclusiveItemsCategoryId).ToList();
                        else
                            //Show exclusive product count beside exclusive category
                            exclusiveItemsCategory.NumberOfProducts = exclusiveItemsCount;
                    }
                }
            }
        }

        #endregion
    }
}
