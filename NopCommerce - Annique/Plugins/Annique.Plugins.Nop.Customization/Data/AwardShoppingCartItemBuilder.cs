using Annique.Plugins.Nop.Customization.Domain.ConsultantAwards;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using System.Data;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class AwardShoppingCartItemBuilder : NopEntityBuilder<AwardShoppingCartItem>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(AwardShoppingCartItem.AwardId)).AsInt32().NotNullable()
                 .WithColumn(nameof(AwardShoppingCartItem.ShoppingCartItemId)).AsInt32().ForeignKey<ShoppingCartItem>
                 (onDelete: Rule.Cascade)
                 .WithColumn(nameof(AwardShoppingCartItem.CustomerId)).AsInt32().NotNullable()
                 .WithColumn(nameof(AwardShoppingCartItem.ProductId)).AsInt32().NotNullable()
                 .WithColumn(nameof(AwardShoppingCartItem.StoreId)).AsInt32().NotNullable()
                 .WithColumn(nameof(AwardShoppingCartItem.Quantity)).AsInt32().NotNullable();
        }

        #endregion
    }
}
