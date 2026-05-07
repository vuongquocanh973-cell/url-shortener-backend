using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UrlShortener.Data;
using UrlShortener.Models;
using Microsoft.AspNetCore.Authorization;

namespace UrlShortener.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _jwtKey = "UrlShortener_Secret_Key_Super_Secure_123456";

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/users/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] User user)
    {
        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            return BadRequest(new { message = "Username already exists!" });

        // Hash password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Registration successful!", userId = user.Id });
    }

    // POST: api/users/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] User loginInfo)
    {
        // Find user by username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginInfo.Username);

        // Verify password
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginInfo.PasswordHash, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password!" });

        // Generate JWT Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new
        {
            token = tokenHandler.WriteToken(token),
            username = user.Username
        });
    }

    // GET: api/users/me
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMyProfile()
    {
        var userName = User.Identity?.Name;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new
        {
            message = "Welcome!",
            user = userName,
            id = userId
        });
    }
}