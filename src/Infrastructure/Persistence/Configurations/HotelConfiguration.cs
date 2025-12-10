using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            // 1. Primary Key
            builder.HasKey(h => h.Id);

            // 2. Properties (Validation Rules)
            builder.Property(h => h.Name)
                .HasMaxLength(100)  // "Senior" Move: Limit string size
                .IsRequired();

            builder.Property(h => h.Address)
                .HasMaxLength(200)
                .IsRequired();
                
            builder.Property(h => h.City)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(h => h.Country)
                .HasMaxLength(100)
                .IsRequired();

            // 3. Relationships
            // "This Hotel has many RoomTypes..."
            builder.HasMany(h => h.RoomTypes)
                // "...and each RoomType is linked with one Hotel"
                .WithOne(rt => rt.Hotel)
                // "The Foreign Key sits on the RoomType table"
                .HasForeignKey(rt => rt.HotelId)
                // "If I delete the Hotel, delete all its RoomTypes too"
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}