using HotelBooking.Application.Bookings.Commands.CancelBooking;
using HotelBooking.Application.Bookings.Commands.ConfirmBooking;
using HotelBooking.Application.Bookings.Commands.CreateBooking;
using HotelBooking.Application.Bookings.Commands.DeleteBooking;
using HotelBooking.Application.Bookings.Commands.UpdateBooking;
using HotelBooking.Application.Bookings.Queries.GetBookingById;
using HotelBooking.Application.Bookings.Queries.GetBookingsByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var query = new GetBookingByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingsByUserId([FromQuery] Guid userId)
        {
            var query = new GetBookingsByUserIdQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetBookingById), new { id = result.BookingId }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] UpdateBookingCommand request)
        {
            var command = new UpdateBookingCommand(id, request.CheckInDate, request.CheckOutDate);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id:guid}/confirm")]
        public async Task<IActionResult> ConfirmBooking(Guid id)
        {
            var command = new ConfirmBookingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var command = new CancelBookingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            var command = new DeleteBookingCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
