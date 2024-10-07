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
        public async Task<ActionResult<Vendas>> PostVenda(Vendas venda)
        {
            Console.WriteLine("Iniciando o processo de venda");

            // Validação para verificar se o serviço existe
            var servico = await _context.Servicos.FindAsync(venda.ServicoId);
            if (servico == null)
            {
                Console.WriteLine("Serviço não encontrado.");
                return BadRequest("Serviço não encontrado.");
            }

            // Se o valor total da venda não foi informado, definir o valor do serviço
            if (venda.TotalVenda == 0)
            {
                venda.TotalVenda = servico.Preco;
                Console.WriteLine($"Valor da venda definido automaticamente para {venda.TotalVenda}, com base no serviço.");
            }

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();
            Console.WriteLine("Venda salva com sucesso");

            // Validação para verificar se o profissional existe
            var profissional = await _context.Profissionais.FindAsync(venda.ProfissionalId);
            if (profissional == null)
            {
                Console.WriteLine("Profissional não encontrado.");
                return BadRequest("Profissional não encontrado.");
            }

            Console.WriteLine($"Profissional encontrado: {profissional.Nome}");

            // Cálculo da comissão (10% do total da venda)
            decimal valorComissao = venda.TotalVenda * 0.10m;

            // Geração da comissão
            var comissao = new Comissao
            {
                ProfissionalId = (int)venda.ProfissionalId,
                VendaId = venda.VendaId,
                ValorComissao = valorComissao,
                DataComissao = DateTime.Now
            };

            _context.Comissoes.Add(comissao);
            await _context.SaveChangesAsync();

            Console.WriteLine("Comissão gerada com sucesso");

            return CreatedAtAction("GetVenda", new { id = venda.VendaId }, venda);
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
