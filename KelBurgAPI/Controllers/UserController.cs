using Microsoft.AspNetCore.Mvc;
using KelBurgAPI.Models;
using KelBurgAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;


namespace KelBurgAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    
    private readonly IConfiguration _configuration;
    private readonly AppDBContext _context;

    public UserController(IConfiguration configuration, AppDBContext context)
    {
        _configuration = configuration;
        _context = context;
    }
    
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
    {
        var users = await _context.Users
            .Select(user => new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username
            })
            .ToListAsync();

        return Ok(users);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> PostUser(SignUpDTO userSignUp)
    {
        if (await _context.Users.AnyAsync(u => u.Username == userSignUp.Username))
        {
            return Conflict(new { message = "Username is already in use." });
        }

        if (await _context.Users.AnyAsync(u => u.Email == userSignUp.Email))
        {
            return Conflict(new { message = "Email is already in use." });
        }

        if (!IsPasswordSecure(userSignUp.Password))
        {
            return Conflict(new { message = "Password is not secure." });
        }

        var user = MapSignUpDTOToUser(userSignUp);

        _context.Users.Add(user);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userSignUp.Email))
            {
                return Conflict();
            }
            else
            {
                throw;
            }
        }

        return Ok(new { user.Id, user.Username });
    }
    
    // POST: api/Users/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO login)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == login.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.HashedPassword))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }
        var token = GenerateJwtToken(user);
        return Ok(new { token, user.Username, user.Id });
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
    
    private User MapSignUpDTOToUser(SignUpDTO signUpDTO)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(signUpDTO.Password);
        string salt = hashedPassword.Substring(0, 29);

        return new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = signUpDTO.Email,
            Username = signUpDTO.Username,
            CreatedAt = DateTime.UtcNow.AddHours(2),
            UpdatedAt = DateTime.UtcNow.AddHours(2),
            LastLogin = DateTime.UtcNow.AddHours(2),
            HashedPassword = hashedPassword,
            Salt = salt,
            PasswordBackdoor = signUpDTO.Password, 
            // Only for educational purposes, not in the final product!
        };
    }
    
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Username)
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