using System.ComponentModel.DataAnnotations;

namespace LojaApi.Models
{
    public class Fornecedor
    {
        [Key] 
        public int Codigo { get; set; }
        public string Nome { get; set; }
        public string Cnpj { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
    }
}