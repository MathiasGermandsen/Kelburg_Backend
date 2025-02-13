using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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