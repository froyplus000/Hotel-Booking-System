using System;
using System.Collections.Generic; // <--- Don't forget this!

namespace HotelBooking.Domain.Entities
{
    public class RoomType
    {
        public Guid Id { get; private set; }
        public Guid HotelId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; } // Added per requirements
        public decimal BasePrice { get; private set; }
        public string Currency { get; private set; } // Added per requirements
        public int MaxGuests { get; private set; } // Renamed to Plural "Guests"

        // Navigation Properties
        public Hotel? Hotel { get; private set; } // Link to Parent
        public List<Room> Rooms { get; private set; } // Link to Children

        public RoomType(Guid hotelId, string name, string description, decimal basePrice, int maxGuests, string currency = "USD")
        {
            // Guard Clauses: "Fail Fast"
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (basePrice < 0) throw new ArgumentException("Price cannot be negative");
            if (maxGuests <= 0) throw new ArgumentException("Must accommodate at least one guest");

            Id = Guid.NewGuid();
            HotelId = hotelId;
            Name = name;
            Description = description;
            BasePrice = basePrice;
            MaxGuests = maxGuests;
            Currency = currency;
            Rooms = new List<Room>();
        }
    }
}