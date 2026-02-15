using MediatR;

namespace HotelBooking.Application.Bookings.Commands.DeleteBooking
{
    public record DeleteBookingCommand(Guid BookingId) : IRequest;
}
