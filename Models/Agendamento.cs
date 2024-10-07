using System;
using System.ComponentModel.DataAnnotations;

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
        public string Status { get; set; }
        public string Observacoes { get; set; }

        // Propriedades de navegação
        public Cliente Cliente { get; set; }
        public Profissional Profissional { get; set; }
        public Servico Servico { get; set; }
    }
}
