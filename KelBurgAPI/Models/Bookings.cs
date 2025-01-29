namespace KelBurgAPI.Models;

public class Bookings : Common
{
    public int UserId { get; set; }
    public int PeopleCount { get; set; }
    public int BookingPrice { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ServiceId { get; set; }

    public int CalculateBookingPrice(Bookings currentBooking, Rooms selectedRoom, List<Services> services)
    {
        int bookingPrice = 0;
        int vacationDays = (currentBooking.EndDate - currentBooking.StartDate).Days;
        
        int totalServicePrices = (services[currentBooking.ServiceId-1].PricePrPersonPrNight*currentBooking.PeopleCount) * vacationDays;
        int totalRoomPrice = selectedRoom.PricePrNight * vacationDays;

        bookingPrice += totalServicePrices;
        bookingPrice += totalRoomPrice;
        
        return bookingPrice;
    }

    public bool CheckBookingOverlap(Bookings booking1, Bookings booking2)
    {
        return booking1.StartDate <= booking2.EndDate && booking2.StartDate <= booking1.EndDate;
    }
}

public class BookingCreateDTO
{
    public int UserId { get; set; }
    public int PeopleCount { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today.Date;
    public DateTime EndDate { get; set; } = DateTime.Today.Date;
    public int ServiceId { get; set; }
}