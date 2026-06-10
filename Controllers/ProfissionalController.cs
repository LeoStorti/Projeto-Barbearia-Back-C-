using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IO;
using Microsoft.EntityFrameworkCore;
using APIBarbearia.Models;
using API.Context;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfissionaisController : ControllerBase
    {
        private readonly APIDbContext _context;

        public ProfissionaisController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Profissionais
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profissional>>> GetProfissionais()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var profissionais = await _context.Profissionais
                .Where(p => p.EmpresaId == empresaId.Value)
                .ToListAsync();

            foreach (var p in profissionais)
            {
                p.FotoUrl = NormalizeExistingFotoUrl(p.FotoUrl);
            }

            return profissionais;
        }

        // GET: api/Profissionais/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Profissional>> GetProfissional(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var profissional = await _context.Profissionais
                .FirstOrDefaultAsync(p => p.ProfissionalId == id && p.EmpresaId == empresaId.Value);

            if (profissional == null)
            {
                return NotFound();
            }

            profissional.FotoUrl = NormalizeExistingFotoUrl(profissional.FotoUrl);

            return profissional;
        }

        // PUT: api/Profissionais/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfissional(int id, Profissional profissional)
        {
            if (id != profissional.ProfissionalId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var existing = await _context.Profissionais
                .FirstOrDefaultAsync(p => p.ProfissionalId == id && p.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Nome = profissional.Nome;
            existing.Especializacao = profissional.Especializacao;
            existing.Telefone = profissional.Telefone;
            existing.Salario = profissional.Salario;
            existing.Email = profissional.Email;
            existing.FotoUrl = profissional.FotoUrl;
            existing.FotoAtualizadaEm = profissional.FotoAtualizadaEm;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfissionalExists(id))
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

        // POST: api/Profissionais
        [HttpPost]
        public async Task<ActionResult<Profissional>> PostProfissional(Profissional profissional)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            profissional.EmpresaId = empresaId.Value;

            if (profissional.ProfissionalId <= 0)
            {
                var maxId = await _context.Profissionais
                    .Select(p => (int?)p.ProfissionalId)
                    .MaxAsync() ?? 0;

                profissional.ProfissionalId = maxId + 1;
            }

            _context.Profissionais.Add(profissional);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProfissional", new { id = profissional.ProfissionalId }, profissional);
        }

        // DELETE: api/Profissionais/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfissional(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var profissional = await _context.Profissionais
                .FirstOrDefaultAsync(p => p.ProfissionalId == id && p.EmpresaId == empresaId.Value);
            if (profissional == null)
            {
                return NotFound();
            }

            _context.Profissionais.Remove(profissional);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfissionalExists(int id)
        {
            return _context.Profissionais.Any(e => e.ProfissionalId == id);
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

        private static string? NormalizeExistingFotoUrl(string? fotoUrl)
        {
            if (string.IsNullOrWhiteSpace(fotoUrl))
            {
                return null;
            }

            var sanitized = fotoUrl.Split('?')[0].Trim();
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                return null;
            }

            if (sanitized.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase)
                || sanitized.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
            {
                return sanitized;
            }

            var relative = sanitized;
            if (relative.StartsWith("/uploads/", System.StringComparison.OrdinalIgnoreCase))
            {
                relative = relative.Substring("/uploads/".Length);
            }
            else if (relative.StartsWith("uploads/", System.StringComparison.OrdinalIgnoreCase))
            {
                relative = relative.Substring("uploads/".Length);
            }
            else
            {
                return sanitized;
            }

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            var filePath = Path.Combine(uploadsRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            return System.IO.File.Exists(filePath) ? sanitized : null;
        }
    }
}
