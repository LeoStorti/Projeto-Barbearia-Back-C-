using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Profissional
    {
        [Key]
        public int ProfissionalId { get; set; }
        public string Nome { get; set; }
        public string Especializacao { get; set; }
        public string Telefone { get; set; }
    }
}
