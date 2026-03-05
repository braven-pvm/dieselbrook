using Annique.Plugins.Nop.Customization.Domain;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class CustomerChangesBuilder :NopEntityBuilder<CustomerChanges>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(CustomerChanges.ChangeId)).AsInt32().Nullable()
                 .WithColumn(nameof(CustomerChanges.cTableName)).AsString(50).Nullable()
                 .WithColumn(nameof(CustomerChanges.CustomerId)).AsInt32().Nullable()
                 .WithColumn(nameof(CustomerChanges.cCustno)).AsString(10).Nullable()
                 .WithColumn(nameof(CustomerChanges.cFieldname)).AsString(50).Nullable()
                 .WithColumn(nameof(CustomerChanges.cOldvalue)).AsString(int.MaxValue).Nullable()
                 .WithColumn(nameof(CustomerChanges.cNewvalue)).AsString(int.MaxValue).Nullable()
                 .WithColumn(nameof(CustomerChanges.InsUpdDate)).AsDateTime().Nullable()
                 .WithColumn(nameof(CustomerChanges.Updated)).AsDateTime().Nullable();
        }

        #endregion
    }
}