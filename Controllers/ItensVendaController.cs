using API.Context;
using APIBarbearia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return await _context.ItensVenda.Include(i => i.Produto).ToListAsync();
        }

        // GET: api/ItensVenda/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItensVenda>> GetItensVenda(int id)
        {
            var itensVenda = await _context.ItensVenda.Include(i => i.Produto)
                                                      .FirstOrDefaultAsync(i => i.ItemVendaId == id);

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

            _context.Entry(itensVenda).State = EntityState.Modified;

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
            var itensVenda = await _context.ItensVenda.FindAsync(id);
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
    }
}
