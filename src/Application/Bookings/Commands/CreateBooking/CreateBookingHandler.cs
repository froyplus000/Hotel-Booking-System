using HotelBooking.Application.Bookings.Queries.GetBookingById;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.CreateBooking
{
    public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _bookingsRepository;

        public CreateBookingHandler(IBookingRepository bookingRepository)
        {
            _bookingsRepository = bookingRepository;
        }

        public async Task<BookingDto> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
        {
            var booking = new Booking(
                command.UserId,
                command.CheckInDate,
                command.CheckOutDate
            );

            var savedBooking = await _bookingsRepository.AddAsync(booking, cancellationToken);
            return new BookingDto(
                BookingId: savedBooking.Id,
                CheckInDate: savedBooking.CheckInDate,
                CheckOutDate: savedBooking.CheckOutDate,
                NumberOfNights: savedBooking.NumberOfNights,
                Status: savedBooking.Status,
                TotalPrice: savedBooking.TotalPrice
            );
        }
    }
}