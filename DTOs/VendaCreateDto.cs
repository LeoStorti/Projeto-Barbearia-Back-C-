using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.DTOs
{
    public class VendaCreateDto
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int ProfissionalId { get; set; }

        [Required]
        public int ServicoId { get; set; }

        public DateTime? DataVenda { get; set; }

        public decimal? TotalVenda { get; set; }
    }
}
