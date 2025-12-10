using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
    {
        public void Configure(EntityTypeBuilder<RoomType> builder)
        {
            builder.HasKey(rt => rt.Id);

            // 1. Properties
            builder.Property(rt => rt.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(rt => rt.Description)
                .HasMaxLength(500) // Give them some space for descriptions
                .IsRequired(false); // Nullable? Up to you. Let's say optional.

            // Money Precision
            builder.Property(rt => rt.BasePrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(rt => rt.Currency)
                .HasMaxLength(3) // "USD", "EUR" -> 3 chars is standard
                .IsRequired();

            // 2. Relationships
            // A RoomType belongs to a Hotel
            builder.HasOne(rt => rt.Hotel)
                .WithMany(h => h.RoomTypes)
                .HasForeignKey(rt => rt.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}