using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Persistence
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
        {
        }

        // The Tables
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        // We don't usually need a DbSet for "Child" entities like Room/BookingRoom 
        // because we access them through the parent (Hotels/Bookings).
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This is where we will configure the "Rules" (Keys, Relationships)
            // For now, let's just apply configurations from the assembly.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}