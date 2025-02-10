using Bogus;
using KelBurgAPI.Models;
namespace KelBurgAPI.BogusGenerators;

public class BogusTicket
{
    public static List<TicketCreateDTO> GenerateRooms(int count, List<int> ValidUserIds)
    {
        Faker<TicketCreateDTO> faker = new Faker<TicketCreateDTO>()
            .RuleFor(t => t.FromUser, f => f.PickRandom(ValidUserIds))
            .RuleFor(t => t.Description, f => f.Rant.Review())
            .RuleFor(t => t.Category, f => f.PickRandom(new[] { "Housekeeping", "Maintenance", "IT Support", "Room Service", "Support Services" }))
            .RuleFor(t => t.Status, f => f.PickRandom(new[] { "Open", "In Progress", "Closed" }));

        return faker.Generate(count);
    }
}