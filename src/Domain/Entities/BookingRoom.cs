using System;
namespace HotelBooking.Domain.Entities
{
    public class BookingRoom
    {
        public Guid RoomId { get; private set; }
        public decimal PriceAtBooking { get; private set; }

        public Room? Room{ get; private set; }

        public BookingRoom(Guid roomId, decimal priceAtBooking)
        {
            if (priceAtBooking < 0) throw new ArgumentException("Price cannot be negative");

            RoomId = roomId;
            PriceAtBooking = priceAtBooking;
        }
    }
}