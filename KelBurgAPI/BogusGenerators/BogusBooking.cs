using Bogus;
using KelBurgAPI.Models;

namespace KelBurgAPI.BogusGenerators;

public class BogusBooking
{
    public static List<BookingCreateDTO> GenerateBookings(int count, List<int> validUserIds, List<Rooms> availableRooms)
    {
        List<Rooms> RoomsToUse = new List<Rooms>(availableRooms);

        Faker<BookingCreateDTO> faker = new Faker<BookingCreateDTO>()
            .CustomInstantiator(f =>
            {
                DateTime startDate = f.Date.Soon().ToUniversalTime();
                
                int numberOfDays = f.Random.Number(3, 14);
                DateTime endDate = startDate.AddDays(numberOfDays).ToUniversalTime();

                if (endDate <= startDate)
                {
                    endDate = startDate.AddDays(1).ToUniversalTime();
                }

                return new BookingCreateDTO
                {
                    StartDate = startDate,
                    EndDate = endDate
                };
            })
            .RuleFor(b => b.UserId, f => f.PickRandom(validUserIds))
            .RuleFor(b => b.PeopleCount, f => f.Random.Number(1, 12))
            .RuleFor(b => b.ServiceId, f => f.Random.Number(1, 4))
            .RuleFor(b => b.RoomId, f =>
            {
                if (RoomsToUse.Count == 0)
                {
                    throw new InvalidOperationException("No more available rooms to assign.");
                }

                Rooms room = f.PickRandom(RoomsToUse);
                RoomsToUse.Remove(room);
                return room.Id;
            });

        return faker.Generate(count);
    }
}