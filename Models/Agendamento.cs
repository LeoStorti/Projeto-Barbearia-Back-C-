using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace APIBarbearia.Models
{
    public class Agendamento
    {
        [Key]
        public int AgendamentoId { get; set; }
        public int ClienteId { get; set; }
        public int ProfissionalId { get; set; }
        public int ServicoId { get; set; }
        public DateTime DataHora { get; set; }
        public string? Status { get; set; }
        public string? Observacoes { get; set; }

        // Propriedades de navegação
        [ValidateNever]
        public Cliente? Cliente { get; set; }

        [ValidateNever]
        public Profissional? Profissional { get; set; }

        [ValidateNever]
        public Servico? Servico { get; set; }
    }
}
