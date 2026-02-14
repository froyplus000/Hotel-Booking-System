using HotelBooking.Application.Users.Queries.GetUserById;
using MediatR;

namespace HotelBooking.Application.Users.Commands.CreateUser
{
    public record CreateUserCommand(string Email, string FirstName, string LastName) : IRequest<UserDto>;
}