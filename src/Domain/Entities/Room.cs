using System;

namespace HotelBooking.Domain.Entities
{
    public class Room
    {
        public Guid Id { get; private set; }
        public Guid RoomTypeId { get; private set; }
        public int RoomNumber { get; private set; }
        
        // This is a "Navigation Property"
        // It allows EF Core to link this Room to the full RoomType object.
        // We make it nullable (?) because sometimes we might load a Room without loading its type details.
        public RoomType? RoomType { get; private set; } 

        public Room(Guid roomTypeId, int roomNumber)
        {
            if (roomNumber <= 0)
                throw new ArgumentException("Room number must be positive");
            
            if (roomTypeId == Guid.Empty)
                throw new ArgumentException("Room must belong to a valid RoomType");

            Id = Guid.NewGuid(); // Generate ID here!
            RoomTypeId = roomTypeId;
            RoomNumber = roomNumber;
        }
    }
}