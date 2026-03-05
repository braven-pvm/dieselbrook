using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using System.Data;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class ReportParameterBuilder : NopEntityBuilder<ReportParameter>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ReportParameter.Name)).AsString(100).NotNullable()
                .WithColumn(nameof(ReportParameter.ReportId)).AsInt32().ForeignKey<Report>
                (onDelete: Rule.Cascade);
        }
    }
}

