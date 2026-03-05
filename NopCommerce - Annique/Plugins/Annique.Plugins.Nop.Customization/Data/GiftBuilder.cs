using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class GiftBuilder : NopEntityBuilder<Gift>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Gift.Sku)).AsString(50).NotNullable()
                .WithColumn(nameof(Gift.ProductId)).AsInt32().NotNullable()
                .WithColumn(nameof(Gift.nQtyLimit)).AsInt32().NotNullable()
                .WithColumn(nameof(Gift.nMinSales)).AsDecimal().Nullable()
                .WithColumn(nameof(Gift.cGiftType)).AsString(10).Nullable()
                .WithColumn(nameof(Gift.CampaignId)).AsInt32().Nullable()
                .WithColumn(nameof(Gift.StartDateUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(Gift.EndDateUtc)).AsDateTime2().NotNullable();
        }

        #endregion
    }
}
