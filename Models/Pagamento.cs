using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Pagamento
    {
        [Key]
        public int PagamentoId { get; set; }
        public int VendaId { get; set; }
        public Vendas Venda { get; set; }
        public decimal ValorPago { get; set; }
        public DateTime DataPagamento { get; set; }
        public string FormaPagamento { get; set; } // Ex: Dinheiro, Cartão de Crédito, etc.
    }
}
