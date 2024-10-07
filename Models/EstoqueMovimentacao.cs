using APIBarbearia.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class EstoqueMovimentacao
    {
        [Key]
        public int MovimentacaoId { get; set; }
        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }
        public string TipoMovimentacao { get; set; } // Entrada ou Saída
        public int Quantidade { get; set; }
        public DateTime DataMovimentacao { get; set; }
    }
}
