using MediatR;

namespace HotelBooking.Application.Users.Queries.GetUserByEmail
{
    public record GetUserByEmailQuery(string email) : IRequest<UserDto>;
}