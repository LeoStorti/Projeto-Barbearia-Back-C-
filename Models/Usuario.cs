using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string NivelAcesso { get; set; } // Admin, Profissional
    }
}
