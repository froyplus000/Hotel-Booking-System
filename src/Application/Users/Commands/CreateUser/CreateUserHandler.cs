using HotelBooking.Application.Users.Queries.GetUserById;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Users.Commands.CreateUser
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            var user = new User(
                command.Email,
                command.FirstName,
                command.LastName
            );
            var savedUser = await _userRepository.AddAsync(user, cancellationToken);
            return new UserDto(
                UserId: savedUser.Id,
                Email: savedUser.Email,
                FirstName: savedUser.FirstName,
                LastName: savedUser.LastName
            );
        }
    }
}