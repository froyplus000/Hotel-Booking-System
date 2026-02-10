using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Repositories;
using Microsoft.EntityFrameworkCore;


namespace HotelBooking.Infrastructure.Persistence.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly HotelDbContext _context;

        public BookingRepository(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Rooms).ThenInclude(r => r.Room)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }
        
        public async Task<List<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {

            return await _context.Bookings
                .Include(b => b.Rooms).ThenInclude(r => r.Room)
                .Where(b => b.UserId == userId)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(cancellationToken);
            return booking;
        }
        
        public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        public async Task DeleteAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
            
            if (booking == null)
            {
                throw new InvalidOperationException($"Booking with ID {bookingId} not found");
            }
            
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}