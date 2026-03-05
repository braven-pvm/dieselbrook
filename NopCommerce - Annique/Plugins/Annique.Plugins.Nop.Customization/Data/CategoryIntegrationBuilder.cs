using Annique.Plugins.Nop.Customization.Domain;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using System.Data;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class CategoryIntegrationBuilder : NopEntityBuilder<CategoryIntegration>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CategoryIntegration.CategoryId)).AsInt32().ForeignKey<Category>
                (onDelete: Rule.Cascade)
                .WithColumn(nameof(CategoryIntegration.IntegrationField)).AsString(20).NotNullable()
                .WithColumn(nameof(CategoryIntegration.IntegrationValue)).AsString(40).NotNullable();
        }
    }
}
