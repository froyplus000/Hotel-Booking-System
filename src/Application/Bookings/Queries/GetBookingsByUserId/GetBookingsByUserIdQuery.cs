using HotelBooking.Application.Bookings.Queries.GetBookingById;
using MediatR;

namespace HotelBooking.Application.Bookings.Queries.GetBookingsByUserId
{
    public record GetBookingsByUserIdQuery(Guid UserId) : IRequest<List<BookingDto>>;
}
