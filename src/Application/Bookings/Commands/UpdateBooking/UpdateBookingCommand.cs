using HotelBooking.Application.Bookings.Queries.GetBookingById;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.UpdateBooking
{
    public record UpdateBookingCommand(
        Guid BookingId,
        DateTime CheckInDate,
        DateTime CheckOutDate
    ) : IRequest<BookingDto>;
}
