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
    public class FeedbacksController : ControllerBase
    {
        private readonly APIDbContext _context;

        public FeedbacksController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Feedbacks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedbacks()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            return await _context.Feedbacks
                .Include(f => f.Cliente)
                .Include(f => f.Servico)
                .Include(f => f.Profissional)
                .Where(f => f.Profissional != null && f.Profissional.EmpresaId == empresaId.Value)
                .ToListAsync();
        }

        // GET: api/Feedbacks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Feedback>> GetFeedback(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var feedback = await _context.Feedbacks
                .Include(f => f.Cliente)
                .Include(f => f.Servico)
                .Include(f => f.Profissional)
                .FirstOrDefaultAsync(f => f.FeedbackId == id && f.Profissional != null && f.Profissional.EmpresaId == empresaId.Value);

            if (feedback == null)
            {
                return NotFound();
            }

            return feedback;
        }

        // POST: api/Feedbacks
        [HttpPost]
        public async Task<ActionResult<Feedback>> PostFeedback(Feedback feedback)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var profissionalValido = await _context.Profissionais.AnyAsync(p => p.ProfissionalId == feedback.ProfissionalId && p.EmpresaId == empresaId.Value);
            var servicoValido = await _context.Servicos.AnyAsync(s => s.ServicoId == feedback.ServicoId && s.EmpresaId == empresaId.Value);
            var clienteValido = await _context.Clientes.AnyAsync(c => c.ClienteId == feedback.ClienteId && c.EmpresaId == empresaId.Value);
            if (!profissionalValido || !servicoValido || !clienteValido)
            {
                return BadRequest("Entidades relacionadas inválidas para a empresa autenticada.");
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFeedback", new { id = feedback.FeedbackId }, feedback);
        }

        // PUT: api/Feedbacks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFeedback(int id, Feedback feedback)
        {
            if (id != feedback.FeedbackId)
            {
                return BadRequest();
            }

            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var existing = await _context.Feedbacks
                .Include(f => f.Profissional)
                .FirstOrDefaultAsync(f => f.FeedbackId == id && f.Profissional != null && f.Profissional.EmpresaId == empresaId.Value);
            if (existing == null)
            {
                return NotFound();
            }

            existing.ClienteId = feedback.ClienteId;
            existing.ProfissionalId = feedback.ProfissionalId;
            existing.ServicoId = feedback.ServicoId;
            existing.Avaliacao = feedback.Avaliacao;
            existing.Comentario = feedback.Comentario;
            existing.DataFeedback = feedback.DataFeedback;

            _context.Entry(existing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(id))
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

        // DELETE: api/Feedbacks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (!empresaId.HasValue) return Forbid();

            var feedback = await _context.Feedbacks
                .Include(f => f.Profissional)
                .FirstOrDefaultAsync(f => f.FeedbackId == id && f.Profissional != null && f.Profissional.EmpresaId == empresaId.Value);
            if (feedback == null)
            {
                return NotFound();
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackId == id);
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

            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => (int?)u.EmpresaId)
                .FirstOrDefaultAsync();
        }
    }
}
