namespace KelBurgAPI.Models;

public class Rooms : Common
{
    public int Size { get; set; }
    public string RoomType { get; set; }
    public string ViewType { get; set; }
    public int PricePrNight { get; set; }

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