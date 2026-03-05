using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class GiftsTakenBuilder : NopEntityBuilder<GiftsTaken>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(GiftsTaken.GiftId)).AsInt32().NotNullable()
                .WithColumn(nameof(GiftsTaken.CustomerId)).AsInt32().NotNullable()
                .WithColumn(nameof(GiftsTaken.OrderItemId)).AsInt32().NotNullable()
                .WithColumn(nameof(GiftsTaken.Qty)).AsInt32().NotNullable();
        }

        #endregion
    }
}

