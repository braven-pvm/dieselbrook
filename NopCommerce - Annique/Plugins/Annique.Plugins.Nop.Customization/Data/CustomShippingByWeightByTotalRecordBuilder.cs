using Annique.Plugins.Nop.Customization.Domain.ShippingRule;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class CustomShippingByWeightByTotalRecordBuilder : NopEntityBuilder<CustomShippingByWeightByTotalRecord>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.WeightFrom)).AsDecimal(18, 4)
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.WeightTo)).AsDecimal(18, 4)
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.OrderSubtotalFrom)).AsDecimal(18, 4)
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.OrderSubtotalTo)).AsDecimal(18, 4)
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.AdditionalFixedCost)).AsDecimal(18, 4)
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.PercentageRateOfSubtotal)).AsDecimal(18, 4)
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.RatePerWeightUnit)).AsDecimal(18, 4)
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.LowerWeightLimit)).AsDecimal(18, 4)
             .WithColumn(nameof(CustomShippingByWeightByTotalRecord.Zip)).AsString(400).Nullable();
        }

        #endregion
    }
}
