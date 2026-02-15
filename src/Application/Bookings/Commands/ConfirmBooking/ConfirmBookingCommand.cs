using HotelBooking.Application.Bookings.Queries.GetBookingById;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.ConfirmBooking
{
    public record ConfirmBookingCommand(Guid BookingId) : IRequest<BookingDto>;
}
