using API.Context;
using APIBarbearia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItensVendaController : ControllerBase
    {
        private readonly APIDbContext _context;

        public ItensVendaController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/ItensVenda
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItensVenda>>> GetItensVenda()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            return await _context.ItensVenda
                .Include(i => i.Produto)
                .Where(i => i.Produto != null && i.Produto.EmpresaId == empresaId.Value)
                .ToListAsync();
        }

        // GET: api/ItensVenda/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItensVenda>> GetItensVenda(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var itensVenda = await _context.ItensVenda.Include(i => i.Produto)
                                                      .FirstOrDefaultAsync(i => i.ItemVendaId == id && i.Produto != null && i.Produto.EmpresaId == empresaId.Value);

            if (itensVenda == null)
            {
                return NotFound();
            }

            return itensVenda;
        }

        // POST: api/ItensVenda
        [HttpPost]
        public async Task<ActionResult<ItensVenda>> PostItensVenda(ItensVenda itensVenda)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var produtoValido = await _context.Produtos.AnyAsync(p => p.ProdutoId == itensVenda.ProdutoId && p.EmpresaId == empresaId.Value);
            if (!produtoValido)
            {
                return BadRequest("Produto inválido para a empresa autenticada.");
            }

            _context.ItensVenda.Add(itensVenda);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItensVenda), new { id = itensVenda.ItemVendaId }, itensVenda);
        }

        // PUT: api/ItensVenda/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItensVenda(int id, ItensVenda itensVenda)
        {
            if (id != itensVenda.ItemVendaId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var existing = await _context.ItensVenda
                .Include(i => i.Produto)
                .FirstOrDefaultAsync(i => i.ItemVendaId == id && i.Produto != null && i.Produto.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            var produtoValido = await _context.Produtos.AnyAsync(p => p.ProdutoId == itensVenda.ProdutoId && p.EmpresaId == empresaId.Value);
            if (!produtoValido)
            {
                return BadRequest("Produto inválido para a empresa autenticada.");
            }

            existing.ProdutoId = itensVenda.ProdutoId;
            existing.Quantidade = itensVenda.Quantidade;
            existing.PrecoUnitario = itensVenda.PrecoUnitario;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItensVendaExists(id))
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

        // DELETE: api/ItensVenda/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItensVenda(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var itensVenda = await _context.ItensVenda
                .Include(i => i.Produto)
                .FirstOrDefaultAsync(i => i.ItemVendaId == id && i.Produto != null && i.Produto.EmpresaId == empresaId.Value);
            if (itensVenda == null)
            {
                return NotFound();
            }

            _context.ItensVenda.Remove(itensVenda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItensVendaExists(int id)
        {
            return _context.ItensVenda.Any(e => e.ItemVendaId == id);
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
