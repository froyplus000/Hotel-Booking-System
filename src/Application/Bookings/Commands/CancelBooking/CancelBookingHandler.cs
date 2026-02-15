using HotelBooking.Application.Bookings.Queries.GetBookingById;
using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.CancelBooking
{
    public class CancelBookingHandler : IRequestHandler<CancelBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _bookingRepository;

        public CancelBookingHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingDto> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {command.BookingId} was not found.");
            }

            booking.Cancel();
            await _bookingRepository.UpdateAsync(booking, cancellationToken);

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
