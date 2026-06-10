using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string NivelAcesso { get; set; } = string.Empty; // Admin, Profissional
        public int EmpresaId { get; set; }
    }
}
