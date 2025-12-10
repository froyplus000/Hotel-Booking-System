using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            // 1. Properties
            builder.Property(b => b.TotalPrice)
                .HasPrecision(18, 2) // Always set precision for Money!
                .IsRequired();

            builder.Property(b => b.CheckInDate)
                .IsRequired(); // Maps to 'timestamp' automatically

            builder.Property(b => b.CheckOutDate)
                .IsRequired();

            // 2. Enum Conversion (Senior Tip!)
            // Saves "Confirmed" instead of "1" in the database
            builder.Property(b => b.Status)
                .HasConversion<string>()
                .IsRequired();

            // 3. Relationships
            // User Relationship (We already did the other side in UserConfig, but defining it here is safe too)
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // The "Rooms" collection is private, so we tell EF Core to access the field directly
            builder.Metadata.FindNavigation(nameof(Booking.Rooms))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}