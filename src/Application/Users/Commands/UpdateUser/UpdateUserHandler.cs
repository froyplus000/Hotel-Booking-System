using HotelBooking.Domain.Repositories;
using MediatR;

namespace HotelBooking.Application.Users.Commands.UpdateUser
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {command.UserId} was not found.");
            }
            user.Update(command.Email, command.FirstName, command.LastName);
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            return new UserDto(
                UserId: user.Id,
                Email: user.Email,
                FirstName: user.FirstName,
                LastName: user.LastName
            );
        }
    }
}