using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using APIBarbearia.Models;
using API.Context;
using APIBarbearia.Services;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly APIDbContext _context;

        public UsuariosController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            return await _context.Usuarios
                .Where(u => u.EmpresaId == empresaId.Value)
                .Select(u => SanitizeUsuario(u))
                .ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var usuario = await _context.Usuarios
                .Where(u => u.UsuarioId == id && u.EmpresaId == empresaId.Value)
                .Select(u => SanitizeUsuario(u))
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.UsuarioId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var existing = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == id && u.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            existing.NomeUsuario = usuario.NomeUsuario;
            existing.Email = usuario.Email;
            existing.NivelAcesso = usuario.NivelAcesso;
            if (!string.IsNullOrWhiteSpace(usuario.Senha))
            {
                existing.Senha = PasswordService.IsHashed(usuario.Senha)
                    ? usuario.Senha
                    : PasswordService.HashPassword(usuario.Senha);
            }

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            usuario.EmpresaId = empresaId.Value;

            if (usuario.UsuarioId <= 0)
            {
                var maxId = await _context.Usuarios
                    .Select(u => (int?)u.UsuarioId)
                    .MaxAsync() ?? 0;

                usuario.UsuarioId = maxId + 1;
            }

            if (string.IsNullOrWhiteSpace(usuario.Senha))
            {
                return BadRequest("Senha is required");
            }

            usuario.Senha = PasswordService.IsHashed(usuario.Senha)
                ? usuario.Senha
                : PasswordService.HashPassword(usuario.Senha);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.UsuarioId }, SanitizeUsuario(usuario));
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == id && u.EmpresaId == empresaId.Value);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }

        private static Usuario SanitizeUsuario(Usuario source)
        {
            return new Usuario
            {
                UsuarioId = source.UsuarioId,
                NomeUsuario = source.NomeUsuario,
                Email = source.Email,
                NivelAcesso = source.NivelAcesso,
                EmpresaId = source.EmpresaId,
                Senha = string.Empty,
            };
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
}
