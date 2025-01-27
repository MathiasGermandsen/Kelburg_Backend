namespace KelBurgAPI.Models;

public class Tickets : Common
{
    public int FromUser { get; set; }
    public string Description { get; set; }
    public int Stars { get; set; }
}

public class TicketCreateDTO
{
    public int FromUser { get; set; }
    public string Description { get; set; }
    public int Stars { get; set; }
}