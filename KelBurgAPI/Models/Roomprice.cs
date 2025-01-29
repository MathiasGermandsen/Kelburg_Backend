namespace KelBurgAPI.Models;

public class HotelPricing
{
    public Dictionary<string, int> BaseRoomPrices { get; set; }
    public Dictionary<string, int> ViewTypePrices { get; set; }
    public int PerOccupantCharge { get; set; }
}