using Annique.Plugins.Nop.Customization.Domain.ShippingRule;
using Annique.Plugins.Nop.Customization.Models.ShippingRule;
using Annique.Plugins.Nop.Customization.Services.ShippingRule;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateProvince = Nop.Core.Domain.Directory.StateProvince;

namespace Annique.Plugins.Nop.Customization.Factories.ShippingRule
{
    public class CustomShippingRuleFactory : ICustomShippingRuleFactory
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly ICustomShippingRuleService _customShippingRuleService;
        private readonly IShippingService _shippingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly MeasureSettings _measureSettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public CustomShippingRuleFactory(CurrencySettings currencySettings,
            ICountryService countryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            ICustomShippingRuleService customShippingRuleService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            MeasureSettings measureSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            ICustomerService customerService)
        {
            _currencySettings = currencySettings;
            _countryService = countryService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _measureService = measureService;
            _customShippingRuleService = customShippingRuleService;
            _stateProvinceService = stateProvinceService;
            _shippingService = shippingService;
            _storeService = storeService;
            _measureSettings = measureSettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _customerService = customerService;
        }

        #endregion
        /// <summary>
        /// Prepare shipping search model
        /// </summary>
        /// <param name="searchModel">shipping rule search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipping rule search model
        /// </returns>
        public Task<CustomShippingRuleSearchModel> PrepareCustomShippingRuleSearchModelAsync(CustomShippingRuleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged Report list model
        /// </summary>
        /// <param name="searchModel">Report search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the custom shipping rule list model
        /// </returns>
        public async Task<CustomShippingRuleListModel> PrepareCustomShippingRuleListModelAsync(CustomShippingRuleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Fetch all customer roles before processing the records
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);

            var records = await _customShippingRuleService.GetAllAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
           
            var gridModel = await new CustomShippingRuleListModel().PrepareToGridAsync(searchModel, records, () =>
            {
                return records.SelectAwait(async record =>
                {
                    var model = new CustomShippingRuleModel
                    {
                        Id = record.Id,
                        StoreId = record.StoreId,
                        StoreName = (await _storeService.GetStoreByIdAsync(record.StoreId))?.Name ?? "*",
                        ShippingMethodId = record.ShippingMethodId,
                        ShippingMethodName = (await _shippingService.GetShippingMethodByIdAsync(record.ShippingMethodId))?.Name ?? "Unavailable",
                        CountryId = record.CountryId,
                        CountryName = (await _countryService.GetCountryByIdAsync(record.CountryId))?.Name ?? "*",
                        WeightFrom = record.WeightFrom,
                        WeightTo = record.WeightTo,
                        OrderSubtotalFrom = record.OrderSubtotalFrom,
                        OrderSubtotalTo = record.OrderSubtotalTo,
                        AdditionalFixedCost = record.AdditionalFixedCost,
                        PercentageRateOfSubtotal = record.PercentageRateOfSubtotal,
                        RatePerWeightUnit = record.RatePerWeightUnit,
                        LowerWeightLimit = record.LowerWeightLimit,
                    };

                    var htmlSb = new StringBuilder("<div>");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.FixedByWeightByTotal.Fields.WeightFrom"),
                        model.WeightFrom);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.FixedByWeightByTotal.Fields.WeightTo"),
                        model.WeightTo);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.FixedByWeightByTotal.Fields.OrderSubtotalFrom"),
                        model.OrderSubtotalFrom);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.FixedByWeightByTotal.Fields.OrderSubtotalTo"),
                        model.OrderSubtotalTo);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.FixedByWeightByTotal.Fields.AdditionalFixedCost"),
                        model.AdditionalFixedCost);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.FixedByWeightByTotal.Fields.RatePerWeightUnit"),
                        model.RatePerWeightUnit);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.FixedByWeightByTotal.Fields.LowerWeightLimit"),
                        model.LowerWeightLimit);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.FixedByWeightByTotal.Fields.PercentageRateOfSubtotal"),
                        model.PercentageRateOfSubtotal);

                    htmlSb.Append("</div>");
                    model.DataHtml = htmlSb.ToString();

                    await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, record, false);

                    // Fetch role names by matching the SelectedCustomerRoleIds with the role Ids
                    var roleNames = allCustomerRoles
                        .Where(role => model.SelectedCustomerRoleIds.Contains(role.Id))
                        .Select(role => role.Name)
                        .ToList();

                    // Set the CustomerRoles property to be shown in the grid column
                    model.CustomerRoles = string.Join(", ", roleNames);


                    // Set the CustomerRoles property to be shown in the grid column
                    model.CustomerRoles = string.Join(", ", roleNames);
                    return model;
                });
            });

            return gridModel;
        }

        /// <summary>
        /// Prepare Report model
        /// </summary>
        /// <param name="model">custom shippingrule model</param>
        /// <param name="customShippingByWeightByTotalRecord">Custom shipping rule</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the custom shipping rule model
        /// </returns>
        public async Task<CustomShippingRuleModel> PrepareCustomShippingRuleModelAsync(
    CustomShippingRuleModel model,
    CustomShippingByWeightByTotalRecord sbw,
    bool excludeProperties = false)
        {
            // Get all shipping methods once to avoid redundant calls.
            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);

            if (sbw != null)
            {
                // Mapping properties from sbw to model
                model = await MapShippingRuleToModel(sbw);

                // Get additional data for sbw's associated entities
                var selectedStore = await _storeService.GetStoreByIdAsync(sbw.StoreId);
                var selectedWarehouse = await _shippingService.GetWarehouseByIdAsync(sbw.WarehouseId);
                var selectedShippingMethod = await _shippingService.GetShippingMethodByIdAsync(sbw.ShippingMethodId);
                var selectedCountry = await _countryService.GetCountryByIdAsync(sbw.CountryId);
                var selectedState = await _stateProvinceService.GetStateProvinceByIdAsync(sbw.StateProvinceId);

                // Populating the available selects
                await PopulateSelectLists(model, selectedStore, selectedWarehouse, selectedShippingMethod, selectedCountry, selectedState, shippingMethods, countries);
            }
            else
            {
                // Populating the available selects when sbw is null
                await PopulateSelectLists(model, null, null, null, null, null, shippingMethods, countries);
            }

            // Prepare model for customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, sbw, excludeProperties);

            return model;
        }

        // Helper method to map sbw to model
        private async Task<CustomShippingRuleModel> MapShippingRuleToModel(CustomShippingByWeightByTotalRecord sbw)
        {
            return new CustomShippingRuleModel
            {
                Id = sbw.Id,
                StoreId = sbw.StoreId,
                WarehouseId = sbw.WarehouseId,
                CountryId = sbw.CountryId,
                StateProvinceId = sbw.StateProvinceId,
                Zip = sbw.Zip,
                ShippingMethodId = sbw.ShippingMethodId,
                WeightFrom = sbw.WeightFrom,
                WeightTo = sbw.WeightTo,
                OrderSubtotalFrom = sbw.OrderSubtotalFrom,
                OrderSubtotalTo = sbw.OrderSubtotalTo,
                AdditionalFixedCost = sbw.AdditionalFixedCost,
                PercentageRateOfSubtotal = sbw.PercentageRateOfSubtotal,
                RatePerWeightUnit = sbw.RatePerWeightUnit,
                LowerWeightLimit = sbw.LowerWeightLimit,
                PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode,
                BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name,
                TransitDays = sbw.TransitDays
            };
        }

        // Helper method to populate SelectLists
        private async Task PopulateSelectLists(
            CustomShippingRuleModel model,
            Store selectedStore,
            Warehouse selectedWarehouse,
            ShippingMethod selectedShippingMethod,
            Country selectedCountry,
            StateProvince selectedState,
            IEnumerable<ShippingMethod> shippingMethods,
            IEnumerable<Country> countries)
        {
            // Stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString(), Selected = selectedStore != null && store.Id == selectedStore.Id });

            // Warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouse in await _shippingService.GetAllWarehousesAsync())
                model.AvailableWarehouses.Add(new SelectListItem { Text = warehouse.Name, Value = warehouse.Id.ToString(), Selected = selectedWarehouse != null && warehouse.Id == selectedWarehouse.Id });

            // Shipping Methods
            foreach (var sm in shippingMethods)
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString(), Selected = selectedShippingMethod != null && sm.Id == selectedShippingMethod.Id });

            // Countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var country in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = country.Name, Value = country.Id.ToString(), Selected = selectedCountry != null && country.Id == selectedCountry.Id });

            // States
            var states = selectedCountry != null ? (await _stateProvinceService.GetStateProvincesByCountryIdAsync(selectedCountry.Id, showHidden: true)).ToList() : new List<StateProvince>();
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var state in states)
                model.AvailableStates.Add(new SelectListItem { Text = state.Name, Value = state.Id.ToString(), Selected = selectedState != null && state.Id == selectedState.Id });
        }

        public CustomShippingByWeightByTotalRecord PrepareCustomShippingRuleFields(CustomShippingRuleModel model)
        {
            if (model != null)
            {
                var customShippingRule = new CustomShippingByWeightByTotalRecord()
                {
                    Id = model.Id,
                    StoreId = model.StoreId,
                    WarehouseId = model.WarehouseId,
                    CountryId = model.CountryId,
                    StateProvinceId = model.StateProvinceId,
                    Zip = model.Zip == "*" ? null : model.Zip,
                    ShippingMethodId = model.ShippingMethodId,
                    WeightFrom = model.WeightFrom,
                    WeightTo = model.WeightTo,
                    OrderSubtotalFrom = model.OrderSubtotalFrom,
                    OrderSubtotalTo = model.OrderSubtotalTo,
                    AdditionalFixedCost = model.AdditionalFixedCost,
                    RatePerWeightUnit = model.RatePerWeightUnit,
                    PercentageRateOfSubtotal = model.PercentageRateOfSubtotal,
                    LowerWeightLimit = model.LowerWeightLimit,
                    TransitDays = model.TransitDays
                };

                return customShippingRule;
            }
            return null;
        }
    }
}
