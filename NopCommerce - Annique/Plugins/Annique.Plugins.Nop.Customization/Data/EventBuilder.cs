using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class EventBuilder : NopEntityBuilder<Event>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Event.Name)).AsString(100).NotNullable()
                .WithColumn(nameof(Event.StartDateTime)).AsDateTime().NotNullable()
                .WithColumn(nameof(Event.EndDateTime)).AsDateTime().NotNullable()
                .WithColumn(nameof(Event.LocationName)).AsString(50).Nullable()
                .WithColumn(nameof(Event.LocationAddress1)).AsString(50).Nullable()
                .WithColumn(nameof(Event.LocationAddress2)).AsString(50).Nullable()
                .WithColumn(nameof(Event.LocationCity)).AsString(50).Nullable()
                .WithColumn(nameof(Event.LocationLocation)).AsString(50).Nullable()
                .WithColumn(nameof(Event.LocationPostalCode)).AsString(50).Nullable()
                .WithColumn(nameof(Event.LocationCountry)).AsString(50).Nullable()
                .WithColumn(nameof(Event.ContactName)).AsString(50).Nullable()
                .WithColumn(nameof(Event.ContactEmail)).AsString(50).Nullable()
                .WithColumn(nameof(Event.ContactPhone)).AsString(50).Nullable()
                .WithColumn(nameof(Event.ShortDescription)).AsString().Nullable()
                .WithColumn(nameof(Event.TicketCode)).AsString(20).Nullable()
                .WithColumn(nameof(Event.ProductID)).AsInt32().Nullable()
                .WithColumn(nameof(Event.Bookingopen)).AsBoolean().NotNullable()
                .WithColumn(nameof(Event.IActive)).AsBoolean().NotNullable()
                .WithColumn(nameof(Event.dlastupd)).AsDateTime().Nullable()
                .WithColumn(nameof(Event.ZoomCode)).AsString(40).Nullable()
                .WithColumn(nameof(Event.IsOnline)).AsBoolean().Nullable()
                .WithColumn(nameof(Event.Published)).AsBoolean().NotNullable()
                .WithColumn(nameof(Event.isField)).AsBoolean().Nullable()
                .WithColumn(nameof(Event.isOptIn)).AsBoolean().Nullable()
                .WithColumn(nameof(Event.CloseDays)).AsInt32().Nullable()
                .WithColumn(nameof(Event.BookingOpenDays)).AsInt32().Nullable()
                .WithColumn(nameof(Event.HOAprovalDays)).AsInt32().Nullable()
                .WithColumn(nameof(Event.NOTIFICATIONDays)).AsInt32().Nullable()
                .WithColumn(nameof(Event.LoadItemsDays)).AsInt32().Nullable()
                .WithColumn(nameof(Event.NotOrderedDays)).AsInt32().Nullable();
        }

        #endregion
    }
}