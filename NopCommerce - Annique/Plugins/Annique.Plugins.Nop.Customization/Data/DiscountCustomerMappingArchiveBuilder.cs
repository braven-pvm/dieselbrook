using Annique.Plugins.Nop.Customization.Domain.DiscountAllocation;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    /// <summary>
    /// Represents a DiscountCustomerMappingArchive entity builder
    /// </summary>
    public class DiscountCustomerMappingArchiveBuilder : NopEntityBuilder<DiscountCustomerMappingArchive>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DiscountCustomerMappingArchive.DiscountId))
                    .AsInt32().ForeignKey<Discount>()
                .WithColumn(nameof(DiscountCustomerMappingArchive.CustomerId))
                    .AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(DiscountCustomerMappingArchive.Comment)).AsString().Nullable()
                .WithColumn(nameof(DiscountCustomerMappingArchive.Notified))
                    .AsBoolean().Nullable();

        }

        #endregion
    }
}
