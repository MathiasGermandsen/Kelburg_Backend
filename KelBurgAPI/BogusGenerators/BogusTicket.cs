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
            .RuleFor(t => t.Stars, f => f.Random.Number(1, 5));


        return faker.Generate(count);
    }
}