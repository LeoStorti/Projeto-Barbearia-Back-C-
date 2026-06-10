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
    public class PagamentosController : ControllerBase
    {
        private readonly APIDbContext _context;

        public PagamentosController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Pagamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pagamento>>> GetPagamentos()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            return await _context.Pagamentos
                .Include(p => p.Venda)
                .Where(p => _context.Vendas.Any(v => v.VendaId == p.VendaId && v.ProfissionalId.HasValue && _context.Profissionais.Any(pr => pr.ProfissionalId == v.ProfissionalId.Value && pr.EmpresaId == empresaId.Value)))
                .ToListAsync();
        }

        // GET: api/Pagamentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pagamento>> GetPagamento(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var pagamento = await _context.Pagamentos
                .Include(p => p.Venda)
                .FirstOrDefaultAsync(p => p.PagamentoId == id
                    && _context.Vendas.Any(v => v.VendaId == p.VendaId && v.ProfissionalId.HasValue && _context.Profissionais.Any(pr => pr.ProfissionalId == v.ProfissionalId.Value && pr.EmpresaId == empresaId.Value)));

            if (pagamento == null)
            {
                return NotFound();
            }

            return pagamento;
        }

        // POST: api/Pagamentos
        [HttpPost]
        public async Task<ActionResult<Pagamento>> PostPagamento(Pagamento pagamento)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var vendaValida = await _context.Vendas
                .AnyAsync(v => v.VendaId == pagamento.VendaId
                    && v.ProfissionalId.HasValue
                    && _context.Profissionais.Any(pr => pr.ProfissionalId == v.ProfissionalId.Value && pr.EmpresaId == empresaId.Value));
            if (!vendaValida)
            {
                return BadRequest("Venda inválida para a empresa autenticada.");
            }

            _context.Pagamentos.Add(pagamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPagamento", new { id = pagamento.PagamentoId }, pagamento);
        }

        // PUT: api/Pagamentos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPagamento(int id, Pagamento pagamento)
        {
            if (id != pagamento.PagamentoId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var existing = await _context.Pagamentos
                .FirstOrDefaultAsync(p => p.PagamentoId == id
                    && _context.Vendas.Any(v => v.VendaId == p.VendaId && v.ProfissionalId.HasValue && _context.Profissionais.Any(pr => pr.ProfissionalId == v.ProfissionalId.Value && pr.EmpresaId == empresaId.Value)));
            if (existing == null)
            {
                return NotFound();
            }

            var vendaValida = await _context.Vendas
                .AnyAsync(v => v.VendaId == pagamento.VendaId
                    && v.ProfissionalId.HasValue
                    && _context.Profissionais.Any(pr => pr.ProfissionalId == v.ProfissionalId.Value && pr.EmpresaId == empresaId.Value));
            if (!vendaValida)
            {
                return BadRequest("Venda inválida para a empresa autenticada.");
            }

            existing.VendaId = pagamento.VendaId;
            existing.ValorPago = pagamento.ValorPago;
            existing.DataPagamento = pagamento.DataPagamento;
            existing.FormaPagamento = pagamento.FormaPagamento;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PagamentoExists(id))
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

        // DELETE: api/Pagamentos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePagamento(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var pagamento = await _context.Pagamentos
                .FirstOrDefaultAsync(p => p.PagamentoId == id
                    && _context.Vendas.Any(v => v.VendaId == p.VendaId && v.ProfissionalId.HasValue && _context.Profissionais.Any(pr => pr.ProfissionalId == v.ProfissionalId.Value && pr.EmpresaId == empresaId.Value)));
            if (pagamento == null)
            {
                return NotFound();
            }

            _context.Pagamentos.Remove(pagamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PagamentoExists(int id)
        {
            return _context.Pagamentos.Any(e => e.PagamentoId == id);
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
