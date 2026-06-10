using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Profissional
    {
        [Key]
        public int ProfissionalId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Especializacao { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;

        // A tabela atual exige EmpresaId (NOT NULL + FK).
        public int EmpresaId { get; set; }

        public decimal? Salario { get; set; }
        public string? Email { get; set; }
        public string? FotoUrl { get; set; }
        public DateTime? FotoAtualizadaEm { get; set; }
    }
}
