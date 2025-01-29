using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KelBurgAPI.Models;

public class Rooms : Common
{
    public int Size { get; set; }
    public string RoomType { get; set; }
    public string ViewType { get; set; }
    public int PricePrNight { get; set; }


    public List<Rooms> GetAvailableRooms(List<Bookings> allExistingBookings, List<Rooms> allRooms)
    {
        DateTime today = DateTime.Now.Date;
        List<Rooms> roomsInUse = new List<Rooms>();
        List<Rooms> roomsAvailable = new List<Rooms>();

        if (allExistingBookings.Any())
        {
            foreach (Bookings existingBooking in allExistingBookings)
            {
                foreach (Rooms room in allRooms)
                {
                    if (existingBooking.RoomId == room.Id && existingBooking.EndDate.Date == today.Date && !roomsInUse.Contains(room))
                    {
                        roomsInUse.Add(room);

                        if (roomsAvailable.Contains(room))
                        {
                            roomsAvailable.Remove(room);
                        }
                        else if (!roomsAvailable.Contains(room) && !roomsInUse.Contains(room))
                        {
                            roomsAvailable.Add(room);
                        }
                    }
                }
            }
        }
        else
        {
            roomsAvailable = allRooms;
        }
        return roomsAvailable;
    }

    public bool IsRoomAvailableAtDate(List<Bookings> allExistingBookings, Rooms selectedRoom, Bookings bookingToBeCreated)
    {
        List<Bookings> bookingsUsingRoom = allExistingBookings.Where(x => x.RoomId == selectedRoom.Id).ToList();

        foreach (Bookings booking in bookingsUsingRoom)
        {
            if (booking.CheckBookingOverlap(booking, bookingToBeCreated))
            {
                return false;
            }
        }
        var latestBooking = bookingsUsingRoom.OrderByDescending(b => b.EndDate).FirstOrDefault();
        if (latestBooking != null)
        {
            DateTime latestEndDatePlus3Hours = latestBooking.EndDate.AddHours(3);
            if (bookingToBeCreated.StartDate < latestEndDatePlus3Hours)
            {
                return false;
            }
        }

        return true;
    }
}

public class RoomCreateDTO
{
    public int Size { get; set; }
    public string RoomType { get; set; }
    public string ViewType { get; set; }
    public int PricePrNight { get; set; }  
}