using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Servico
    {
        [Key]
        public int ServicoId { get; set; }
        public string NomeServico { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public int Duracao { get; set; } // Duração em minutos
        public string Categoria { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
    }
}
