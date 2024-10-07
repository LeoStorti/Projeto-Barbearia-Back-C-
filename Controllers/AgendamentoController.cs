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
    public class AgendamentosController : ControllerBase
    {
        private readonly APIDbContext _context;

        public AgendamentosController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Agendamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Agendamento>>> GetAgendamentos()
        {
            return await _context.Agendamentos
                .Include(a => a.Cliente)       // Inclui a entidade Cliente relacionada
                .Include(a => a.Profissional)  // Inclui a entidade Profissional relacionada
                .Include(a => a.Servico)       // Inclui a entidade Servico relacionada
                .ToListAsync();
        }

        // GET: api/Agendamentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Agendamento>> GetAgendamento(int id)
        {
            var agendamento = await _context.Agendamentos
                .Include(a => a.Cliente)       // Inclui a entidade Cliente relacionada
                .Include(a => a.Profissional)  // Inclui a entidade Profissional relacionada
                .Include(a => a.Servico)       // Inclui a entidade Servico relacionada
                .FirstOrDefaultAsync(a => a.AgendamentoId == id);

            if (agendamento == null)
            {
                return NotFound();
            }

            return agendamento;
        }


        // POST: api/Agendamentos
        [HttpPost]
        public async Task<ActionResult<Agendamento>> PostAgendamento(Agendamento agendamento)
        {
            _context.Agendamentos.Add(agendamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAgendamento", new { id = agendamento.AgendamentoId }, agendamento);
        }

        // PUT: api/Agendamentos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAgendamento(int id, Agendamento agendamento)
        {
            if (id != agendamento.AgendamentoId)
            {
                return BadRequest();
            }

            _context.Entry(agendamento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AgendamentoExists(id))
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

        // DELETE: api/Agendamentos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgendamento(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento == null)
            {
                return NotFound();
            }

            _context.Agendamentos.Remove(agendamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AgendamentoExists(int id)
        {
            return _context.Agendamentos.Any(e => e.AgendamentoId == id);
        }
    }
}
