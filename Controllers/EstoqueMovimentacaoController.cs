using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using APIBarbearia.Models;
using API.Context;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return await _context.EstoqueMovimentacao
                .Include(e => e.Produto)
                .ToListAsync();
        }

        // GET: api/EstoqueMovimentacoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EstoqueMovimentacao>> GetEstoqueMovimentacao(int id)
        {
            var estoqueMovimentacao = await _context.EstoqueMovimentacao
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.MovimentacaoId == id);

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

            _context.Entry(estoqueMovimentacao).State = EntityState.Modified;

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
            var estoqueMovimentacao = await _context.EstoqueMovimentacao.FindAsync(id);
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
    }
}
