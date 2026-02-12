using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Bookings.Queries.GetBookingById
{
    public record BookingDto(
        Guid BookingId,
        DateTime CheckInDate,
        DateTime CheckOutDate,
        int NumberOfNights,
        BookingStatus Status,
        decimal TotalPrice
    );
}