using Annique.Plugins.Nop.Customization.Domain;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using System.Data;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class GiftCardAdditionalInfoBuilder : NopEntityBuilder<GiftCardAdditionalInfo>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(GiftCardAdditionalInfo.GiftCardId)).AsInt32().ForeignKey<GiftCard>
                (onDelete: Rule.Cascade)
                .WithColumn(nameof(GiftCardAdditionalInfo.Username)).AsString(50).NotNullable();
        }
    }
}
