using KelBurgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelBurgAPI.Controllers;

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
            Category = ticket.Category,
            Status = ticket.Status,
        };
        
        _context.tickets.Add(newTicket);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTickets), new { id = newTicket.Id }, newTicket);
    }

    [HttpGet("read")]
    public async Task<ActionResult<IReadOnlyList<Tickets>>> GetTickets(int? ticketId, int? fromUserId, string? ticketStatus, string? ticketCategory, int pageSize = 100, int pageNumber = 1)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and size must be greater than 0");
        }
        
        IQueryable<Tickets> query = _context.tickets.AsQueryable();

        if (ticketId.HasValue)
        {
            query = query.Where(c => c.Id == ticketId);
        } 
        if (fromUserId.HasValue)
        {
            query = query.Where(c => c.FromUser == fromUserId);
        }
        
        if (ticketStatus != null)
        {
            query = query.Where(c => c.Status == ticketStatus);
        }
        
        if (ticketCategory != null)
        {
            query = query.Where(c => c.Category == ticketCategory);
        }

        List<Tickets> tickets = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(tickets);
    }
    
    [HttpPatch("updateStatus")]
    public async Task<ActionResult<Tickets>> UpdateTicketStatus(int ticketId, string newStatus)
    {
        Tickets ticketToUpdate = await _context.tickets.FindAsync(ticketId);

        if (ticketToUpdate == null)
        {
            return NotFound("Ticket not found");
        }

        ticketToUpdate.Status = newStatus;
        await _context.SaveChangesAsync();
        return Ok(ticketToUpdate);
    }
    
    [HttpDelete("delete")]
    public async Task<ActionResult<Tickets>> DeleteTicket(int ticketId)
    {
        Tickets ticketToDelete = await _context.tickets.FindAsync(ticketId);

        if (ticketToDelete == null)
        {
            return NotFound("Ticket not found");
        }
        
        _context.tickets.Remove(await _context.tickets.FindAsync(ticketId));
        await _context.SaveChangesAsync();
        return Ok(ticketToDelete);
    }
}