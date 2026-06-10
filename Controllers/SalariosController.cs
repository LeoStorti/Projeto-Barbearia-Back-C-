// Controllers/SalariosController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBarbearia.Models;
using API.Context;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalariosController : ControllerBase
    {
        private readonly APIDbContext _context;

        public SalariosController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Salarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Salario>>> GetSalarios()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            return await _context.Salarios
                .Include(s => s.Profissional)
                .Where(s => s.Profissional != null && s.Profissional.EmpresaId == empresaId.Value)
                .ToListAsync();
        }

        // GET: api/Salarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Salario>> GetSalario(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var salario = await _context.Salarios
                .Include(s => s.Profissional)
                .FirstOrDefaultAsync(s => s.SalarioId == id && s.Profissional != null && s.Profissional.EmpresaId == empresaId.Value);

            if (salario == null)
            {
                return NotFound();
            }

            return salario;
        }

        // POST: api/Salarios
        [HttpPost]
        public async Task<ActionResult<Salario>> PostSalario(Salario salario)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var profissionalValido = await _context.Profissionais
                .AnyAsync(p => p.ProfissionalId == salario.ProfissionalId && p.EmpresaId == empresaId.Value);
            if (!profissionalValido)
            {
                return BadRequest("Profissional inválido para a empresa autenticada.");
            }

            // Validação: verificar se já existe um salário ativo para o profissional
            var salarioExistente = await _context.Salarios
                .Where(s => s.ProfissionalId == salario.ProfissionalId && s.DataFim == null)
                .FirstOrDefaultAsync();

            if (salarioExistente != null)
            {
                return BadRequest("Já existe um salário ativo para este profissional.");
            }

            _context.Salarios.Add(salario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalario), new { id = salario.SalarioId }, salario);
        }

        // PUT: api/Salarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSalario(int id, Salario salario)
        {
            if (id != salario.SalarioId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var existing = await _context.Salarios
                .Include(s => s.Profissional)
                .FirstOrDefaultAsync(s => s.SalarioId == id && s.Profissional != null && s.Profissional.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            var profissionalValido = await _context.Profissionais
                .AnyAsync(p => p.ProfissionalId == salario.ProfissionalId && p.EmpresaId == empresaId.Value);
            if (!profissionalValido)
            {
                return BadRequest("Profissional inválido para a empresa autenticada.");
            }

            existing.ProfissionalId = salario.ProfissionalId;
            existing.SalarioFixo = salario.SalarioFixo;
            existing.DataInicio = salario.DataInicio;
            existing.DataFim = salario.DataFim;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalarioExists(id))
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

        // DELETE: api/Salarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalario(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var salario = await _context.Salarios
                .Include(s => s.Profissional)
                .FirstOrDefaultAsync(s => s.SalarioId == id && s.Profissional != null && s.Profissional.EmpresaId == empresaId.Value);
            if (salario == null)
            {
                return NotFound();
            }

            _context.Salarios.Remove(salario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SalarioExists(int id)
        {
            return _context.Salarios.Any(e => e.SalarioId == id);
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
