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
    public class EstoqueMovimentacoesController : ControllerBase
    {
        private readonly APIDbContext _context;

        public EstoqueMovimentacoesController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/EstoqueMovimentacoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstoqueMovimentacao>>> GetEstoqueMovimentacoes()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            return await _context.EstoqueMovimentacao
                .Include(e => e.Produto)
                .Where(e => e.Produto != null && e.Produto.EmpresaId == empresaId.Value)
                .ToListAsync();
        }

        // GET: api/EstoqueMovimentacoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EstoqueMovimentacao>> GetEstoqueMovimentacao(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var estoqueMovimentacao = await _context.EstoqueMovimentacao
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.MovimentacaoId == id && e.Produto != null && e.Produto.EmpresaId == empresaId.Value);

            if (estoqueMovimentacao == null)
            {
                return NotFound();
            }

            return estoqueMovimentacao;
        }

        // POST: api/EstoqueMovimentacoes
        [HttpPost]
        public async Task<ActionResult<EstoqueMovimentacao>> PostEstoqueMovimentacao(EstoqueMovimentacao estoqueMovimentacao)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var produtoValido = await _context.Produtos.AnyAsync(p => p.ProdutoId == estoqueMovimentacao.ProdutoId && p.EmpresaId == empresaId.Value);
            if (!produtoValido)
            {
                return BadRequest("Produto inválido para a empresa autenticada.");
            }

            _context.EstoqueMovimentacao.Add(estoqueMovimentacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEstoqueMovimentacao", new { id = estoqueMovimentacao.MovimentacaoId }, estoqueMovimentacao);
        }

        // PUT: api/EstoqueMovimentacoes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstoqueMovimentacao(int id, EstoqueMovimentacao estoqueMovimentacao)
        {
            if (id != estoqueMovimentacao.MovimentacaoId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var existing = await _context.EstoqueMovimentacao
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.MovimentacaoId == id && e.Produto != null && e.Produto.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            var produtoValido = await _context.Produtos.AnyAsync(p => p.ProdutoId == estoqueMovimentacao.ProdutoId && p.EmpresaId == empresaId.Value);
            if (!produtoValido)
            {
                return BadRequest("Produto inválido para a empresa autenticada.");
            }

            existing.ProdutoId = estoqueMovimentacao.ProdutoId;
            existing.TipoMovimentacao = estoqueMovimentacao.TipoMovimentacao;
            existing.Quantidade = estoqueMovimentacao.Quantidade;
            existing.DataMovimentacao = estoqueMovimentacao.DataMovimentacao;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstoqueMovimentacaoExists(id))
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

        // DELETE: api/EstoqueMovimentacoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstoqueMovimentacao(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var estoqueMovimentacao = await _context.EstoqueMovimentacao
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.MovimentacaoId == id && e.Produto != null && e.Produto.EmpresaId == empresaId.Value);
            if (estoqueMovimentacao == null)
            {
                return NotFound();
            }

            _context.EstoqueMovimentacao.Remove(estoqueMovimentacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EstoqueMovimentacaoExists(int id)
        {
            return _context.EstoqueMovimentacao.Any(e => e.MovimentacaoId == id);
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
