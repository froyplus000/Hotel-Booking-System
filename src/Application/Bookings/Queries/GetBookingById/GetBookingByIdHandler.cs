using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
    {
        // Dependencies we need
        private readonly IBookingRepository _bookingRepository;

        // Constructor (Dependency Injection)
        public GetBookingByIdHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        // MediatR calls this method automatically
        public async Task<BookingDto> Handle(GetBookingByIdQuery query, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(query.BookingId, cancellationToken);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {query.BookingId} not found");
            }
            return new BookingDto(
                BookingId: booking.Id,
                CheckInDate: booking.CheckInDate,
                CheckOutDate: booking.CheckOutDate,
                NumberOfNights: booking.NumberOfNights,
                Status: booking.Status,
                TotalPrice: booking.TotalPrice
            );

        }
    }
}
