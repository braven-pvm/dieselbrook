using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using System.Data;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class ReportParameterValueBuilder : NopEntityBuilder<ReportParameterValue>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ReportParameterValue.ReportParameterId)).AsInt32().ForeignKey<ReportParameter>
                (onDelete: Rule.Cascade);
        }
    }
}

