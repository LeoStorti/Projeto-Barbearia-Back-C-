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
    public class ClientesController : ControllerBase
    {
        private readonly APIDbContext _context;
        private readonly ILogger<ClientesController> _logger;
        private readonly IWebHostEnvironment _env;

        public ClientesController(APIDbContext context, ILogger<ClientesController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return cliente;
        }

        // PUT: api/Clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.ClienteId)
            {
                return BadRequest();
            }

            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(id))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException ex)
            {
                var correlationId = Request.Headers["X-Correlation-Id"].ToString();
                _logger.LogError(ex, "Erro ao atualizar cliente {ClienteId}. CorrelationId={CorrelationId}", id, correlationId);

                var pd = new ProblemDetails
                {
                    Title = "Erro ao atualizar cliente",
                    Detail = _env.IsDevelopment() ? (ex.InnerException?.Message ?? ex.Message) : null,
                    Status = 500,
                    Instance = HttpContext.Request.Path
                };
                pd.Extensions["correlationId"] = correlationId;
                pd.Extensions["traceId"] = HttpContext.TraceIdentifier;

                return StatusCode(500, pd);
            }
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            try
            {
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCliente", new { id = cliente.ClienteId }, cliente);
            }
            catch (DbUpdateException ex)
            {
                var correlationId = Request.Headers["X-Correlation-Id"].ToString();
                _logger.LogError(ex, "Erro ao inserir cliente. CorrelationId={CorrelationId}. Payload={@Cliente}", correlationId, cliente);

                var pd = new ProblemDetails
                {
                    Title = "Erro ao salvar cliente",
                    Detail = _env.IsDevelopment() ? (ex.InnerException?.Message ?? ex.Message) : null,
                    Status = 500,
                    Instance = HttpContext.Request.Path
                };
                pd.Extensions["correlationId"] = correlationId;
                pd.Extensions["traceId"] = HttpContext.TraceIdentifier;

                return StatusCode(500, pd);
            }
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            _context.Clientes.Remove(cliente);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                var correlationId = Request.Headers["X-Correlation-Id"].ToString();
                _logger.LogError(ex, "Erro ao excluir cliente {ClienteId}. CorrelationId={CorrelationId}", id, correlationId);

                var pd = new ProblemDetails
                {
                    Title = "Erro ao excluir cliente",
                    Detail = _env.IsDevelopment() ? (ex.InnerException?.Message ?? ex.Message) : null,
                    Status = 500,
                    Instance = HttpContext.Request.Path
                };
                pd.Extensions["correlationId"] = correlationId;
                pd.Extensions["traceId"] = HttpContext.TraceIdentifier;

                return StatusCode(500, pd);
            }
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.ClienteId == id);
        }
    }
}