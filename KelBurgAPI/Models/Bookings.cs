namespace KelBurgAPI.Models;

public class Bookings : Common
{
    public int PeopleCount { get; set; }
    public int BookingPrice { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Breakfast { get; set; }
    public bool AllInclusive { get; set; }

    public int CalculateBookingPrice(Bookings currentBookings, Rooms selectedRoom, List<ServicePricesDict> priceDict)
    {
        int bookingPrice = 0;
        int breakfastPricePrNightPrPerson = 0;
        int allInclusivePricePrNightPrPerson = 0;
        
        foreach (ServicePricesDict servicePrice in priceDict)
        {
            switch (servicePrice.Type)
            {
                case "breakfast":
                    breakfastPricePrNightPrPerson = servicePrice.PricePrPersonPrNight;
                    break;
                case "allinclusive":
                    allInclusivePricePrNightPrPerson = servicePrice.PricePrPersonPrNight;
                    break;
                default:
                    break;
            }
        }
        
        int vacationDays = (currentBookings.EndDate - currentBookings.StartDate).Days;
        
        bookingPrice += (selectedRoom.PricePrNight * vacationDays);

        if (currentBookings.Breakfast)
        {
            bookingPrice += (breakfastPricePrNightPrPerson * currentBookings.PeopleCount) * vacationDays;
        }

        if (currentBookings.AllInclusive)
        {
            bookingPrice += (allInclusivePricePrNightPrPerson*currentBookings.PeopleCount) * vacationDays;
        }
        
        return bookingPrice;
    }
}

public class BookingCreateDTO
{
    public int PeopleCount { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today.Date;
    public DateTime EndDate { get; set; } = DateTime.Today.Date;
    public bool Breakfast { get; set; }
    public bool AllInclusive { get; set; }
}