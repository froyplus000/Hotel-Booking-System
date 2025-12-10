using System;
using System.Collections.Generic;
using System.Linq; // Required for .Sum()

namespace HotelBooking.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime CheckInDate { get; private set; }
        public DateTime CheckOutDate { get; private set; }
        public int NumberOfNights { get; private set; }
        public decimal TotalPrice { get; private set; }
        public BookingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public User? User { get; private set;}
        // Encapsulation: Use a private list for backing field
        private readonly List<BookingRoom> _rooms = new();
        // Publicly expose ONLY a read-only view. 
        // This prevents 'booking.Rooms.Add()' from the outside.
        public IReadOnlyCollection<BookingRoom> Rooms => _rooms.AsReadOnly();

        private Booking() { }

        // Removed 'roomId' and 'price' from constructor. 
        // Create the booking first, THEN add rooms.
        public Booking(Guid userId, DateTime checkIn, DateTime checkOut)
        {
            if (checkIn >= checkOut)
                throw new ArgumentException("Check-out must be after check-in");

            Id = Guid.NewGuid();
            UserId = userId;
            CheckInDate = checkIn;
            CheckOutDate = checkOut;
            NumberOfNights = (checkOut - checkIn).Days; // Fixed Math
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            TotalPrice = 0;
        }

        public void AddRoom(Room room, decimal priceAtBooking)
        {
             // Validate
            if (_rooms.Any(r => r.RoomId == room.Id))
                throw new InvalidOperationException("Room already added");

            var bookingRoom = new BookingRoom(Id, room.Id, priceAtBooking);
            _rooms.Add(bookingRoom);
            
            CalculateTotal(); // Re-calculate price
        }

        private void CalculateTotal()
        {
            // LINQ: Sum of all rooms * Nights
            TotalPrice = _rooms.Sum(r => r.PriceAtBooking) * NumberOfNights;
        }

        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
        }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }
}