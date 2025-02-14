using KelBurgAPI.BogusGenerators;
using Microsoft.AspNetCore.Mvc;
using KelBurgAPI.Models;
using KelBurgAPI.Data;

namespace KelBurgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BogusController : ControllerBase
{
    private readonly DatabaseContext _context;

    public BogusController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("GenUsers")]
    public async Task<ActionResult<List<UserCreateDTO>>> GenUsers(int count = 10)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }

        List<UserCreateDTO> usersGenerated = KelBurgAPI.BogusGenerators.BogusUsers.GenerateUsers(count);
        List<Users> usersMapped = new List<Users>();

        foreach (UserCreateDTO user in usersGenerated)
        {
            Users userMapped = new Users()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password),
                PasswordBackdoor = user.Password,
                Address = user.Address,
                City = user.City,
                PostalCode = user.PostalCode,
                Country = user.Country,
                PhoneNumber = user.PhoneNumber,
                AccountType = user.AccountType
            };
            usersMapped.Add(userMapped);
        }

        _context.Users.AddRange(usersMapped);
        await _context.SaveChangesAsync();
        return Ok(usersMapped);
    }

    [HttpPost("GenRooms")]
    public async Task<ActionResult<List<RoomCreateDTO>>> GenRooms(int count = 10)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }

        HotelPricing pricing;
        try
        {
            pricing = KelBurgAPI.BogusGenerators.BogusRooms.LoadHotelPricing("RoomPricing.json");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error loading pricing data: {ex.Message}");
        }

        List<RoomCreateDTO> roomsGenerated = KelBurgAPI.BogusGenerators.BogusRooms.GenerateRooms(count, pricing);
        List<Rooms> roomsMapped = new List<Rooms>();

        foreach (RoomCreateDTO room in roomsGenerated)
        {
            Rooms roomMapped = new Rooms()
            {
                Size = room.Size,
                RoomType = room.RoomType,
                ViewType = room.ViewType,
                PricePrNight = room.PricePrNight,
            };
            roomsMapped.Add(roomMapped);
        }

        _context.Rooms.AddRange(roomsMapped);
        await _context.SaveChangesAsync();

        return Ok(roomsMapped);
    }

    [HttpPost("GenBookings")]
    public async Task<ActionResult<List<BookingCreateDTO>>> GenBookings(int MaxCount = 10)
    {
        if (MaxCount <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }

        List<int> validUserIdList = _context.Users.Select(u => u.Id).ToList();
        List<Services> servicePrices = _context.Services.ToList();
        List<Rooms> allRooms = _context.Rooms.ToList();

        if (!validUserIdList.Any())
        {
            return BadRequest("No Users in Database!");
        }

        if (!allRooms.Any())
        {
            return BadRequest("No Rooms in Database!");
        }
        
        if (!servicePrices.Any())
        {
            return BadRequest("No Service-prices found. Cannot make booking.");
        }

        List<Bookings> allExistingBookings = _context.Booking.ToList();
        List<HotelCars> allCars = _context.HotelCars.ToList();
        
        List<BookingCreateDTO> bookingsGenerated = new List<BookingCreateDTO>();

        for (int i = 1; i <= MaxCount; i++)
        {
            Random rand = new Random();
            bool withCar = rand.Next(0, 3) == 2 ? true : false;

            int carId = allCars.Select(c => c.Id).OrderBy(_ => rand.Next()).First();

            List<BookingCreateDTO> bookingGenerated =
                KelBurgAPI.BogusGenerators.BogusBooking.GenerateBookings(1, validUserIdList, allRooms, allExistingBookings, allCars, withCar, carId);
            
            Bookings bookingGeneratedMapped = new Bookings()
            {
                UserId = bookingGenerated[0].UserId,
                PeopleCount = bookingGenerated[0].PeopleCount,
                BookingPrice = 0,
                RoomId = bookingGenerated[0].RoomId,
                StartDate = bookingGenerated[0].StartDate,
                EndDate = bookingGenerated[0].EndDate,
                ServiceId = bookingGenerated[0].ServiceId,
                CarId = bookingGenerated[0].CarId,
            };
            
            bookingsGenerated.AddRange(bookingGenerated);
            allExistingBookings.Add(bookingGeneratedMapped);
        }
        
        List<Bookings> bookingsMapped = new List<Bookings>();

        foreach (BookingCreateDTO booking in bookingsGenerated)
        {
            Rooms? selectedRoom = allRooms.Find(r => r.Id == booking.RoomId);
            HotelCars? selectedCar = allCars.Find(c => c.Id == booking.CarId);
            if (selectedRoom == null)
            {
                return BadRequest($"Room ID {booking.RoomId} not found.");
            }

            Bookings newBooking = new Bookings()
            {
                UserId = booking.UserId,
                PeopleCount = booking.PeopleCount,
                BookingPrice = 0,
                RoomId = booking.RoomId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                ServiceId = booking.ServiceId,
                CarId = booking.CarId,
            };

            newBooking.BookingPrice = newBooking.CalculateBookingPrice(newBooking, selectedRoom, selectedCar, servicePrices);
            bookingsMapped.Add(newBooking);
        }

        _context.Booking.AddRange(bookingsMapped);
        await _context.SaveChangesAsync();
        return Ok(bookingsMapped);
    }

    [HttpPost("GenTickets")]
    public async Task<ActionResult<List<TicketCreateDTO>>> GenTickets(int count = 10)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }
        
        List<int> ValidUserIdList = _context.Users.Select(u => u.Id).ToList();

        if (!ValidUserIdList.Any())
        {
            return BadRequest("No Users in Database!");
        }
        
        List<TicketCreateDTO> ticketsGenerated = KelBurgAPI.BogusGenerators.BogusTicket.GenerateRooms(count, ValidUserIdList);
        List<Tickets> ticketsMappedList = new List<Tickets>();

        foreach (TicketCreateDTO ticket in ticketsGenerated)
        {
            Tickets ticketMapped = new Tickets()
            {
                FromUser = ticket.FromUser,
                Description = ticket.Description,
                Status = ticket.Status,
                Category = ticket.Category,
            };
            ticketsMappedList.Add(ticketMapped);
        }

        _context.Tickets.AddRange(ticketsMappedList);
        await _context.SaveChangesAsync();
        return Ok(ticketsMappedList);
    }

    [HttpPost("GenCars")]
    public async Task<ActionResult<List<HotelCarsDTO>>> GenCar(int MaxCount = 10)
    {
        List<HotelCarsDTO> generatedCars = BogusHotelCars.GenCars(MaxCount);
        List<HotelCars> mappedCars = new List<HotelCars>();
        foreach (HotelCarsDTO currentCar in generatedCars)
        {
            HotelCars carsToBeCreated = new HotelCars()
            {
                Manufacturer = currentCar.Manufacturer,
                Model = currentCar.Model,
                Vin = currentCar.Vin,
                Size = currentCar.Size,
                Type = currentCar.Type,
                Fuel = currentCar.Fuel,
                PricePrNight = currentCar.PricePrNight,
            };

            mappedCars.Add(carsToBeCreated);
        }

        _context.HotelCars.AddRange(mappedCars);
        await _context.SaveChangesAsync();
        return Ok(mappedCars);
    }
}