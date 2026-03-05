using Annique.Plugins.Nop.Customization.Domain.ConsultantAwards;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class AwardBuilder : NopEntityBuilder<Award>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(Award.CustomerId)).AsInt32().NotNullable()
                 .WithColumn(nameof(Award.AwardType)).AsString(10).Nullable()
                 .WithColumn(nameof(Award.Description)).AsString(40).Nullable()
                 .WithColumn(nameof(Award.MaxValue)).AsInt32().NotNullable()
                 .WithColumn(nameof(Award.ExpiryDate)).AsDateTime().NotNullable()
                 .WithColumn(nameof(Award.OrderId)).AsInt32().Nullable()
                 .WithColumn(nameof(Award.dcreated)).AsDateTime().Nullable()
                 .WithColumn(nameof(Award.dtaken)).AsDateTime().Nullable();
        }

        #endregion
    }
}