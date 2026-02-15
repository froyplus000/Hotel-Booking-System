using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Users.Queries.GetUserByEmail
{
    public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByEmailHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(query.email, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {query.email} was not found.");
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