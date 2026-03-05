using Annique.Plugins.Nop.Customization.Domain.SpecialOffers;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    /// <summary>
    /// Represents a discount customer mapping entity builder
    /// </summary>
    public class OffersBuilder : NopEntityBuilder<Offers>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Offers.DiscountId))
                    .AsInt32().Nullable()
               .WithColumn(nameof(Offers.RuleType)).AsString(50).Nullable()
               .WithColumn(nameof(Offers.MinQty)).AsInt32().Nullable()
               .WithColumn(nameof(Offers.MaxQty)).AsInt32().Nullable()
               .WithColumn(nameof(Offers.MinValue)).AsDecimal().Nullable()
               .WithColumn(nameof(Offers.MaxValue)).AsDecimal().Nullable()
               .WithColumn(nameof(Offers.PictureId)).AsInt32().Nullable();
        }

        #endregion
    }
}
