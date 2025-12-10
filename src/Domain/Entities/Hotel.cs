using System;

namespace HotelBooking.Domain.Entities
{
    public class Hotel
    {
        public Guid Id { get; private set; }
        public string Name { get; private set;}
        public string Address { get; private set;}
        public string City { get; private set;}
        public string Country { get; private set;}

        public List<RoomType> RoomTypes { get; private set;}

        public Hotel( string name, string address, string city, string country)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException(nameof(address));
            if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException(nameof(city));
            if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException(nameof(country));

            Id = Guid.NewGuid();
            Name = name;
            Address = address;
            City = city;
            Country = country;
            RoomTypes = new List<RoomType>();
        }
    }
}