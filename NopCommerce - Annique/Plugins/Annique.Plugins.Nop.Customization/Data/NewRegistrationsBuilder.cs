using Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class NewRegistrationsBuilder : NopEntityBuilder<NewRegistrations>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
               .WithColumn("csponsor").AsCustom("char(10)").Nullable()   // CHAR(10)
                .WithColumn("CreatedOnUtc").AsDateTime().NotNullable()
                .WithColumn("UpdatedOnUtc").AsDateTime().Nullable()
                .WithColumn("ccustno").AsCustom("char(10)").Nullable()      // CHAR(10)
                .WithColumn("cLname").AsCustom("char(30)").Nullable()       // CHAR(30)
                .WithColumn("cFname").AsCustom("char(30)").Nullable()       // CHAR(30)
                .WithColumn("cCompany").AsCustom("char(40)").Nullable()     // CHAR(40)
                .WithColumn("cTitle").AsCustom("char(30)").Nullable()       // CHAR(30)
                .WithColumn("cEmail").AsCustom("char(250)").Nullable()      // CHAR(250)
                .WithColumn("cPhone1").AsCustom("char(20)").Nullable()      // CHAR(20)
                .WithColumn("cPhone2").AsCustom("char(20)").Nullable()      // CHAR(20)
                .WithColumn("cPhone3").AsCustom("char(20)").Nullable()      // CHAR(20)
                .WithColumn("cFax").AsCustom("char(20)").Nullable()         // CHAR(20)
                .WithColumn("cZip").AsCustom("char(10)").Nullable()         // CHAR(10)
                .WithColumn("ccountry").AsCustom("char(25)").Nullable()     // CHAR(25)
                .WithColumn("latitude").AsDecimal(10, 4).Nullable()
                .WithColumn("longitude").AsDecimal(10, 4).Nullable()
                .WithColumn("laccept").AsInt16().WithDefaultValue(0).NotNullable()
                .WithColumn("daccept").AsDateTime().Nullable()
                .WithColumn("besttocall").AsCustom("char(20)").Nullable()   // CHAR(20)
                .WithColumn("hearabout").AsCustom("char(20)").Nullable()    // CHAR(20)
                .WithColumn("interests").AsCustom("char(20)").Nullable()    // CHAR(20)
                .WithColumn("Status").AsCustom("char(20)").Nullable()       // CHAR(20)
                .WithColumn("Referredby").AsCustom("char(30)").Nullable()   // CHAR(30)
                .WithColumn("SMSactive").AsBoolean().Nullable()
                .WithColumn("CreatedBy").AsCustom("char(20)").Nullable()    // CHAR(20)
                .WithColumn("LastUser").AsCustom("char(30)").Nullable()     // CHAR(30)
                .WithColumn("IPAddress").AsCustom("char(20)").Nullable()    // CHAR(20)
                .WithColumn("Browser").AsString().Nullable()                // VARCHAR(MAX)
                .WithColumn("cLanguage").AsCustom("varchar(20)").Nullable()
                .WithColumn("ActivateLink").AsCustom("varchar(36)").Nullable();        // VARCHAR(36)
        }

        #endregion
    }
}
