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
    public class NivelAcessoController : ControllerBase
    {
        private readonly APIDbContext _context;

        public NivelAcessoController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/NivelAcesso
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NivelAcesso>>> GetNivelAcessos()
        {
            return await _context.NivelAcesso.ToListAsync();
        }

        // GET: api/NivelAcesso/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NivelAcesso>> GetNivelAcesso(int id)
        {
            var nivelAcesso = await _context.NivelAcesso.FindAsync(id);

            if (nivelAcesso == null)
            {
                return NotFound();
            }

            return nivelAcesso;
        }

        // PUT: api/NivelAcesso/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNivelAcesso(int id, NivelAcesso nivelAcesso)
        {
            if (id != nivelAcesso.NivelAcessoId)
            {
                return BadRequest();
            }

            _context.Entry(nivelAcesso).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NivelAcessoExists(id))
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

        // POST: api/NivelAcesso
        [HttpPost]
        public async Task<ActionResult<NivelAcesso>> PostNivelAcesso(NivelAcesso nivelAcesso)
        {
            _context.NivelAcesso.Add(nivelAcesso);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNivelAcesso", new { id = nivelAcesso.NivelAcessoId }, nivelAcesso);
        }

        // DELETE: api/NivelAcesso/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNivelAcesso(int id)
        {
            var nivelAcesso = await _context.NivelAcesso.FindAsync(id);
            if (nivelAcesso == null)
            {
                return NotFound();
            }

            _context.NivelAcesso.Remove(nivelAcesso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NivelAcessoExists(int id)
        {
            return _context.NivelAcesso.Any(e => e.NivelAcessoId == id);
        }
    }
}
