using KelBurgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelBurgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly DatabaseContext _context;

    public RoomsController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<ActionResult<Rooms>> CreateRoom([FromQuery] RoomCreateDTO room)
    {
        if (room == null)
        {
            return BadRequest("Room already exists");
        }

        Rooms newRoom = new Rooms()
        {
            Size = room.Size,
            RoomType = room.RoomType,
            ViewType = room.ViewType,
            PricePrNight = room.PricePrNight,
        };

        _context.rooms.Add(newRoom);
        await _context.SaveChangesAsync();
        return Ok(newRoom);
    }

    [HttpGet("read")]
    public async Task<ActionResult<IEnumerable<Rooms>>> GetRooms(int? roomId, int? roomSize, int pageSize = 100,
        int pageNumber = 1)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and size must be greater than 0");
        }

        IQueryable<Rooms> query = _context.rooms.AsQueryable();

        if (roomId.HasValue)
        {
            query = query.Where(c => c.Id == roomId);
        }

        if (roomSize.HasValue)
        {
            query = query.Where(c => c.Size == roomSize);
        }

        List<Rooms> rooms = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(rooms);
    }

    [HttpPatch("changePrice")]
    public async Task<ActionResult<Rooms>> ChangePriceId(int roomIdToChange, int newPrice)
    {
        Rooms roomTopatch = await _context.rooms.FindAsync(roomIdToChange);
        roomTopatch.PricePrNight = newPrice;
        await _context.SaveChangesAsync();
        return Ok(roomTopatch);
    }

    [HttpGet("availableBetweenDates")]
    public async Task<ActionResult<IEnumerable<Rooms>>> GetAvailableBetweenDates(DateTime startDate, DateTime endDate,
        int? roomSize, int pageSize = 100, int pageNumber = 1)
    {

        List<Rooms> allRooms = await _context.rooms.ToListAsync();
        List<Bookings> allBookings = await _context.booking.ToListAsync();


        List<Rooms> availableRooms = allRooms
            .Where(room =>
                (!roomSize.HasValue || room.Size == roomSize.Value) &&
                !allBookings.Any(booking =>
                    booking.RoomId == room.Id &&
                    ((booking.StartDate < endDate && booking.EndDate > startDate) ||
                     (startDate < booking.EndDate && endDate > booking.StartDate))
                )
            )
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(availableRooms);
    }

    [HttpGet("unavailableBetweenDates")]
    public async Task<ActionResult<IEnumerable<Rooms>>> GetUnavailableBetweenDates(DateTime startDate, DateTime endDate,
        int? roomSize, int pageSize = 100, int pageNumber = 1)
    {
        List<Rooms> allRooms = await _context.rooms.ToListAsync();
        List<Bookings> allBookings = await _context.booking.ToListAsync();

        List<Rooms> availableRooms = allRooms
            .Where(room =>
                (!roomSize.HasValue || room.Size == roomSize.Value) &&
                allBookings.Any(booking =>
                    booking.RoomId == room.Id &&
                    ((booking.StartDate < endDate && booking.EndDate > startDate) ||
                     (startDate < booking.EndDate && endDate > booking.StartDate))
                )
            )
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(availableRooms);
    }

    [HttpDelete("delete")]
    public async Task<ActionResult<Rooms>> DeleteRoom(int roomId)
    {

        Rooms room = await _context.rooms.FindAsync(roomId);


        if (room == null)
        {
            return NotFound("Room not found");
        }
        
        bool hasActiveBookings = _context.Booking.Any(b=> b.RoomId == room.Id && b.EndDate.ToUniversalTime().Date >= DateTime.Now.ToUniversalTime().Date);

        if (hasActiveBookings)
        {
            return BadRequest("Cannot delete room cause of booking");
        }
        
        _context.rooms.Remove(room);
  
        await _context.SaveChangesAsync();
        return Ok(room);
    }
}