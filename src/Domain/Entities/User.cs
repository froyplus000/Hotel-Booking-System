using System;

namespace HotelBooking.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set;}
        public string Email { get; private set;}
        public string FirstName { get; private set;}
        public string LastName { get; private set;}

        public List<Booking> Bookings{ get; private set; }
        public User(string email, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(nameof(email));
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException(nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException(nameof(lastName));
            Id = Guid.NewGuid();
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Bookings = new List<Booking>();
        }

        public void Update(string email, string firstName, string lastName)
        {
            // You can add validation here if needed
            if (string.IsNullOrWhiteSpace(email)) 
                throw new ArgumentException(nameof(email));
            if (string.IsNullOrWhiteSpace(firstName)) 
                throw new ArgumentException(nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) 
                throw new ArgumentException(nameof(lastName));
            
            Email = email;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}