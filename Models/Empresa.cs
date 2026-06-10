using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Empresa
    {
        [Key]
        public int EmpresaId { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
    }
}
