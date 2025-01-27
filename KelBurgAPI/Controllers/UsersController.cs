using KelburgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelburgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class UsersController : ControllerBase
{
    private readonly DatabaseContext _context;

    public UsersController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<ActionResult<Users>> CreateUser([FromQuery] UserCreateDTO user)
    {
        if (user == null)
        {
            return BadRequest("User is null");
        }

        Users newUser = new Users()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password,
            Address = user.Address,
            City = user.City,
            PostalCode = user.PostalCode,
            Country = user.Country,
            CountryCode = user.CountryCode,
            PhoneNumber = user.PhoneNumber,
            BookingId = user.BookingId,
            AccountType = user.AccountType,
        };
        
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, newUser );
    }

    [HttpGet("read")]
    public async Task<ActionResult<IEnumerable<Users>>> GetUsers(string? FirstName, string? LastName, int pageSize = 100, int pageNumber = 1) 
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and size must be greater than 0");
        }
        
        int allPeople = _context.Users.Count();
        int totalPages = (int)Math.Ceiling(allPeople / (double)pageSize);

        if (pageNumber > totalPages)
        {
            return NotFound("Page number exceeds total pages");
        }
        
        List<Users> users = new List<Users>();
        
        if (FirstName != null && LastName == null)
        {
            users = await _context.Users.Where(c => c.FirstName == FirstName)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
            
        } else if (LastName != null && FirstName == null)
        {
            users = await _context.Users.Where(c => c.LastName == LastName)  
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
            
        } else if (LastName != null && FirstName != null)
        {
            users = await _context.Users.Where(c => c.LastName == LastName && c.FirstName == FirstName)  
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        else
        {
            users = await _context.Users
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        return Ok(users);
    }

    [HttpGet("findById")]
    public async Task<ActionResult<Users>> GetUserById([FromQuery] int id)
    {
        Users foundUser = await _context.Users.FindAsync(id);

        if (foundUser == null)
        {
            return NotFound("User not found");
        }
        
        return Ok(foundUser);
    }
}