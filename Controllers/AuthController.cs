using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Patrakov.Data;
using Patrakov.Models;
using Patrakov.Services;
using BCrypt.Net;

namespace Patrakov.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _tokenService;

    public AuthController(AppDbContext context, IJwtTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Patrakov.Models.Auth.RegisterRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            
            var existingUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            var user = new AppUser
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            
            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);
            return Ok(new Patrakov.Models.Auth.AuthResponse { Token = token, Email = user.Email });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server error: " + ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Patrakov.Models.Auth.LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

           
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var token = _tokenService.GenerateToken(user);
            return Ok(new Patrakov.Models.Auth.AuthResponse { Token = token, Email = user.Email });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server error: " + ex.Message });
        }
    }
}