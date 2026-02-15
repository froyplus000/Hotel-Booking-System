using HotelBooking.Application.Users.Commands.CreateUser;
using HotelBooking.Application.Users.Commands.DeleteUser;
using HotelBooking.Application.Users.Commands.UpdateUser;
using HotelBooking.Application.Users.Queries.GetUserByEmail;
using HotelBooking.Application.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /api/users/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var query = new GetUserByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET /api/users?email={email}
        [HttpGet]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var query = new GetUserByEmailQuery(email);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // POST /api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUserById), new { id = result.UserId }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand request)
        {
            var command = new UpdateUserCommand(id, request.Email, request.FirstName, request.LastName);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // DELETE /api/users/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var command = new DeleteUserCommand(id);
            await _mediator.Send(command);  
            return NoContent(); 
        }
    }
}