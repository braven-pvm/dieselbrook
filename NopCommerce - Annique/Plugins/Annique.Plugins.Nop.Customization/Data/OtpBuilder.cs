using Annique.Plugins.Nop.Customization.Domain;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class OtpBuilder : NopEntityBuilder<Otp>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Otp.CustomerID)).AsInt32().Nullable()
                .WithColumn(nameof(Otp.OTP)).AsInt32().Nullable()
                .WithColumn(nameof(Otp.Expiry)).AsDateTime().Nullable()
                .WithColumn(nameof(Otp.Cell)).AsString(20).Nullable()
                .WithColumn(nameof(Otp.Email)).AsString(100).Nullable()
                .WithColumn(nameof(Otp.Iverified)).AsBoolean().Nullable();
        }

        #endregion
    }
}
