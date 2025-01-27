namespace KelBurgAPI.Models;

public class ServicePricesDict : Common
{
    public string Type { get; set; }
    public int PricePrPersonPrNight { get; set; }
}

public class ServicePricesDictCreateDTO
{
    public string Type { get; set; }
    public int PricePrPersonPrNight { get; set; }
}