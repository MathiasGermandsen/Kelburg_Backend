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
                AccountType =  user.AccountType
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
        
        List<RoomCreateDTO> roomsGenerated = KelBurgAPI.BogusGenerators.BogusRooms.GenerateRooms(count);
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

        DateTime today = DateTime.Now.Date;
        List<Bookings> allExistingBookings = _context.Booking.ToList();

        List<Rooms> allRooms = _context.Rooms.ToList();
        
        Dictionary<int, DateTime> latestEndDates = _context.Booking
            .GroupBy(b => b.RoomId)
            .Select(g => new { RoomId = g.Key, LatestEndDate = g.Max(b => b.EndDate) })
            .ToDictionary(x => x.RoomId, x => x.LatestEndDate);
        
        List<Rooms> roomsAvailable = allRooms
            .Where(r =>
            {
                var overlappingBookings = allExistingBookings
                    .Where(b => b.RoomId == r.Id && (b.StartDate < today && b.EndDate>=today))
                    .ToList();
                    
                    bool isAvailable = !overlappingBookings.Any() || 
                                       (latestEndDates.ContainsKey(r.Id) && latestEndDates[r.Id] < today);
                    return isAvailable;
            })
            .ToList();

        int countToUse = Math.Min(roomsAvailable.Count, MaxCount);

        List<BookingCreateDTO> bookingsGenerated =
            KelBurgAPI.BogusGenerators.BogusBooking.GenerateBookings(countToUse, validUserIdList, roomsAvailable);

        List<Bookings> bookingsMapped = new List<Bookings>();
        List<Services> servicePrices = _context.Services.ToList();
        
        if (!servicePrices.Any())
        {
            return BadRequest("No Service-prices found. Cannot make booking.");
        }
        
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

            Rooms? SelectedRoom = roomsAvailable.Find(r => r.Id == booking.RoomId);

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
        List<TicketCreateDTO> ticketsGenerated = KelBurgAPI.BogusGenerators.BogusTicket.GenerateRooms(count, ValidUserIdList);
        List<Tickets> ticketsMappedList = new List<Tickets>();

        foreach (TicketCreateDTO ticket in ticketsGenerated)
        {
            Tickets ticketMapped = new Tickets()
            {
                FromUser = ticket.FromUser,
                Description = ticket.Description,
                Stars = ticket.Stars,
            };
            ticketsMappedList.Add(ticketMapped);
        }
        _context.Tickets.AddRange(ticketsMappedList);
        await _context.SaveChangesAsync();
        return Ok(ticketsMappedList);
    }
}


