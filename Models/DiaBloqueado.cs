using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class DiaBloqueado
    {
        [Key]
        public int DiaBloqueadoId { get; set; }

        public int EmpresaId { get; set; }

        public DateTime Data { get; set; }

        public string Motivo { get; set; } = string.Empty;
    }
}
