using KelBurgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelBurgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class BookingsController : ControllerBase
{
    private readonly DatabaseContext _context;

    public BookingsController(DatabaseContext context)
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
        
        Bookings bookingToBeCreated = new Bookings()
        {
            UserId = booking.UserId,
            PeopleCount = booking.PeopleCount,
            BookingPrice = 0, // Will be calculated
            RoomId = booking.RoomId,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            ServiceId = booking.ServiceId,
        };
        
        List<Bookings> allExistingBookings = _context.Booking.ToList();
        Rooms roomInstance = new Rooms();
        
        Rooms SelectedRoom = _context.Rooms.Find(booking.RoomId);

        bool RoomAvailableAtDate = true;

        if (allExistingBookings.Any())
        {
            RoomAvailableAtDate = roomInstance.IsRoomAvailableAtDate(allExistingBookings, SelectedRoom, bookingToBeCreated);      
        }

        if (!RoomAvailableAtDate)
        {
            return BadRequest($"Room is not available between {bookingToBeCreated.StartDate} and {bookingToBeCreated.EndDate}");
        }
        
        List<Services> servicePricesDicts = _context.Services.ToList();
        
        bookingToBeCreated.BookingPrice = bookingToBeCreated.CalculateBookingPrice(bookingToBeCreated, SelectedRoom, servicePricesDicts);
        
        _context.Booking.Add(bookingToBeCreated);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBookings), new { id = bookingToBeCreated.Id }, bookingToBeCreated);
    }

    [HttpGet("read")]
    public async Task<ActionResult<IEnumerable<Bookings>>> GetBookings(int? roomId, int pageSize = 100, int pageNumber = 1)
    {
        List<Bookings> bookings = new List<Bookings>();
        
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
    
    [HttpGet("findByUserId")]
    public async Task<ActionResult<Bookings>> FindByUserId(int UserId)
    {
        List<Bookings> foundBookings = await _context.Booking.Where(c => c.UserId == UserId).ToListAsync();

        if (foundBookings == null)
        {
            return NotFound("No bookings found");
        }
        
        return Ok(foundBookings);
    }

    [HttpDelete("delete")]
    public async Task<ActionResult<Bookings>> DeleteBooking(int bookingId)
    {
        Bookings bookingsToDelete = await _context.Booking.FindAsync(bookingId);

        if (bookingsToDelete == null)
        {
            return NotFound();
        }
        _context.Booking.Remove(await _context.Booking.FindAsync(bookingId));
        await _context.SaveChangesAsync();
        return Ok(bookingsToDelete);
    }
}