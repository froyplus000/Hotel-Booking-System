using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Users.Queries.GetUserById
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {query.UserId} was not found.");
            }
            return new UserDto(
                UserId: user.Id,
                Email: user.Email,
                FirstName: user.FirstName,
                LastName: user.LastName
            );
        }
    }
}