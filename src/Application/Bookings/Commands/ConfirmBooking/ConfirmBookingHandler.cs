using HotelBooking.Application.Bookings.Queries.GetBookingById;
using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.ConfirmBooking
{
    public class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _bookingRepository;

        public ConfirmBookingHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingDto> Handle(ConfirmBookingCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {command.BookingId} was not found.");
            }

            booking.Confirm();
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
