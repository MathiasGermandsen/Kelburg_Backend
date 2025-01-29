namespace KelBurgAPI.Models;

public class Tickets : Common
{
    public int FromUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(+1);
    
    public string Category { get; set; } 
    
    public string Description { get; set; }
    
    public string Status { get; set; }
    
}

public class TicketCreateDTO
{
    public int FromUser { get; set; }
    public string Description { get; set; }
    
    public string Category { get; set; } 

    public string Status { get; set; }
}