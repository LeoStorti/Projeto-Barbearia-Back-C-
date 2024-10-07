using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Fidelidade
    {
        [Key]
        public int FidelidadeId { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public int PontosAcumulados { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}
