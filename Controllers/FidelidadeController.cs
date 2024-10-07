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
            return await _context.Fidelidade
                .Include(f => f.Cliente)
                .ToListAsync();
        }

        // GET: api/Fidelidades/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Fidelidade>> GetFidelidade(int id)
        {
            var fidelidade = await _context.Fidelidade
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.FidelidadeId == id);

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

            _context.Entry(fidelidade).State = EntityState.Modified;

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
            var fidelidade = await _context.Fidelidade.FindAsync(id);
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
    }
}
