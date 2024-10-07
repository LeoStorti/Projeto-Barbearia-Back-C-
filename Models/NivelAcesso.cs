using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class NivelAcesso
    {
        [Key]
        public int NivelAcessoId { get; set; }
        public string Descricao { get; set; }
        public string Permissoes { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
