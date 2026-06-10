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
    public class ComissoesController : ControllerBase
    {
        private readonly APIDbContext _context;

        public ComissoesController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Comissoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comissao>>> GetComissoes()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            return await _context.Comissoes
                .Include(c => c.Profissional)
                .Include(c => c.Venda)
                .Where(c => c.Profissional != null && c.Profissional.EmpresaId == empresaId.Value)
                .ToListAsync();
        }

        // GET: api/Comissoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comissao>> GetComissao(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var comissao = await _context.Comissoes
                .Include(c => c.Profissional)
                .Include(c => c.Venda)
                .FirstOrDefaultAsync(c => c.ComissaoId == id && c.Profissional != null && c.Profissional.EmpresaId == empresaId.Value);

            if (comissao == null)
            {
                return NotFound();
            }

            return comissao;
        }

        // POST: api/Comissoes
        [HttpPost]
        public async Task<ActionResult<Comissao>> PostComissao(Comissao comissao)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var profissionalValido = await _context.Profissionais
                .AnyAsync(p => p.ProfissionalId == comissao.ProfissionalId && p.EmpresaId == empresaId.Value);
            if (!profissionalValido)
            {
                return BadRequest("Profissional inválido para a empresa autenticada.");
            }

            _context.Comissoes.Add(comissao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComissao", new { id = comissao.ComissaoId }, comissao);
        }

        // PUT: api/Comissoes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComissao(int id, Comissao comissao)
        {
            if (id != comissao.ComissaoId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var existing = await _context.Comissoes
                .Include(c => c.Profissional)
                .FirstOrDefaultAsync(c => c.ComissaoId == id && c.Profissional != null && c.Profissional.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            var profissionalValido = await _context.Profissionais
                .AnyAsync(p => p.ProfissionalId == comissao.ProfissionalId && p.EmpresaId == empresaId.Value);
            if (!profissionalValido)
            {
                return BadRequest("Profissional inválido para a empresa autenticada.");
            }

            existing.ProfissionalId = comissao.ProfissionalId;
            existing.VendaId = comissao.VendaId;
            existing.ValorComissao = comissao.ValorComissao;
            existing.DataComissao = comissao.DataComissao;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComissaoExists(id))
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

        // DELETE: api/Comissoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComissao(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var comissao = await _context.Comissoes
                .Include(c => c.Profissional)
                .FirstOrDefaultAsync(c => c.ComissaoId == id && c.Profissional != null && c.Profissional.EmpresaId == empresaId.Value);
            if (comissao == null)
            {
                return NotFound();
            }

            _context.Comissoes.Remove(comissao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ComissaoExists(int id)
        {
            return _context.Comissoes.Any(e => e.ComissaoId == id);
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
