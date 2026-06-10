using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public int ServicoId { get; set; }
        public Servico Servico { get; set; } = null!;
        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; } = null!;
        public int Avaliacao { get; set; } // 1 a 5 estrelas
        public string Comentario { get; set; } = string.Empty;
        public DateTime DataFeedback { get; set; }
    }
}
