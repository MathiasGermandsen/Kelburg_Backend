namespace KelBurgAPI.Models;

public class Booking : Common
{
    public int PeopleCount { get; set; }
    public int BookingPrice { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Breakfast { get; set; }
    public bool AllInclusive { get; set; }

    public int CalculateBookingPrice(Booking currentBooking, Rooms selectedRoom, List<ServicePricesDict> priceDict)
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
        
        int vacationDays = (currentBooking.EndDate - currentBooking.StartDate).Days;
        
        bookingPrice += (selectedRoom.PricePrNight * vacationDays);

        if (currentBooking.Breakfast)
        {
            bookingPrice += (breakfastPricePrNightPrPerson * currentBooking.PeopleCount) * vacationDays;
        }

        if (currentBooking.AllInclusive)
        {
            bookingPrice += (allInclusivePricePrNightPrPerson*currentBooking.PeopleCount) * vacationDays;
        }
        
        return bookingPrice;
    }
}

public class BookingCreateDTO
{
    public int PeopleCount { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today;
    public bool Breakfast { get; set; }
    public bool AllInclusive { get; set; }
}