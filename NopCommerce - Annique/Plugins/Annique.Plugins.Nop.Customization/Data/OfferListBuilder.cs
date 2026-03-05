using Annique.Plugins.Nop.Customization.Domain.SpecialOffers;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    /// <summary>
    /// Represents a offer list entity builder
    /// </summary>
    public class OfferListBuilder : NopEntityBuilder<OfferList>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
               .WithColumn(nameof(OfferList.ListType)).AsString(1).Nullable()
               .WithColumn(nameof(OfferList.OfferId)).AsInt32().Nullable()
               .WithColumn(nameof(OfferList.ProductId)).AsInt32().Nullable()
               .WithColumn(nameof(OfferList.citemno)).AsString(20).Nullable();
        }

        #endregion
    }
}
