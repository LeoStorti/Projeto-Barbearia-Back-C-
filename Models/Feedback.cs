using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public int ServicoId { get; set; }
        public Servico Servico { get; set; }
        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }
        public int Avaliacao { get; set; } // 1 a 5 estrelas
        public string Comentario { get; set; }
        public DateTime DataFeedback { get; set; }
    }
}
