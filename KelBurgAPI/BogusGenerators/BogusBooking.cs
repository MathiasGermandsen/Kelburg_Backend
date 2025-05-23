using Bogus;
using KelBurgAPI.Models;

namespace KelBurgAPI.BogusGenerators
{
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


        public static List<BookingCreateDTO> GenerateBookings(int count, List<int> validUserIds, List<Rooms> allRooms, List<Bookings> allExistingBookings, List<HotelCars> allCars, bool withCar, int carId)
        {
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

                    (startDate, endDate) = FindNextAvailableDates(selectedRoom, startDate, numberOfDays, allExistingBookings);

                    BookingCreateDTO newBooking = new BookingCreateDTO
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        RoomId = selectedRoom.Id
                    };

                    //for loop to make sure that some of the bookings has a car
                    if (withCar)
                    {
                        HotelCars selectedCar = allCars.Find(c => c.Id == carId);
                        
                        (startDate, endDate) = FindNextAvailableCar(selectedCar, startDate, numberOfDays, allExistingBookings);

                        newBooking.CarId = selectedCar.Id;
                    }

                    return newBooking;
                })
                .RuleFor(b => b.UserId, f => f.PickRandom(validUserIds))
                .RuleFor(b => b.ServiceId, f => f.Random.Number(1, 4))
                .RuleFor(b => b.PeopleCount, (f, b) =>
                {
                    Rooms? assignedRoom = allRooms.FirstOrDefault(r => r.Id == b.RoomId);

                    if (assignedRoom == null)
                    {
                        return 1;
                    }

                    int maxOccupancy = RoomOccupancyLimits.ContainsKey(assignedRoom.RoomType)
                        ? RoomOccupancyLimits[assignedRoom.RoomType]
                        : 12;
                    return f.Random.Number(1, assignedRoom.Size);
                });

            return faker.Generate(count);
        }


        //this is where we are making sure there is no overlapping in the booking of a car
        private static (DateTime startDate, DateTime endDate) FindNextAvailableCar(HotelCars selectedCar, DateTime startDate, int numberOfDays, List<Bookings> allBookings)
        {
            DateTime highestEndDate = new DateTime();
            while (true)
            {
                Bookings? overlappingBooking = allBookings
                    .Where(b => b.CarId != null && b.CarId == selectedCar.Id) //checking only for when the car is booked
                    .Where(b => b.CheckBookingOverlap(b, new Bookings
                    {
                        CarId = selectedCar.Id, 
                        StartDate = startDate,
                        EndDate = startDate.AddDays(numberOfDays)
                    }))
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

        private static (DateTime startDate, DateTime endDate) FindNextAvailableDates(Rooms selectedRoom, DateTime startDate, int numberOfDays, List<Bookings> allBookings)
        {
            DateTime highestEndDate = new DateTime();

            while (true)
            {
                Bookings? overlappingBooking = allBookings
                    .Where(b => b.RoomId == selectedRoom.Id && b.CheckBookingOverlap(b,
                        new Bookings
                        {
                            RoomId = selectedRoom.Id, StartDate = startDate,
                            EndDate = startDate.AddDays(numberOfDays)
                        }))
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