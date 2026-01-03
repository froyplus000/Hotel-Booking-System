using System;
namespace HotelBooking.Domain.Entities
{
    public class BookingRoom
    {
        public Guid BookingId { get; private set; }
        public Guid RoomId { get; private set; }
        public decimal PriceAtBooking { get; private set; }
        public Room? Room{ get; private set; }
        public BookingRoom(Guid bookingId, Guid roomId, decimal priceAtBooking)
        {
            if (priceAtBooking < 0) throw new ArgumentException("Price cannot be negative");
            BookingId = bookingId;
            RoomId = roomId;
            PriceAtBooking = priceAtBooking;
        }
    }
}