using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class ItensVenda
    {
        [Key]
        public int ItemVendaId { get; set; }
        //public Venda venda { get; set; }

        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }

        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}
