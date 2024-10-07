using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Vendas
    {
        [Key]
        public int VendaId { get; set; }
        public DateTime DataVenda { get; set; }
        public int? ClienteId { get; set; }
        public int? ProfissionalId { get; set; }
        public Cliente Cliente { get; set; }
        public decimal TotalVenda { get; set; }

        // Adicione o relacionamento com Servico
        public int? ServicoId { get; set; }  // Pode ser nulo se a venda não tiver serviço
        public Servico Servico { get; set; }  // Relação com o serviço

        // Relacionamento com produtos vendidos
        // public List<ItensVenda> ItensVenda { get; set; }
    }


}
