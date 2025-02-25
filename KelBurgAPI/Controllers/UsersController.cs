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
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Net.Http.Headers;

namespace KelBurgAPI.Controllers;

public static class SecretHelper
{
    public static string GetSecretValue(string keyOrFilePath)
    {
        if (!string.IsNullOrEmpty(keyOrFilePath) && File.Exists(keyOrFilePath))
        {
            return File.ReadAllText(keyOrFilePath).Trim();
        }
        return keyOrFilePath;
    }
}

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

    [HttpGet("read")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Users>>> GetUsers(int? userId, string? firstName, string? lastName, int pageSize = 100, int pageNumber = 1) 
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and size must be greater than 0");
        }
        
        IQueryable<Users> query = _context.Users.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(c => c.Id == userId);
        }
        
        if (!string.IsNullOrEmpty(firstName))
        {
            query = query.Where(c => c.FirstName.ToLower().Contains(firstName.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(lastName))
        {
            query = query.Where(c => c.LastName.ToLower().Contains(lastName.ToLower()));
        }

        List<Users> users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(users);
    }
    
    [HttpGet("getUserFromToken")]
    //[Authorize] Don't know how to make it work with Authorize - Arian
    public async Task<ActionResult<Users>> GetUserFromToken(string jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken))
        {
            return BadRequest("Token is required");
        }

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken token = handler.ReadJwtToken(jwtToken);

        string? userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        Users? user = await _context.Users.FindAsync(int.Parse(userId));

        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
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
        var jwtKey = SecretHelper.GetSecretValue(_configuration["JwtSettings:Key"]);
        var jwtIssuer = SecretHelper.GetSecretValue(_configuration["JwtSettings:Issuer"]);
        var jwtAudience = SecretHelper.GetSecretValue(_configuration["JwtSettings:Audience"]);
        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Email)
        };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes
            (jwtKey));
        SigningCredentials? creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            jwtIssuer,
            jwtAudience,
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    [HttpDelete("delete")]
    public async Task<ActionResult<Users>> DeleteUser(int userId)
    {
        Users user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }
    

}