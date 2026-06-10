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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using APIBarbearia.Services;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly APIDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public AuthController(APIDbContext context, ILogger<AuthController> logger, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null)
        {
            return BadRequest("Invalid request");
        }

        var email = loginRequest.Email?.Trim();
        var senha = loginRequest.Senha;

        _logger.LogInformation("Login request received with login: {Login}", email);

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            _logger.LogWarning("Invalid login payload");
            return BadRequest("Invalid login payload");
        }

        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !PasswordService.VerifyPassword(senha, user.Senha))
        {
            _logger.LogWarning("Invalid username or password for login: {Login}", email);
            return Unauthorized("Invalid username or password");
        }

        // Transparently migrate legacy plaintext passwords on successful login.
        if (!PasswordService.IsHashed(user.Senha))
        {
            user.Senha = PasswordService.HashPassword(senha);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Login successful for user: {Login}", email);

        // Gerar token JWT
        var token = GenerateJwtToken(user);

        Response.Cookies.Append("auth_token", token, BuildAuthCookieOptions());

        return Ok(new
        {
            message = "Login successful",
            login = email,
            empresaId = user.EmpresaId,
            usuarioId = user.UsuarioId,
            nivelAcesso = user.NivelAcesso
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var options = BuildAuthCookieOptions();
        options.Expires = DateTimeOffset.UtcNow.AddDays(-1);
        Response.Cookies.Append("auth_token", string.Empty, options);
        return Ok(new { message = "Logout successful" });
    }

    private string GenerateJwtToken(Usuario user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required in configuration.");
        var key = Encoding.ASCII.GetBytes(jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("EmpresaId", user.EmpresaId.ToString()),
                new Claim("UsuarioId", user.UsuarioId.ToString()),
                new Claim("NivelAcesso", user.NivelAcesso ?? string.Empty),
            }),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private CookieOptions BuildAuthCookieOptions()
    {
        var isDev = _hostEnvironment.IsDevelopment();
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddHours(8),
            IsEssential = true
        };
    }

    [Authorize]
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetUsers()
    {
        _logger.LogInformation("Get all users request received");

        var empresaId = await GetEmpresaIdAsync();
        if (!empresaId.HasValue)
        {
            return Forbid();
        }

        var users = await _context.Usuarios
            .Where(u => u.EmpresaId == empresaId.Value)
            .Select(u => new Usuario
            {
                UsuarioId = u.UsuarioId,
                NomeUsuario = u.NomeUsuario,
                Email = u.Email,
                NivelAcesso = u.NivelAcesso,
                EmpresaId = u.EmpresaId,
                Senha = string.Empty,
            })
            .ToListAsync();

        if (users == null || !users.Any())
        {
            _logger.LogWarning("No users found");
            return NotFound("No users found");
        }

        _logger.LogInformation("Users retrieved successfully");

        return Ok(users);
    }

    private async Task<int?> GetEmpresaIdAsync()
    {
        var claim = User.FindFirst("EmpresaId")?.Value;
        if (!string.IsNullOrWhiteSpace(claim) && int.TryParse(claim, out var claimEmpresaId) && claimEmpresaId > 0)
        {
            return claimEmpresaId;
        }

        var email = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirst("unique_name")?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => (int?)u.EmpresaId)
            .FirstOrDefaultAsync();
    }
}
