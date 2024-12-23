using CryptoExchangeBackend.Models;
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

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "Bu e-posta adresi zaten kayıtlıdır." });
        }

        user.Password = HashPassword(user.Password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Kullanıcı başarıyla kaydedildi." });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == loginRequest.Email);

        if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
            return Unauthorized(new { message = "Geçersiz e-posta veya şifre." });

        var token = _jwtService.GenerateToken(user.Username, user.Email); 

        return Ok(new
        {
            token,
            username = user.Username,
        });
    }
}
