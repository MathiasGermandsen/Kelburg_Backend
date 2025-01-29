using KelBurgAPI.Data;
using KelBurgAPI.Models;
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
        
        _context.Rooms.Add(newRoom);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRooms), new { id = newRoom.Id }, newRoom);
    }

    [HttpGet("read")]
    public async Task<ActionResult<IEnumerable<Rooms>>> GetRooms(int roomId, int roomSize, int pageSize = 100, int pageNumber = 1)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and size must be greater than 0");
        }
        
        List<Rooms> rooms = new List<Rooms>();
        
        if (roomId > 0 && roomSize > 0)
        {
            rooms = await _context.Rooms.Where(c => c.Id == roomId && c.Size == roomSize)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        } else if (roomId > 0 && roomSize <= 0)
        {
            rooms = await _context.Rooms.Where(c => c.Id == roomId)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        } else if (roomId <= 0 && roomSize > 0)
        {
            rooms = await _context.Rooms.Where(c =>  c.Size == roomSize)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        } else
        {
            rooms = await _context.Rooms
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        return Ok(rooms);
    }

    [HttpPatch("changePriceId")]
    public async Task<ActionResult<Rooms>> ChangePriceId(int roomIdToChange, int newPriceId)
    {
        Rooms roomTopatch = await _context.Rooms.FindAsync(roomIdToChange);
        roomTopatch.PricePrNight = newPriceId;
        await _context.SaveChangesAsync();
        return Ok(roomTopatch);
    }

    [HttpDelete("delete")]
    public async Task<ActionResult<Rooms>> DeleteRoom(int roomId)
    {
        Rooms room = await _context.Rooms.FindAsync(roomId);
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return Ok(room);
    }
}