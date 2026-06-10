using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Produto
    {
        [Key]
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal PrecoVenda { get; set; }
        public int QuantidadeEmEstoque { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
    }
}
