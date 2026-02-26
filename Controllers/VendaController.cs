using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using APIBarbearia.Models;
using API.Context;
using APIBarbearia.DTOs;

namespace APIBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendaController : ControllerBase
    {
        private readonly APIDbContext _context;

        public VendaController(APIDbContext context)
        {
            _context = context;
        }

        // GET: api/Vendas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vendas>>> GetVendas()
        {
            return await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Servico) // Incluindo os detalhes do serviço na venda
                .ToListAsync();
        }

        // GET: api/Vendas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vendas>> GetVenda(int id)
        {
            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Servico) // Incluindo os detalhes do serviço
                .FirstOrDefaultAsync(v => v.VendaId == id);

            if (venda == null)
            {
                return NotFound();
            }

            return venda;
        }

        // POST: api/Vendas
        [HttpPost]
        public async Task<ActionResult<Vendas>> PostVenda([FromBody] VendaCreateDto venda)
        {
            Console.WriteLine("Iniciando o processo de venda");

            if (venda == null)
            {
                return BadRequest("Payload inválido.");
            }

            if (venda.ClienteId <= 0 || venda.ProfissionalId <= 0 || venda.ServicoId <= 0)
            {
                return BadRequest("ClienteId, ProfissionalId e ServicoId são obrigatórios.");
            }

            // Validações com consultas sem tracking para não conflitar com o attach do grafo.
            var clienteExists = await _context.Clientes.AsNoTracking().AnyAsync(c => c.ClienteId == venda.ClienteId);
            if (!clienteExists)
            {
                return BadRequest("Cliente não encontrado.");
            }

            var profissionalExists = await _context.Profissionais.AsNoTracking().AnyAsync(p => p.ProfissionalId == venda.ProfissionalId);
            if (!profissionalExists)
            {
                return BadRequest("Profissional não encontrado.");
            }

            // Validação para verificar se o serviço existe
            var servicoInfo = await _context.Servicos
                .AsNoTracking()
                .Where(s => s.ServicoId == venda.ServicoId)
                .Select(s => new { s.ServicoId, s.Preco })
                .FirstOrDefaultAsync();

            if (servicoInfo == null)
            {
                Console.WriteLine("Serviço não encontrado.");
                return BadRequest("Serviço não encontrado.");
            }

            // Se o valor total da venda não foi informado, definir o valor do serviço
            var totalVenda = venda.TotalVenda ?? 0m;
            if (totalVenda == 0m)
            {
                totalVenda = servicoInfo.Preco;
                Console.WriteLine($"Valor da venda definido automaticamente para {totalVenda}, com base no serviço.");
            }

            // Persistimos somente FKs (não anexar Cliente/Servico do payload)
            var entity = new Vendas
            {
                DataVenda = venda.DataVenda ?? DateTime.Now,
                ClienteId = venda.ClienteId,
                ProfissionalId = venda.ProfissionalId,
                ServicoId = venda.ServicoId,
                TotalVenda = totalVenda,
                Cliente = null,
                Servico = null
            };

            _context.Vendas.Add(entity);
            await _context.SaveChangesAsync();
            Console.WriteLine("Venda salva com sucesso");

            // Cálculo da comissão (10% do total da venda)
            decimal valorComissao = entity.TotalVenda * 0.10m;

            // Geração da comissão
            var comissao = new Comissao
            {
                ProfissionalId = venda.ProfissionalId,
                VendaId = entity.VendaId,
                ValorComissao = valorComissao,
                DataComissao = DateTime.Now
            };

            _context.Comissoes.Add(comissao);
            await _context.SaveChangesAsync();

            Console.WriteLine("Comissão gerada com sucesso");

            return CreatedAtAction("GetVenda", new { id = entity.VendaId }, entity);
        }


        // PUT: api/Vendas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenda(int id, Vendas venda)
        {
            if (id != venda.VendaId)
            {
                return BadRequest();
            }

            _context.Entry(venda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendaExists(id))
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

        // DELETE: api/Vendas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenda(int id)
        {
            var venda = await _context.Vendas.FindAsync(id);
            if (venda == null)
            {
                return NotFound();
            }

            _context.Vendas.Remove(venda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VendaExists(int id)
        {
            return _context.Vendas.Any(e => e.VendaId == id);
        }
    }
}
