using HotelBooking.Domain.Entities;

namespace HotelBooking.Domain.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<List<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        
        Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default);
        
        Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
        
        Task DeleteAsync(Guid bookingId, CancellationToken cancellationToken = default);
    }
}