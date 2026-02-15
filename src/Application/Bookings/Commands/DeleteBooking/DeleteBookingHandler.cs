using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.DeleteBooking
{
    public class DeleteBookingHandler : IRequestHandler<DeleteBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;

        public DeleteBookingHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task Handle(DeleteBookingCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {command.BookingId} was not found.");
            }

            await _bookingRepository.DeleteAsync(booking.Id, cancellationToken);
        }
    }
}
