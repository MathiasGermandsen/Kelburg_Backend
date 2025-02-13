using Bogus;
using KelBurgAPI.Models;

namespace KelBurgAPI.BogusGenerators;

public class BogusHotelCars
{
    private static readonly Dictionary<string, int> CarTypeSize = new()
    {
        { "Convertible", 2 },
        { "Super Car", 2 },
        { "Sedan", 5 },
        { "Station Wagon", 5 },
        { "SUV", 5 },
        { "American Muscle", 4 },
        { "Minivan", 7 },
        { "Minivan XL", 10 },
        { "Coupé", 2 },
        { "ATV", 2 }
    };

    public static List<HotelCarsDTO> GenCars(int count)
    {
        Faker<HotelCarsDTO> faker = new Faker<HotelCarsDTO>()
            .RuleFor(h => h.Vin, f => f.Vehicle.Vin())
            .RuleFor(h => h.Manufacturer, f => f.Vehicle.Manufacturer())
            .RuleFor(h => h.Model, f => f.Vehicle.Model())
            .RuleFor(h => h.Fuel, f => f.Vehicle.Fuel())
            .RuleFor(h => h.Type, f => f.PickRandom(CarTypeSize.Keys.ToList()))
            .RuleFor(h => h.Size, (f, h) => CarTypeSize[h.Type])
            .RuleFor(h => h.PricePrNight, (f, h) => f.Random.Int(50, 110)*h.Size);

        return faker.Generate(count);
    }
}