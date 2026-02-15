using MediatR;

namespace HotelBooking.Application.Users.Commands.UpdateUser
{
    public record UpdateUserCommand(
        Guid UserId,
        string Email,
        string FirstName,
        string LastName
    ) : IRequest<UserDto>;
}