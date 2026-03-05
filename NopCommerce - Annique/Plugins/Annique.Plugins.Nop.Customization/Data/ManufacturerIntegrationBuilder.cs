using System.Data;
using Annique.Plugins.Nop.Customization.Domain;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class ManufacturerIntegrationBuilder : NopEntityBuilder<ManufacturerIntegration>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ManufacturerIntegration.ManufacturerId)).AsInt32().ForeignKey<Manufacturer>
                (onDelete: Rule.Cascade)
                .WithColumn(nameof(CategoryIntegration.IntegrationField)).AsString(20).NotNullable()
                .WithColumn(nameof(CategoryIntegration.IntegrationValue)).AsString(40).NotNullable();
        }
    }
}
