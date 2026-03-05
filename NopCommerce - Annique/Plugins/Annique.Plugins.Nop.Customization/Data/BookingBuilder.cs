using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Annique.Plugins.Nop.Customization.Data
{
    public class BookingBuilder : NopEntityBuilder<Booking>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Booking.EventID)).AsInt32().NotNullable()
                .WithColumn(nameof(Booking.CustomerID)).AsInt32().NotNullable()
                .WithColumn(nameof(Booking.ConsultantCustomerID)).AsInt32().Nullable()
                .WithColumn(nameof(Booking.Name)).AsString(50).NotNullable()
                .WithColumn(nameof(Booking.Status)).AsString(10).Nullable()
                .WithColumn(nameof(Booking.DateBooked)).AsDateTime2().Nullable()
                .WithColumn(nameof(Booking.Attended)).AsString(3).Nullable()
                .WithColumn(nameof(Booking.OrderID)).AsInt32().Nullable()
                .WithColumn(nameof(Booking.cSono)).AsString(10).Nullable()
                .WithColumn(nameof(Booking.cInvno)).AsString(10).Nullable()
                .WithColumn(nameof(Booking.IsPrimaryRegistrant)).AsBoolean().Nullable()
                .WithColumn(nameof(Booking.dlastupd)).AsDateTime2().Nullable()
                .WithColumn(nameof(Booking.IEmail)).AsBoolean().Nullable();
        }

        #endregion
    }
}

