namespace KelBurgAPI.Models;

public class Rooms : Common
{
    public int Size { get; set; }
    public string RoomType { get; set; }
    public string ViewType { get; set; }
    public int PricePrNight { get; set; }
}

public class RoomCreateDTO
{
    public int Size { get; set; }
    public string RoomType { get; set; }
    public string ViewType { get; set; }
    public int PricePrNight { get; set; }  
}