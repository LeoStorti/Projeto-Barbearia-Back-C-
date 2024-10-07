using System;
using System.ComponentModel.DataAnnotations;

namespace APIBarbearia.Models
{
    public class Empresa
    {
        [Key]
        public int EmpresaId { get; set; }
        public string NomeEmpresa { get; set; }
        public string CNPJ { get; set; }
        public string Endereco { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
