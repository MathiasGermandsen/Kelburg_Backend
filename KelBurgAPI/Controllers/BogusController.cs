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
        List<Rooms> roomsInUse = new List<Rooms>();
        List<Rooms> roomsAvailable = new List<Rooms>();

        foreach (Bookings existingBooking in allExistingBookings)
        {
            foreach (Rooms room in allRooms)
            {
                if (existingBooking.RoomId == room.Id && existingBooking.EndDate.Date > today &&
                    !roomsInUse.Contains(room))
                {
                    roomsInUse.Add(room);

                    if (roomsAvailable.Contains(room))
                    {
                        roomsAvailable.Remove(room);
                    }
                }
                else if (!roomsAvailable.Contains(room) && !roomsInUse.Contains(room))
                {
                    roomsAvailable.Add(room);
                }
            }
        }

        int countToUse = 0;

        if (roomsAvailable.Count > MaxCount)
        {
            countToUse = MaxCount;
        }
        else
        {
            countToUse = roomsAvailable.Count;
        }

        List<BookingCreateDTO> bookingsGenerated =
            KelBurgAPI.BogusGenerators.BogusBooking.GenerateBookings(countToUse, validUserIdList, roomsAvailable);

        List<Bookings> bookingsMapped = new List<Bookings>();
        List<ServicePricesDict> servicePrices = _context.ServicePricesDict.ToList();

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
                Breakfast = booking.Breakfast,
                AllInclusive = booking.AllInclusive,
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


