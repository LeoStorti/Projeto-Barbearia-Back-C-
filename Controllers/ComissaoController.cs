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
            return await _context.Comissoes
                .Include(c => c.Profissional)
                .Include(c => c.Venda)
                .ToListAsync();
        }

        // GET: api/Comissoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comissao>> GetComissao(int id)
        {
            var comissao = await _context.Comissoes
                .Include(c => c.Profissional)
                .Include(c => c.Venda)
                .FirstOrDefaultAsync(c => c.ComissaoId == id);

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

            _context.Entry(comissao).State = EntityState.Modified;

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
            var comissao = await _context.Comissoes.FindAsync(id);
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
    }
}
