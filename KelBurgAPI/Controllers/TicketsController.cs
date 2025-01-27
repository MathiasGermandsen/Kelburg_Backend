using KelburgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelburgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TicketsController : ControllerBase
{
    private readonly DatabaseContext _context;

    public TicketsController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<ActionResult<Tickets>> CreateTicket([FromQuery] TicketCreateDTO ticket)
    {
        if (ticket == null)
        {
            return BadRequest("Ticket is null");
        }
        
        Tickets newTicket = new Tickets()
        {
            FromUser = ticket.FromUser,
            Description = ticket.Description,
            Stars = ticket.Stars,
        };
        
        _context.Tickets.Add(newTicket);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTickets), new { id = newTicket.Id }, newTicket);
    }

    [HttpGet("read")]
    public async Task<ActionResult<IReadOnlyList<Tickets>>> GetTickets(int? fromUserId, int pageSize = 100, int pageNumber = 1)
    {
        List<Tickets> tickets = new List<Tickets>();
        
        if (fromUserId != null)
        {
            tickets = await _context.Tickets.Where(c => c.FromUser == fromUserId)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        else
        {
            tickets = await _context.Tickets
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();  
        }
        
        return Ok(tickets);
    }
    
    [HttpDelete("delete")]
    public async Task<ActionResult<Tickets>> DeleteTicket(int ticketId)
    {
        Tickets ticketToDelete = await _context.Tickets.FindAsync(ticketId);

        if (ticketToDelete == null)
        {
            return NotFound();
        }
        _context.Tickets.Remove(await _context.Tickets.FindAsync(ticketId));
        await _context.SaveChangesAsync();
        return Ok(ticketToDelete);
    }
}