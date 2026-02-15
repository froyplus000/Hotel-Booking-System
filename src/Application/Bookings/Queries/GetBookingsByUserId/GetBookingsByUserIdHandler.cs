using HotelBooking.Application.Bookings.Queries.GetBookingById;
using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Bookings.Queries.GetBookingsByUserId
{
    public class GetBookingsByUserIdHandler : IRequestHandler<GetBookingsByUserIdQuery, List<BookingDto>>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetBookingsByUserIdHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<List<BookingDto>> Handle(GetBookingsByUserIdQuery query, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(query.UserId, cancellationToken);

            return bookings.Select(booking => new BookingDto(
                BookingId: booking.Id,
                CheckInDate: booking.CheckInDate,
                CheckOutDate: booking.CheckOutDate,
                NumberOfNights: booking.NumberOfNights,
                Status: booking.Status,
                TotalPrice: booking.TotalPrice
            )).ToList();
        }
    }
}
