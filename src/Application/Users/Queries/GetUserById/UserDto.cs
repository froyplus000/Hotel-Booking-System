namespace HotelBooking.Application.Users.Queries.GetUserById
{
    public record UserDto(
        Guid UserId,
        string Email,
        string FirstName,
        string LastName
    );
}