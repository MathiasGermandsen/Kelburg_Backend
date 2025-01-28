using KelBurgAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using KelBurgAPI.Data;
using Microsoft.AspNetCore.Authorization;

namespace KelBurgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class UsersController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IConfiguration _configuration;

    public UsersController(DatabaseContext context, IConfiguration configuration)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("create")]
    public async Task<ActionResult<Users>> CreateUser([FromQuery] UserCreateDTO user)
    {
        if (user == null)
        {
            return BadRequest("User is null");
        }
        
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
            return Conflict(new { message = "Email is already in use." });
        }
        
        if (!IsPasswordSecure(user.Password))
        {
            return Conflict(new { message = "Password is not secure." });
        }
        
        
        Users newUser = new Users()
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
            AccountType = user.AccountType,
        };
        
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, newUser );
    }

    [Authorize]
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
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDTO login)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == login.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.HashedPassword))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }
        var token = GenerateJwtToken(user);
        return Ok(new { token, user.FirstName, user.Id });
    }
    
    private bool IsPasswordSecure(string password)
    {
        var hasUpperCase = new Regex(@"[A-Z]+");
        var hasLowerCase = new Regex(@"[a-z]+");
        var hasDigits = new Regex(@"[0-9]+");
        var hasSpecialChar = new Regex(@"[\W_]+");
        var hasMinimum8Chars = new Regex(@".{8,}");

        return hasUpperCase.IsMatch(password)
               && hasLowerCase.IsMatch(password)
               && hasDigits.IsMatch(password)
               && hasSpecialChar.IsMatch(password)
               && hasMinimum8Chars.IsMatch(password);
    }
    
    private string GenerateJwtToken(Users user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes
            (_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}