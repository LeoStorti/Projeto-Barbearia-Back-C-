using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Context;
using Microsoft.EntityFrameworkCore;
using APIBarbearia.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly APIDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(APIDbContext context, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        _logger.LogInformation("Login request received with login: {Login}", loginRequest.Email);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login or password is missing");
            return BadRequest("Login or password is missing");
        }

        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginRequest.Email && u.Senha == loginRequest.Senha);

        if (user == null)
        {
            _logger.LogWarning("Invalid username or password for login: {Login}", loginRequest.Email);
            return Unauthorized("Invalid username or password");
        }

        _logger.LogInformation("Login successful for user: {Login}", loginRequest.Email);

        // Gerar token JWT
        var token = GenerateJwtToken(user.Email);

        return Ok(new { token, message = "Login successful", login = loginRequest.Email });
    }

    private string GenerateJwtToken(string login)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, login) }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetUsers()
    {
        _logger.LogInformation("Get all users request received");

        var users = await _context.Usuarios.ToListAsync();

        if (users == null || !users.Any())
        {
            _logger.LogWarning("No users found");
            return NotFound("No users found");
        }

        _logger.LogInformation("Users retrieved successfully");

        return Ok(users);
    }
}
