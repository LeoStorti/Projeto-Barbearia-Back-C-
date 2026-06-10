using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Comissao
    {
        [Key]
        public int ComissaoId { get; set; }
        public int EmpresaId { get; set; }
        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; } = null!;
        public int VendaId { get; set; }
        public Vendas Venda { get; set; } = null!;
        public decimal ValorComissao { get; set; }
        public DateTime DataComissao { get; internal set; }
    }
}
