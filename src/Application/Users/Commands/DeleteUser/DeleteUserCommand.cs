using MediatR;
namespace HotelBooking.Application.Users.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid UserId) : IRequest;
}