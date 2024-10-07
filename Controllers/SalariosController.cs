// Controllers/SalariosController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBarbearia.Models;
using API.Context;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalariosController : ControllerBase
    {
        private readonly APIDbContext _context;

        public SalariosController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Salarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Salario>>> GetSalarios()
        {
            return await _context.Salarios.Include(s => s.Profissional).ToListAsync();
        }

        // GET: api/Salarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Salario>> GetSalario(int id)
        {
            var salario = await _context.Salarios.Include(s => s.Profissional).FirstOrDefaultAsync(s => s.SalarioId == id);

            if (salario == null)
            {
                return NotFound();
            }

            return salario;
        }

        // POST: api/Salarios
        [HttpPost]
        public async Task<ActionResult<Salario>> PostSalario(Salario salario)
        {
            // Validação: verificar se já existe um salário ativo para o profissional
            var salarioExistente = await _context.Salarios
                .Where(s => s.ProfissionalId == salario.ProfissionalId && s.DataFim == null)
                .FirstOrDefaultAsync();

            if (salarioExistente != null)
            {
                return BadRequest("Já existe um salário ativo para este profissional.");
            }

            _context.Salarios.Add(salario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalario), new { id = salario.SalarioId }, salario);
        }

        // PUT: api/Salarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSalario(int id, Salario salario)
        {
            if (id != salario.SalarioId)
            {
                return BadRequest();
            }

            _context.Entry(salario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalarioExists(id))
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

        // DELETE: api/Salarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalario(int id)
        {
            var salario = await _context.Salarios.FindAsync(id);
            if (salario == null)
            {
                return NotFound();
            }

            _context.Salarios.Remove(salario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SalarioExists(int id)
        {
            return _context.Salarios.Any(e => e.SalarioId == id);
        }
    }
}
