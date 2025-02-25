namespace KelBurgAPI.Models;

public class Services : Common
{
    public bool AllInclusive { get; set; }
    public bool Breakfast { get; set; }
    public bool Dinner { get; set; }
    public bool BreakfastAndDinner { get; set; }
    public int PricePrPersonPrNight { get; set; }
}

public class ServiceCreateDTO
{
    public bool AllInclusive { get; set; }
    public bool Breakfast { get; set; }
    public bool Dinner { get; set; }
    public bool BreakfastAndDinner { get; set; }
    public int PricePrPersonPrNight { get; set; }
}


public class ServicePaymentDTO
{
    public bool AllInclusive { get; set; }
    public bool Breakfast { get; set; }
    public bool Dinner { get; set; }
    public bool BreakfastAndDinner { get; set; }
    public int PricePrPersonPrNight { get; set; }
    public string PrettyName { get; set; }
}