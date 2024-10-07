using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Produto
    {
        [Key]
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; }
        public string Descricao { get; set; }
        public decimal PrecoVenda { get; set; }
        public int QuantidadeEmEstoque { get; set; }
        public string Categoria { get; set; }
    }
}
