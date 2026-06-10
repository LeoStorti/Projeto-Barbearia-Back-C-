using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using APIBarbearia.Models;
using API.Context;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServicosController : ControllerBase
    {
        private readonly APIDbContext _context;

        public ServicosController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Servicos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Servico>>> GetServicos()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            return await _context.Servicos
                .Where(s => s.EmpresaId == empresaId.Value)
                .ToListAsync();
        }

        // GET: api/Servicos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Servico>> GetServico(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var servico = await _context.Servicos
                .FirstOrDefaultAsync(s => s.ServicoId == id && s.EmpresaId == empresaId.Value);

            if (servico == null)
            {
                return NotFound();
            }

            return servico;
        }

        // PUT: api/Servicos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServico(int id, Servico servico)
        {
            if (id != servico.ServicoId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var existing = await _context.Servicos
                .FirstOrDefaultAsync(s => s.ServicoId == id && s.EmpresaId == empresaId.Value);

            if (existing == null)
            {
                return NotFound();
            }

            existing.NomeServico = servico.NomeServico;
            existing.Preco = servico.Preco;
            existing.Descricao = servico.Descricao;
            existing.Duracao = servico.Duracao;
            existing.Categoria = servico.Categoria;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicoExists(id))
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

        // POST: api/Servicos
        [HttpPost]
        public async Task<ActionResult<Servico>> PostServico(Servico servico)
        {
            try
            {
                var empresaId = await GetEmpresaIdAsync();
                if (!empresaId.HasValue)
                {
                    return Forbid();
                }

                servico.EmpresaId = empresaId.Value;
                _context.Servicos.Add(servico);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetServico", new { id = servico.ServicoId }, servico);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    title = "Erro ao salvar serviço",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    title = "Erro inesperado ao salvar serviço",
                    detail = ex.Message
                });
            }
        }

        // DELETE: api/Servicos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServico(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var servico = await _context.Servicos
                .FirstOrDefaultAsync(s => s.ServicoId == id && s.EmpresaId == empresaId.Value);
            if (servico == null)
            {
                return NotFound();
            }

            var possuiAgendamentos = await _context.Agendamentos.AnyAsync(a => a.ServicoId == id);
            if (possuiAgendamentos)
            {
                return Conflict("Não é possível excluir o serviço porque ele está vinculado a agendamentos.");
            }

            var possuiVendas = await _context.Vendas.AnyAsync(v => v.ServicoId == id);
            if (possuiVendas)
            {
                return Conflict("Não é possível excluir o serviço porque ele está vinculado a vendas.");
            }

            _context.Servicos.Remove(servico);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mySqlEx && mySqlEx.Number == 1451)
            {
                return Conflict("Não é possível excluir o serviço porque ele possui vínculos em outros registros.");
            }

            return NoContent();
        }

        private bool ServicoExists(int id)
        {
            return _context.Servicos.Any(e => e.ServicoId == id);
        }

        private async Task<int?> GetEmpresaIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirst("unique_name")?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => (int?)u.EmpresaId)
                .FirstOrDefaultAsync();
        }
    }
}
