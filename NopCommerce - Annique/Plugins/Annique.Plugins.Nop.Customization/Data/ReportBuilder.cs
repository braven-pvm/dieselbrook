using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class ReportBuilder : NopEntityBuilder<Report>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Report.Name)).AsString(100).NotNullable()
                .WithColumn(nameof(Report.Published)).AsString(400).Nullable();
        }

        #endregion
    }
}