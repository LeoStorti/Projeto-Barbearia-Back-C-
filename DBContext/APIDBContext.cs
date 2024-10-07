using APIBarbearia.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Context
{
    public class APIDbContext : DbContext
    {
        public APIDbContext(DbContextOptions<APIDbContext> options) : base(options)
        {
            try
            {
                if (!this.Database.CanConnect())
                {
                    throw new Exception("Não foi possível conectar ao banco de dados.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao conectar ao banco de dados: " + ex.Message);
                throw;
            }
        }

        public DbSet<Cliente> Clientes { get; set; }  // Adicione esta linha
        public DbSet<Profissional> Profissionais { get; set; }  // Adicione esta linha
        public DbSet<Servico> Servicos { get; set; }  // Adicione esta linha
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Vendas> Vendas { get; set; }
        public DbSet<ItensVenda> ItensVenda { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Fidelidade> Fidelidade { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Comissao> Comissoes { get; set; }
        public DbSet<EstoqueMovimentacao> EstoqueMovimentacao { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<NivelAcesso> NivelAcesso { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Salario> Salarios { get; set; }

    }

}
