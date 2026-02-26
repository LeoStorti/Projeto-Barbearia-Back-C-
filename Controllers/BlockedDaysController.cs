using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using API.Context;
using APIBarbearia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBarbearia.Controllers
{
    [ApiController]
    [Route("api/shops/{shopId:int}/blocked-days")]
    public class BlockedDaysController : ControllerBase
    {
        private readonly APIDbContext _context;

        public BlockedDaysController(APIDbContext context)
        {
            _context = context;
        }

        public class BlockedDayUpsertRequest
        {
            public string Date { get; set; } = string.Empty; // yyyy-MM-dd
            public string? Reason { get; set; }
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check(int shopId, [FromQuery] string date)
        {
            if (!TryParseDateOnly(date, out var parsedDate))
            {
                return BadRequest(new { message = "Parâmetro 'date' inválido. Use yyyy-MM-dd." });
            }

            var day = await _context.DiasBloqueados
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.EmpresaId == shopId && d.Data == parsedDate);

            if (day == null)
            {
                return Ok(new { blocked = false, blockedDay = (object?)null });
            }

            return Ok(new
            {
                blocked = true,
                blockedDay = new
                {
                    id = day.DiaBloqueadoId,
                    date = day.Data.ToString("yyyy-MM-dd"),
                    reason = day.Motivo
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(int shopId, [FromBody] BlockedDayUpsertRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Date))
            {
                return BadRequest(new { message = "Body inválido. Informe 'date'." });
            }

            if (!TryParseDateOnly(request.Date, out var parsedDate))
            {
                return BadRequest(new { message = "Campo 'date' inválido. Use yyyy-MM-dd." });
            }

            var motivo = (request.Reason ?? string.Empty).Trim();

            var existing = await _context.DiasBloqueados
                .FirstOrDefaultAsync(d => d.EmpresaId == shopId && d.Data == parsedDate);

            if (existing == null)
            {
                var created = new DiaBloqueado
                {
                    EmpresaId = shopId,
                    Data = parsedDate,
                    Motivo = motivo
                };

                _context.DiasBloqueados.Add(created);
                await _context.SaveChangesAsync();

                return Ok(new { id = created.DiaBloqueadoId, date = created.Data.ToString("yyyy-MM-dd"), reason = created.Motivo });
            }

            existing.Motivo = motivo;
            await _context.SaveChangesAsync();

            return Ok(new { id = existing.DiaBloqueadoId, date = existing.Data.ToString("yyyy-MM-dd"), reason = existing.Motivo });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int shopId, int id)
        {
            var existing = await _context.DiasBloqueados
                .FirstOrDefaultAsync(d => d.DiaBloqueadoId == id && d.EmpresaId == shopId);

            if (existing == null)
            {
                return NotFound();
            }

            _context.DiasBloqueados.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static bool TryParseDateOnly(string date, out DateTime parsed)
        {
            // Salva como DateTime com hora 00:00:00 para facilitar igualdade no banco.
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                parsed = dt.Date;
                return true;
            }

            // fallback tolerante
            if (DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
            {
                parsed = dt.Date;
                return true;
            }

            parsed = default;
            return false;
        }
    }
}
