namespace KelBurgAPI.Models;

public class HotelCars : Common
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string Vin { get; set; }
    public int Size { get; set; }
    public string Type { get; set; }
    public string Fuel { get; set; }
    public int PricePrNight { get; set; }
}

public class HotelCarsDTO
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string Vin { get; set; }
    public int Size { get; set; }
    public string Type { get; set; }
    public string Fuel { get; set; }
    public int PricePrNight { get; set; }
}