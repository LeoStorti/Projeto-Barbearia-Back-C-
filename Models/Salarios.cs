// Models/Salario.cs
using APIBarbearia.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIBarbearia.Models
{
    public class Salario
    {
        [Key]
        public int SalarioId { get; set; }

        [ForeignKey("Profissional")]
        public int ProfissionalId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SalarioFixo { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; } // Pode ser nulo, caso o salário esteja ativo indefinidamente

        // Relacionamento com a tabela de Profissionais
        public virtual Profissional Profissional { get; set; }
    }
}
