using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Endereco { get; set; }
        public string Observacoes { get; set; }
        public string Alergias { get; set; }
    }
}