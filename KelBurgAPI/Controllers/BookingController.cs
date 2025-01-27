using KelburgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelburgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class BookingController : ControllerBase
{
    private readonly DatabaseContext _context;

    public BookingController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<ActionResult<Rooms>> CreateBooking([FromBody] BookingCreateDTO booking) // Change to FromQuery when doing frontend
    {
        if (booking == null)
        {
            return BadRequest("Booking is null");
        } 
        
        Booking newBooking = new Booking()
        {
            PeopleCount = booking.PeopleCount,
            BookingPrice = 0, // Will be calculated
            RoomId = booking.RoomId,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            Breakfast = booking.Breakfast,
            AllInclusive = booking.AllInclusive,
        };
        
        List<ServicePricesDict> servicePricesDicts = _context.ServicePricesDict.ToList();
        Rooms selectedRoom = _context.Rooms.Find(booking.RoomId);
        
        newBooking.BookingPrice = newBooking.CalculateBookingPrice(newBooking, selectedRoom, servicePricesDicts);
        
        _context.Booking.Add(newBooking);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBookings), new { id = newBooking.Id }, newBooking);
    }

    [HttpGet("read")]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookings(int? roomId, int pageSize = 100, int pageNumber = 1)
    {
        List<Booking> bookings = new List<Booking>();
        
        if (roomId != null)
        {
            bookings = await _context.Booking.Where(c => c.RoomId == roomId)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        else
        {
            bookings = await _context.Booking
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();  
        }
        
        return Ok(bookings);
    }

    [HttpDelete("delete")]
    public async Task<ActionResult<Booking>> DeleteBooking(int bookingId)
    {
        Booking bookingToDelete = await _context.Booking.FindAsync(bookingId);

        if (bookingToDelete == null)
        {
            return NotFound();
        }
        _context.Booking.Remove(await _context.Booking.FindAsync(bookingId));
        await _context.SaveChangesAsync();
        return Ok(bookingToDelete);
    }
}