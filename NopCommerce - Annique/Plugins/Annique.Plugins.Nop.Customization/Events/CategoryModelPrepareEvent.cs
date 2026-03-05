using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Web.Factories;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// CategoryModel prepare event
    /// </summary>
    public class CategoryModelPrepareEvent : IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IExclusiveItemsService _exclusiveItemsService;
        private readonly IProductModelFactory _productModelFactory;

        #endregion

        #region Ctor

        public CategoryModelPrepareEvent(IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            IExclusiveItemsService exclusiveItemsService,
            IProductModelFactory productModelFactory)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _exclusiveItemsService = exclusiveItemsService;
            _productModelFactory = productModelFactory;
        }

        #endregion

        #region Method

        /// <summary>
        /// Represents an event that occurs after CategoryModel  prepare
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
                if (eventMessage.Model is CategoryModel)
                {
                    var model = eventMessage.Model as CategoryModel;

                    if(model.Id == settings.ExclusiveItemsCategoryId)
                    {
                        //Get current customer
                        var customer = await _workContext.GetCurrentCustomerAsync();

                        //Check customer have access to exclusive category or not
                        var canAccessExclusiveCategory = _exclusiveItemsService.CanAccessExclusiveCategory(customer.Id);
                        
                        //If have access
                        if(canAccessExclusiveCategory) 
                        {
                            //Get exclusive category products
                            var exclusiveProducts = await _exclusiveItemsService.SearchExclusiveProductsAsync(customer.Id);

                            //Add exclusive products to catalog product model
                            model.CatalogProductsModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(exclusiveProducts)).ToList();
                        }
                    }
                }
            }
        }

        #endregion
    }
}

