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
    public class EmpresaController : ControllerBase
    {
        private readonly APIDbContext _context;

        public EmpresaController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Empresa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
        {
            var current = await GetCurrentUserContextAsync();
            if (current == null)
            {
                return Unauthorized();
            }

            if (!current.EmpresaId.HasValue)
            {
                return Forbid();
            }

            var empresa = await _context.Empresas
                .Where(e => e.EmpresaId == current.EmpresaId.Value)
                .ToListAsync();

            return empresa;
        }

        // GET: api/Empresa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Empresa>> GetEmpresa(int id)
        {
            var current = await GetCurrentUserContextAsync();
            if (current == null)
            {
                return Unauthorized();
            }

            if (current.EmpresaId != id)
            {
                return Forbid();
            }

            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound();
            }

            return empresa;
        }

        // PUT: api/Empresa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmpresa(int id, Empresa empresa)
        {
            if (id != empresa.EmpresaId)
            {
                return BadRequest();
            }

            var current = await GetCurrentUserContextAsync();
            if (current == null)
            {
                return Unauthorized();
            }

            if (current.EmpresaId != id)
            {
                return Forbid();
            }

            _context.Entry(empresa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmpresaExists(id))
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

        // POST: api/Empresa
        [HttpPost]
        public ActionResult<Empresa> PostEmpresa(Empresa empresa)
        {
            return Forbid();
        }

        // DELETE: api/Empresa/5
        [HttpDelete("{id}")]
        public IActionResult DeleteEmpresa(int id)
        {
            return Forbid();
        }

        private bool EmpresaExists(int id)
        {
            return _context.Empresas.Any(e => e.EmpresaId == id);
        }

        private async Task<UserContext?> GetCurrentUserContextAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirst("unique_name")?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var userRow = await _context.Usuarios
                .FromSqlInterpolated($"SELECT * FROM usuarios WHERE Email = {email} LIMIT 1")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (userRow == null)
            {
                return null;
            }

            // Usuario model may not be updated with EmpresaId yet. Read tenant id directly from DB.
            var tenantRow = await _context.Database
                .SqlQuery<TenantLookup>($"SELECT EmpresaId FROM usuarios WHERE Email = {email} LIMIT 1")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return new UserContext
            {
                Email = email,
                EmpresaId = tenantRow?.EmpresaId,
            };
        }

        private sealed class TenantLookup
        {
            public int? EmpresaId { get; set; }
        }

        private sealed class UserContext
        {
            public string Email { get; set; } = string.Empty;
            public int? EmpresaId { get; set; }
        }
    }
}
