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
        int servicePricePrPersonPrNight = services[currentBooking.ServiceId-1].PricePrPersonPrNight;
        int vacationDays = (currentBooking.EndDate - currentBooking.StartDate).Days;
        
        bookingPrice += (servicePricePrPersonPrNight*vacationDays)*currentBooking.PeopleCount;
        bookingPrice += (selectedRoom.PricePrNight*vacationDays);
        
        return bookingPrice;
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