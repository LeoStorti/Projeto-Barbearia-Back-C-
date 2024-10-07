using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Comissao
    {
        [Key]
        public int ComissaoId { get; set; }
        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }
        public int VendaId { get; set; }
        public Vendas Venda { get; set; }
        public decimal ValorComissao { get; set; }
        public DateTime DataComissao { get; internal set; }
    }
}
