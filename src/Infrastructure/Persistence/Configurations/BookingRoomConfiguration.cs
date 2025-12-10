using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class BookingRoomConfiguration : IEntityTypeConfiguration<BookingRoom>
    {
        public void Configure(EntityTypeBuilder<BookingRoom> builder)
        {
            // 1. THE COMPOSITE KEY
            // "The ID of this table is the COMBINATION of BookingId and RoomId"
            builder.HasKey(br => new { br.BookingId, br.RoomId });

            // 2. Properties
            builder.Property(br => br.PriceAtBooking)
                .HasPrecision(18, 2) // Best practice for Money in SQL
                .IsRequired();

            // 3. Relationships
            builder.HasOne(br => br.Room)
                .WithMany() // Room doesn't strictly need a list of BookingRooms
                .HasForeignKey(br => br.RoomId);
                
            // We don't need to configure the 'Booking' side here because 
            // we already did "HasMany(Rooms)" inside the Booking entity/config 
            // (or EF Core infers it).
        }
    }
}