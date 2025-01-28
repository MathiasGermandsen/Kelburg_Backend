using KelBurgAPI.Models;
using Bogus;

namespace KelBurgAPI.BogusGenerators;

public class BogusRooms
{
    public static List<RoomCreateDTO> GenerateRooms(int count)
    {
        Faker<RoomCreateDTO> faker = new Faker<RoomCreateDTO>()
            .RuleFor(r => r.Size, f => f.Random.Number(1, 12))
            .RuleFor(r => r.RoomType, f => f.PickRandom(new[]
            {
                "Single room",
                "Quad room",
                "Hollywood Twin",
                "Cabana",
                "Double room",
                "Queen room",
                "Twin room",
                "Suite",
                "Presidential Suites",
                "Triple room",
                "King room",
                "Double hotel rooms",
                "Standard hotel rooms",
                "Deluxe Room"
            }))
            .RuleFor(r => r.ViewType, f => f.PickRandom(new[]{"Ocean view",
                "Sea view",
                "Mountain view",
                "City view",
                "Garden view",
                "Pool view",
                "Lake view",
                "River view",
                "Beachfront view",
                "Park view",
                "Skyline view",
                "Courtyard view",
                "Marina view",
                "Forest view",
                "Golf course view",
                "Desert view"}))
            .RuleFor(r => r.PricePrNight, f => f.Random.Number(3215, 7560));
        return faker.Generate(count);
    }
}