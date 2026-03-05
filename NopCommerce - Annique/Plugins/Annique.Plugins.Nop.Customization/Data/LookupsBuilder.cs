using Annique.Plugins.Nop.Customization.Domain;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class LookupsBuilder : NopEntityBuilder<Lookups>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Lookups.ctype)).AsString(12).NotNullable()
                .WithColumn(nameof(Lookups.code)).AsString(20).NotNullable()
                .WithColumn(nameof(Lookups.description)).AsString(35).NotNullable()
                .WithColumn(nameof(Lookups.Iactive)).AsBoolean().NotNullable()
                .WithColumn(nameof(Lookups.StoreId)).AsInt32().NotNullable();
        }

        #endregion
    }
}

