using MediatR;

namespace HotelBooking.Application.Users.Queries.GetUserById
{
    public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;
}