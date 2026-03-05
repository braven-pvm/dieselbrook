using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class EventItemsBuilder : NopEntityBuilder<EventItems>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(EventItems.EventID)).AsInt32().NotNullable()
                .WithColumn(nameof(EventItems.ProductID)).AsInt32().NotNullable()
                .WithColumn(nameof(EventItems.nQtyLimit)).AsInt32().NotNullable()
                .WithColumn(nameof(EventItems.dFrom)).AsDateTime().NotNullable()
                .WithColumn(nameof(EventItems.dTo)).AsDateTime().NotNullable()
                .WithColumn(nameof(EventItems.Sku)).AsString(200).Nullable()
                .WithColumn(nameof(EventItems.IActive)).AsBoolean().NotNullable();
        }

        #endregion
    }
}
