using System.ComponentModel.DataAnnotations;

namespace LojaApi.Models
{
    public class Cliente
    {
        [Key] // Chave primária para o Entity Framework não dar erro
        public int Codigo { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
    }
}