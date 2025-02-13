using KelBurgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;

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
    public async Task<ActionResult<Rooms>> CreateBooking([FromQuery] BookingCreateDTO booking) // Change to FromQuery when doing frontend
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
            return BadRequest($"Room is not available at this date");
        }
        
        List<Services> servicePricesDicts = _context.Services.ToList();
        
        bookingToBeCreated.BookingPrice = bookingToBeCreated.CalculateBookingPrice(bookingToBeCreated, SelectedRoom, servicePricesDicts);
        
        _context.Booking.Add(bookingToBeCreated);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBookings), new { id = bookingToBeCreated.Id }, bookingToBeCreated);
    }

    [HttpGet("read")]
    public async Task<ActionResult<IEnumerable<Bookings>>> GetBookings(int? BookingId, int? UserId, int? RoomId, int pageSize = 100, int pageNumber = 1)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and size must be greater than 0");
        }
        
        IQueryable<Bookings> query = _context.Booking.AsQueryable();

        if (BookingId.HasValue)
        {
            query = query.Where(c => c.Id == BookingId);
        }
        if (UserId.HasValue)
        {
            query = query.Where(c => c.UserId == UserId);
        }
        if (RoomId.HasValue)
        {
            query = query.Where(c => c.RoomId == RoomId);
        }

        List<Bookings> bookings = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(bookings);
    }
    
    [HttpPut("update")]
    public async Task<ActionResult<Bookings>> EditBooking(int bookingIdToChange, [FromQuery] BookingEditDTO editedBooking)
    {
        Bookings bookingToEdit = await _context.Booking.FindAsync(bookingIdToChange);
        
        Rooms selectedRoom = await _context.Rooms.FindAsync(editedBooking.RoomId);
        List<Services> services = await _context.Services.ToListAsync();
        
        List<Bookings> allExistingBookings = _context.Booking.ToList();
        
        if (allExistingBookings.Contains(bookingToEdit))
        {
            allExistingBookings.Remove(bookingToEdit);
        }
        
        if (bookingToEdit == null)
        {
            return NotFound();
        }

        PropertyInfo[] dtoProperties = typeof(BookingEditDTO).GetProperties();
        PropertyInfo[] existingProperties = typeof(Bookings).GetProperties();

        foreach (PropertyInfo dtoProp in dtoProperties)
        {
            object? newValue = dtoProp.GetValue(editedBooking);
            if (newValue != null)
            {
                PropertyInfo? existingProp = existingProperties.FirstOrDefault(p => p.Name == dtoProp.Name);
                if (existingProp != null && existingProp.CanWrite)
                {
                    existingProp.SetValue(bookingToEdit, newValue);
                }
            }
        }
        
        if (selectedRoom != null)
        {
            if (!selectedRoom.IsRoomAvailableAtDate(allExistingBookings, selectedRoom, bookingToEdit))
            {
                return BadRequest("Room is not available at this date");
            }
        }
        
        bookingToEdit.BookingPrice = bookingToEdit.CalculateBookingPrice(bookingToEdit, selectedRoom, services);

        await _context.SaveChangesAsync();
        return Ok(bookingToEdit);
    }

    [HttpDelete("delete")]
    public async Task<ActionResult<Bookings>> DeleteBooking(int bookingId)
    {
        Bookings bookingsToDelete = await _context.Booking.FindAsync(bookingId);

        if (bookingsToDelete == null)
        {
            return NotFound("Booking not found");
        }
        
        _context.Booking.Remove(await _context.Booking.FindAsync(bookingId));
        await _context.SaveChangesAsync();
        return Ok(bookingsToDelete);
    }
}