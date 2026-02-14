using HotelBooking.Application.Bookings.Queries.GetBookingById;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.CreateBooking
{
    public record CreateBookingCommand(
        Guid UserId,
        DateTime CheckInDate,
        DateTime CheckOutDate
    ) : IRequest<BookingDto>;
}