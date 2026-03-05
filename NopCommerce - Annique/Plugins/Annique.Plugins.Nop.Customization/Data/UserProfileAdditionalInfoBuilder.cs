using Annique.Plugins.Nop.Customization.Domain;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using System.Data;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class UserProfileAdditionalInfoBuilder : NopEntityBuilder<UserProfileAdditionalInfo>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(UserProfileAdditionalInfo.CustomerId)).AsInt32().ForeignKey<Customer>
                (onDelete: Rule.Cascade)
                .WithColumn(nameof(UserProfileAdditionalInfo.Title)).AsString().NotNullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.Nationality)).AsString().NotNullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.IdNumber)).AsString().NotNullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.Language)).AsString().NotNullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.Ethnicity)).AsString().NotNullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.BankName)).AsString().Nullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.AccountHolder)).AsString().Nullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.AccountNumber)).AsString().Nullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.AccountType)).AsString().Nullable()
                .WithColumn(nameof(UserProfileAdditionalInfo.BrevoID)).AsInt32().Nullable();
        }
    }
}