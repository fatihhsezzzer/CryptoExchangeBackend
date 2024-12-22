using CryptoExchangeBackend.Models;
using CryptoExchangeBackend.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public IActionResult Login(User user)
    {
        var dbUser = _context.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);

        if (dbUser == null)
            return Unauthorized(new { message = "Invalid username or password" });

        var token = _jwtService.GenerateToken(user.Username);
        return Ok(new { token });
    }
}
