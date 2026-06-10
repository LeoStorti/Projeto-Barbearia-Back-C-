using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using APIBarbearia.Models;
using API.Context;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FidelidadesController : ControllerBase
    {
        private readonly APIDbContext _context;

        public FidelidadesController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Fidelidades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fidelidade>>> GetFidelidades()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            return await _context.Fidelidade
                .Include(f => f.Cliente)
                .Where(f => f.Cliente != null && f.Cliente.EmpresaId == empresaId.Value)
                .ToListAsync();
        }

        // GET: api/Fidelidades/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Fidelidade>> GetFidelidade(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var fidelidade = await _context.Fidelidade
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.FidelidadeId == id && f.Cliente != null && f.Cliente.EmpresaId == empresaId.Value);

            if (fidelidade == null)
            {
                return NotFound();
            }

            return fidelidade;
        }

        // POST: api/Fidelidades
        [HttpPost]
        public async Task<ActionResult<Fidelidade>> PostFidelidade(Fidelidade fidelidade)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var clienteValido = await _context.Clientes.AnyAsync(c => c.ClienteId == fidelidade.ClienteId && c.EmpresaId == empresaId.Value);
            if (!clienteValido)
            {
                return BadRequest("Cliente inválido para a empresa autenticada.");
            }

            _context.Fidelidade.Add(fidelidade);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFidelidade", new { id = fidelidade.FidelidadeId }, fidelidade);
        }

        // PUT: api/Fidelidades/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFidelidade(int id, Fidelidade fidelidade)
        {
            if (id != fidelidade.FidelidadeId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var existing = await _context.Fidelidade
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.FidelidadeId == id && f.Cliente != null && f.Cliente.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            existing.ClienteId = fidelidade.ClienteId;
            existing.PontosAcumulados = fidelidade.PontosAcumulados;
            existing.DataAtualizacao = fidelidade.DataAtualizacao;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FidelidadeExists(id))
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

        // DELETE: api/Fidelidades/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFidelidade(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var fidelidade = await _context.Fidelidade
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.FidelidadeId == id && f.Cliente != null && f.Cliente.EmpresaId == empresaId.Value);
            if (fidelidade == null)
            {
                return NotFound();
            }

            _context.Fidelidade.Remove(fidelidade);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FidelidadeExists(int id)
        {
            return _context.Fidelidade.Any(e => e.FidelidadeId == id);
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

            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => (int?)u.EmpresaId)
                .FirstOrDefaultAsync();
        }
    }
}
