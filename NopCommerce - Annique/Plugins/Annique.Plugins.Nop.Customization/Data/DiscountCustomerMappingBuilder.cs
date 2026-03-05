using Annique.Plugins.Nop.Customization.Domain.DiscountAllocation;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    /// <summary>
    /// Represents a discount customer mapping entity builder
    /// </summary>
    public class DiscountCustomerMappingBuilder : NopEntityBuilder<DiscountCustomerMapping>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DiscountCustomerMapping.DiscountId))
                    .AsInt32().ForeignKey<Discount>()
                .WithColumn(nameof(DiscountCustomerMapping.CustomerId))
                    .AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(DiscountCustomerMapping.Comment)).AsString().Nullable()
                .WithColumn(nameof(DiscountCustomerMapping.Notified))
                    .AsBoolean().Nullable()
                .WithColumn(nameof(DiscountCustomerMapping.NotifyWhatsApp))
                    .AsBoolean().Nullable();

        }

        #endregion
    }
}
