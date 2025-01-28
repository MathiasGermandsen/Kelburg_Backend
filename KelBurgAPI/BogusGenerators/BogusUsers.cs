using KelBurgAPI.Models;
using Bogus;


namespace KelBurgAPI.BogusGenerators;

public class BogusUsers
{
    public static List<UserCreateDTO> GenerateUsers(int count)
    {
        Faker<UserCreateDTO> faker = new Faker<UserCreateDTO>()
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Password, f => f.Internet.Password())
            .RuleFor(u => u.Address, f => f.Address.StreetAddress())
            .RuleFor(u => u.City, f => f.Address.City())
            .RuleFor(u => u.PostalCode, f => f.Address.ZipCode())
            .RuleFor(u => u.Country, f => f.Address.Country())
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.AccountType, f => f.PickRandom(new[] { "user" }));

        return faker.Generate(count);
    }
}



