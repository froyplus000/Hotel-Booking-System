namespace HotelBooking.Application.Users
{
    public record UserDto(
        Guid UserId,
        string Email,
        string FirstName,
        string LastName
    );
}