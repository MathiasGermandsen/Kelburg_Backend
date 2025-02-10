using Bogus;
using KelBurgAPI.Models;

namespace KelBurgAPI.BogusGenerators
{
    public class BogusBooking
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
        public static List<BookingCreateDTO> GenerateBookings(int count, List<int> validUserIds, List<Rooms> allRooms, List<Bookings> allExistingBookings)
        {
            List<BookingCreateDTO> newlyCreatedBookings = new List<BookingCreateDTO>();
            Faker<BookingCreateDTO> faker = new Faker<BookingCreateDTO>()
                .CustomInstantiator(f =>
                {
                    Rooms selectedRoom = f.PickRandom(allRooms);
                    DateTime startDate = f.Date.Soon().ToUniversalTime();
                    int numberOfDays = f.Random.Number(3, 14);
                    DateTime endDate = startDate.AddDays(numberOfDays).ToUniversalTime();

                    if (endDate <= startDate)
                    {
                        endDate = startDate.AddDays(1).ToUniversalTime();
                    }

                    List<Bookings> allBookingsIncludingNew = new List<Bookings>(allExistingBookings);
                    allBookingsIncludingNew.AddRange(newlyCreatedBookings.Select(b => new Bookings 
                    { 
                        RoomId = b.RoomId, 
                        StartDate = b.StartDate, 
                        EndDate = b.EndDate 
                    }));

                    (startDate, endDate) = FindNextAvailableDates(selectedRoom, startDate, numberOfDays, allBookingsIncludingNew);

                    BookingCreateDTO newBooking = new BookingCreateDTO
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        RoomId = selectedRoom.Id
                    };

                    newlyCreatedBookings.Add(newBooking);
                    return newBooking;
                })
                .RuleFor(b => b.UserId, f => f.PickRandom(validUserIds))
                .RuleFor(b => b.PeopleCount, f => f.Random.Number(1, 12))
                .RuleFor(b => b.ServiceId, f => f.Random.Number(1, 4));

            return faker.Generate(count);
        }

        private static (DateTime startDate, DateTime endDate) FindNextAvailableDates(Rooms selectedRoom, DateTime startDate, int numberOfDays, List<Bookings> allBookings)
        {
            DateTime highestEndDate = new DateTime();
            
            while (true)
            {
                Bookings? overlappingBooking = allBookings
                    .Where(b => b.RoomId == selectedRoom.Id && b.CheckBookingOverlap(b, new Bookings { RoomId = selectedRoom.Id, StartDate = startDate, EndDate = startDate.AddDays(numberOfDays) }))
                    .OrderBy(b => b.EndDate)
                    .FirstOrDefault();

                if (overlappingBooking == null)
                {
                    break;
                }

                Random random = new Random();
                
                if (highestEndDate < overlappingBooking.EndDate)
                {
                    highestEndDate = overlappingBooking.EndDate;
                }
                
                startDate = highestEndDate.AddHours(random.Next(3, 24));
            }
            return (startDate, startDate.AddDays(numberOfDays));
        }
    }
}
