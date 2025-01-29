using Bogus;
using KelBurgAPI.Models;

namespace KelBurgAPI.BogusGenerators;

public class BogusBooking
{
    public static List<BookingCreateDTO> GenerateBookings(int count, List<int> validUserIds, List<Rooms> availableRooms, Dictionary<int, DateTime> latestEndDates)
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
        Dictionary<int, List<BookingCreateDTO>> roomBookings = new Dictionary<int, List<BookingCreateDTO>>();

        Faker<BookingCreateDTO> faker = new Faker<BookingCreateDTO>()
            .CustomInstantiator(f =>
            {
                Rooms selectedRoom = null;
                DateTime startDate = default;
                DateTime endDate = default;
                
                foreach (Rooms room in availableRooms)
                DateTime startDate = f.Date.Soon().ToUniversalTime();
                int numberOfDays = f.Random.Number(3, 14);
                DateTime endDate = startDate.AddDays(numberOfDays).ToUniversalTime();

                if (endDate <= startDate)
                {
                    if (!roomBookings.ContainsKey(room.Id))
                    {
                        roomBookings[room.Id] = new List<BookingCreateDTO>();
                    }

                    if (latestEndDates.ContainsKey(room.Id))
                    {
                        startDate = latestEndDates[room.Id].AddHours(3);
                    }
                    else
                    {
                        startDate = f.Date.Soon().ToUniversalTime();
                    }

                    int numberOfDays = f.Random.Number(1, 14);
                    endDate = startDate.AddDays(numberOfDays).ToUniversalTime();
                    
                    bool hasOverlap = true;

                    while (hasOverlap)
                    {
                        hasOverlap = roomBookings[room.Id].Any(existingBooking =>
                            startDate < existingBooking.EndDate && endDate > existingBooking.StartDate);

                        if (hasOverlap)
                        {
                            startDate = startDate.AddHours(3);
                            endDate = startDate.AddDays(numberOfDays).ToUniversalTime();
                        }
                    }

                    selectedRoom = room;
                    roomBookings[room.Id].Add(new BookingCreateDTO { StartDate = startDate, EndDate = endDate });
                    break;
                }

                if (selectedRoom == null)
                {
                    throw new InvalidOperationException($"No available rooms were selected {string.Join(", ", roomBookings.Keys)}.");
                }

                return new BookingCreateDTO
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    RoomId = selectedRoom.Id
                };
            })
            .RuleFor(b => b.UserId, f => f.PickRandom(validUserIds))
            .RuleFor(b => b.RoomId, f =>
            {
                if (availableRooms.Count == 0)
                {
                    throw new InvalidOperationException("No available rooms to assign.");
                }

                Rooms room = f.PickRandom(availableRooms);
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
