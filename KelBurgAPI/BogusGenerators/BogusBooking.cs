using Bogus;
using KelBurgAPI.Models;

namespace KelBurgAPI.BogusGenerators;

public class BogusBooking
{
    private static readonly Dictionary<string, int> RoomOccupancyLimits = new()
    {
        { "Single room", 1 },
        { "Double room", 2 },
        { "Twin room", 2 },
        { "Hollywood Twin", 2 },
        { "Queen room", 2 },
        { "Triple room", 3 },
        { "Quad room", 4 },
        { "Suite", 5 },
        { "Deluxe Room", 6 },
        { "Presidential Suites", 12 }
    };

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
            .RuleFor(b => b.RoomId, f =>
            {
                if (RoomsToUse.Count == 0)
                {
                    throw new InvalidOperationException("No more available rooms to assign.");
                }

                Rooms room = f.PickRandom(RoomsToUse);
                RoomsToUse.Remove(room);
                return room.Id;
            })
            .RuleFor(b => b.PeopleCount, (f, b) =>
            {
                Rooms? assignedRoom = availableRooms.FirstOrDefault(r => r.Id == b.RoomId);
                if (assignedRoom == null)
                {
                    return 1; 
                }

                int maxOccupancy = RoomOccupancyLimits.ContainsKey(assignedRoom.RoomType) ? RoomOccupancyLimits[assignedRoom.RoomType] : 12;
                return f.Random.Number(1, maxOccupancy);
            })
            .RuleFor(b => b.ServiceId, f => f.Random.Number(1, 4));

        return faker.Generate(count);
    }
}
