using HotelBooking.Application.Bookings.Queries.GetBookingById;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.CancelBooking
{
    public record CancelBookingCommand(Guid BookingId) : IRequest<BookingDto>;
}
