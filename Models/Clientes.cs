using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório.")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;

        public DateTime? DataNascimento { get; set; }

        [Required(ErrorMessage = "Endereço é obrigatório.")]
        public string Endereco { get; set; } = string.Empty;

        public string Observacoes { get; set; } = string.Empty;
        public string Alergias { get; set; } = string.Empty;
    }
}