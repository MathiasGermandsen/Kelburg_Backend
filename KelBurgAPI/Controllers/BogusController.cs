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

        await Parallel.ForEachAsync(usersGenerated, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, count / 20) }, async (user, _) =>
        {
            Users userMapped = new Users()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                HashedPassword = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(user.Password)),
                PasswordBackdoor = user.Password,
                Address = user.Address,
                City = user.City,
                PostalCode = user.PostalCode,
                Country = user.Country,
                PhoneNumber = user.PhoneNumber,
                AccountType = user.AccountType
            };

            lock (usersMapped)
            {
                usersMapped.Add(userMapped);
            }
        });

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
        Rooms roomInstance = new Rooms();
        
        int countToUse = 0;

        List<BookingCreateDTO> bookingsGenerated = KelBurgAPI.BogusGenerators.BogusBooking.GenerateBookings(MaxCount, validUserIdList, allRooms, allExistingBookings);

        List<Bookings> bookingsMapped = new List<Bookings>();
        
        foreach (BookingCreateDTO booking in bookingsGenerated)
        {
            Bookings newBooking = new Bookings()
            {
                UserId = booking.UserId,
                PeopleCount = booking.PeopleCount,
                BookingPrice = 0,
                RoomId = booking.RoomId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                ServiceId = booking.ServiceId,
            };

            Rooms? SelectedRoom = allRooms.Find(r => r.Id == booking.RoomId);

            newBooking.BookingPrice = newBooking.CalculateBookingPrice(newBooking, SelectedRoom, servicePrices);
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
}


