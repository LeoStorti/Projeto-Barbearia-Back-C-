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
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            return await _context.Agendamentos
                .Include(a => a.Cliente)       // Inclui a entidade Cliente relacionada
                .Include(a => a.Profissional)  // Inclui a entidade Profissional relacionada
                .Include(a => a.Servico)       // Inclui a entidade Servico relacionada
                .Where(a => a.Profissional != null && a.Profissional.EmpresaId == empresaId.Value)
                .ToListAsync();
        }

        // GET: api/Agendamentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Agendamento>> GetAgendamento(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var agendamento = await _context.Agendamentos
                .Include(a => a.Cliente)       // Inclui a entidade Cliente relacionada
                .Include(a => a.Profissional)  // Inclui a entidade Profissional relacionada
                .Include(a => a.Servico)       // Inclui a entidade Servico relacionada
                .FirstOrDefaultAsync(a => a.AgendamentoId == id && a.Profissional != null && a.Profissional.EmpresaId == empresaId.Value);

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
            if (agendamento == null)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            // Compat: caso o cliente envie apenas as entidades relacionadas, copiar os IDs.
            if (agendamento.ClienteId <= 0 && agendamento.Cliente?.ClienteId > 0)
            {
                agendamento.ClienteId = agendamento.Cliente.ClienteId;
            }
            if (agendamento.ProfissionalId <= 0 && agendamento.Profissional?.ProfissionalId > 0)
            {
                agendamento.ProfissionalId = agendamento.Profissional.ProfissionalId;
            }
            if (agendamento.ServicoId <= 0 && agendamento.Servico?.ServicoId > 0)
            {
                agendamento.ServicoId = agendamento.Servico.ServicoId;
            }

            // NÃO rastrear o grafo recebido (evita INSERT em tabelas relacionadas e PK duplicada).
            agendamento.Cliente = null;
            agendamento.Profissional = null;
            agendamento.Servico = null;

            // Default de status
            agendamento.Status ??= "Pendente";

            // Validações rápidas (melhor erro do que 500/constraint)
            if (agendamento.ClienteId <= 0 || agendamento.ProfissionalId <= 0 || agendamento.ServicoId <= 0)
            {
                return BadRequest("ClienteId, ProfissionalId e ServicoId são obrigatórios.");
            }

            var clienteExiste = await _context.Clientes.AnyAsync(c => c.ClienteId == agendamento.ClienteId && c.EmpresaId == empresaId.Value);
            if (!clienteExiste)
            {
                return BadRequest("Cliente inválido para a empresa autenticada.");
            }

            var profissionalExiste = await _context.Profissionais.AnyAsync(p => p.ProfissionalId == agendamento.ProfissionalId && p.EmpresaId == empresaId.Value);
            if (!profissionalExiste)
            {
                return BadRequest("Profissional inválido para a empresa autenticada.");
            }

            var servicoExiste = await _context.Servicos.AnyAsync(s => s.ServicoId == agendamento.ServicoId && s.EmpresaId == empresaId.Value);
            if (!servicoExiste)
            {
                return BadRequest("Serviço inválido para a empresa autenticada.");
            }

            // Impede conflito no mesmo minuto para o mesmo profissional.
            var inicioSlot = new DateTime(
                agendamento.DataHora.Year,
                agendamento.DataHora.Month,
                agendamento.DataHora.Day,
                agendamento.DataHora.Hour,
                agendamento.DataHora.Minute,
                0,
                agendamento.DataHora.Kind);
            var fimSlot = inicioSlot.AddMinutes(1);

            var candidatosConflito = await _context.Agendamentos
                .AsNoTracking()
                .Where(a =>
                a.ProfissionalId == agendamento.ProfissionalId &&
                a.DataHora >= inicioSlot &&
                a.DataHora < fimSlot)
                .ToListAsync();

            var conflito = candidatosConflito.Any(a => !IsStatusCancelado(a.Status));

            if (conflito)
            {
                return Conflict("Horário indisponível para este profissional.");
            }

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

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            // Mesma proteção do POST: evitar que o EF tente inserir/atualizar entidades relacionadas.
            if (agendamento.ClienteId <= 0 && agendamento.Cliente?.ClienteId > 0)
            {
                agendamento.ClienteId = agendamento.Cliente.ClienteId;
            }
            if (agendamento.ProfissionalId <= 0 && agendamento.Profissional?.ProfissionalId > 0)
            {
                agendamento.ProfissionalId = agendamento.Profissional.ProfissionalId;
            }
            if (agendamento.ServicoId <= 0 && agendamento.Servico?.ServicoId > 0)
            {
                agendamento.ServicoId = agendamento.Servico.ServicoId;
            }
            agendamento.Cliente = null;
            agendamento.Profissional = null;
            agendamento.Servico = null;

            var existing = await _context.Agendamentos
                .Include(a => a.Profissional)
                .FirstOrDefaultAsync(a => a.AgendamentoId == id && a.Profissional != null && a.Profissional.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            var clienteValido = await _context.Clientes.AnyAsync(c => c.ClienteId == agendamento.ClienteId && c.EmpresaId == empresaId.Value);
            var profissionalValido = await _context.Profissionais.AnyAsync(p => p.ProfissionalId == agendamento.ProfissionalId && p.EmpresaId == empresaId.Value);
            var servicoValido = await _context.Servicos.AnyAsync(s => s.ServicoId == agendamento.ServicoId && s.EmpresaId == empresaId.Value);
            if (!clienteValido || !profissionalValido || !servicoValido)
            {
                return BadRequest("Entidades relacionadas inválidas para a empresa autenticada.");
            }

            // Impede conflito com outro agendamento no mesmo minuto para o mesmo profissional.
            var inicioSlot = new DateTime(
                agendamento.DataHora.Year,
                agendamento.DataHora.Month,
                agendamento.DataHora.Day,
                agendamento.DataHora.Hour,
                agendamento.DataHora.Minute,
                0,
                agendamento.DataHora.Kind);
            var fimSlot = inicioSlot.AddMinutes(1);

            var candidatosConflito = await _context.Agendamentos
                .AsNoTracking()
                .Where(a =>
                a.AgendamentoId != id &&
                a.ProfissionalId == agendamento.ProfissionalId &&
                a.DataHora >= inicioSlot &&
                a.DataHora < fimSlot)
                .ToListAsync();

            var conflito = candidatosConflito.Any(a => !IsStatusCancelado(a.Status));

            if (conflito)
            {
                return Conflict("Horário indisponível para este profissional.");
            }

            existing.ClienteId = agendamento.ClienteId;
            existing.ProfissionalId = agendamento.ProfissionalId;
            existing.ServicoId = agendamento.ServicoId;
            existing.DataHora = agendamento.DataHora;
            existing.Status = agendamento.Status;
            existing.Observacoes = agendamento.Observacoes;

            _context.Entry(existing).State = EntityState.Modified;

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
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var agendamento = await _context.Agendamentos
                .Include(a => a.Profissional)
                .FirstOrDefaultAsync(a => a.AgendamentoId == id && a.Profissional != null && a.Profissional.EmpresaId == empresaId.Value);
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

        private static bool IsStatusCancelado(string? status)
        {
            var st = (status ?? string.Empty).Trim().ToLowerInvariant();
            return st == "cancelado" || st == "canceled" || st == "cancelled";
        }
    }
}
